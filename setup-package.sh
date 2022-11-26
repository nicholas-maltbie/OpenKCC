#!/bin/bash
# Usage: 
#   setup-package.sh -p $package_path [-t $tag] [-s $sample1,$sample2,...]
#
# Creates a package of the unity files at the `$package_path` folder
# and will include the files form "./Assets/Samples" in the export
# under the path "Samples~" to follow unity convention.
#
# Will also preserve any git-lfs links for files to avoid
# duplicating assets in the repo.
#
# Arguments:
#
#   -p $package_path - Required, path to package folder of project,
#       Should be something like "Packages/com.companyname.packagename"
#   [-t $tag] - Optional, tag version to checkout before building
#       package. If provided, will create a new branch with
#       the name pattern "release/$tag"
#   [-s $sample1,$sample2,...] - Optional, comma separated list
#       of samples to copy from the "./Assets/Samples" folder. If none
#       is provided, then will select all the samples found in the Samples
#       folder (if any).

sample_path="./Assets/Samples/"
current_branch=$(git rev-parse --abbrev-ref HEAD)
current_sha=$(git rev-parse --verify HEAD)
previous_githooks=$(git config core.hooksPath)

if [ $# -eq 0 ]
then
  show_help
  exit 0
fi

show_help () {
  echo "Usage:"
  echo "  setup-package.sh -p \$package_path [-t \$tag] [-s \$sample1,\$sample2,...]"
  echo ""
  echo "  Creates a package of the unity files at the `\$package_path` folder"
  echo "  and will include the files form \"$sample_path\" in the export"
  echo "  under the path \"Samples~\" to follow unity convention."
  echo ""
  echo "  Will also preserve any git-lfs links for files to avoid"
  echo "  duplicating assets in the repo."
  echo "  Arguments:"
  echo ""
  echo "    -p \$package_path - Required, path to package folder of project,"
  echo "        Should be something like \"Packages/com.companyname.packagename\""
  echo "    [-t \$tag] - Optional, tag version to checkout before building"
  echo "        package. If provided, will create a new branch with"
  echo "        the name pattern \"release/\$tag\""
  echo "    [-s \$sample1,\$sample2,...] - Optional, comma separated list"
  echo "        of samples to copy from the \"$sample_path\" folder. If none"
  echo "        is provided, then will select all the samples found in the Samples"
  echo "        folder (if any)."
}

while getopts "p:t:s:h" opt; do
  case $opt in
    p) package_path=$OPTARG      ;;
    t) selected_tag=$OPTARG      ;;
    s) selected_samples=$OPTARG  ;;
    h) show_help;exit 0          ;;
    ?)
      echo "Invalid option: -${OPTARG}."
      echo
      show_help
      exit 1
      ;;
  esac
done

if [ -z "$package_path" ]
then
  echo "Error: Did not provide package path (-p) argument" 1>&2
  show_help
  exit 1
fi

if [ ! -d "$package_path" ]
then
  echo "Error: Did not find package at path: \"$package_path\"" 1>&2
fi

# Checkout specific tag if one is provided
if [ ! -z "$selected_tag" ]
then
  echo "Attempting to make release for tag $selected_tag"
  if git rev-parse "$selected_tag" >/dev/null 2>&1; then
  #   git config core.hooksPath .git/hooks
    echo "Found tag $selected_tag, checking out changes"
  #   git checkout "$selected_tag"
  else
    echo "Error: Tag $selected_tag does not exist, aborting changes" 1>&2
    exit 1
  fi
fi

# Find the samples listed by the user
if [ ! -z "$selected_samples" ]
then
  IFS=',' read -ra samples_array <<< "$selected_samples"
  samples_array=("${samples_array[@]/#/$sample_path}")

  for sample in "${samples_array[@]}"
  do
    if [ ! -d "$sample" ]
    then
      echo "Error: Did not find sample at path: \"$sample\"" 1>&2
      exit 1
    else
      echo "Found provided sample at path: \"$sample\""
    fi
  done
else
  echo "No samples selected, searching for samples in $sample_path"
  samples_array=()
  while IFS=  read -r -d $'\0'; do
      samples_array+=("$REPLY")
  done < <(find $sample_path -maxdepth 1 -mindepth 1 -type d -print0)

  echo "Found samples: '${samples_array[@]}'"
fi

# Check if there are changes
if [ ! -z "$(git status --porcelain)" ]; then
  echo "Found unstaged changes:"
  git status --porcelain
  echo "Will not setup package if branch has changes" 1>&2
  exit 1
fi

# Move to temporary branch
exists=`git show-ref refs/heads/temp-branch`
if [ -n "$exists" ]; then
  git branch -D temp-branch
fi
git checkout -b temp-branch

user_email=$(git config --global user.email)
user_name=$(git config --global user.name)

if [ -z "$user_email" ]
then
  git config --global user.email "github-actions[bot]@users.noreply.github.com"
fi

if [ -z "$user_name" ]
then
  git config --global user.name "github-actions[bot]"
fi

git lfs install


# Sets up unity package samples
for sample in "${samples_array[@]}"
do
  sample_name=${sample#"$sample_path"}
  dest="$package_path/Samples/$sample_name"
  echo "Moving sample at path \"$sample\" to \"$dest\""

  # Setup sample directory
  mkdir -p "$(dirname $dest)"
  git mv "$sample.meta" "$dest.meta"
  git mv "$sample/" "$dest/"
done
echo "git commit -m \"Moved $sample_path to $package_path/Samples\""
git commit -m "Moved $sample_path to $package_path/Samples"

# Reset all other changes
git rm -rf .
git checkout HEAD -- "$package_path"

# Keep .gitattributes for lfs files
git checkout HEAD -- .gitattributes

git commit -m "Filtered for only package files"

# Move files from $package_path to root folder
git mv $package_path/* .
git commit -m "Moved files from \"$package_path/*\" to root"

# Push changes to repo if tag was provided
if [ ! -z "$selected_tag" ]
then
  # Push changes to original repo
  exists=`git show-ref refs/heads/release/$selected_tag`
  if [ -n "$exists" ]; then
    git branch -D "release/$selected_tag"
  fi
  git branch -m "release/$selected_tag"
  git push --set-upstream origin "release/$selected_tag" --force

  git config core.hooksPath "$previous_githooks"
  # Cleanup any files in the repo we don't care about
  git checkout . && git clean -xdf .
  git checkout "$current_sha" && git checkout "$current_branch"
fi
