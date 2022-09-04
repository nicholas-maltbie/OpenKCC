# Usage: 
#   setup-package.sh $package_path [$tag]
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
#   $package_path - Required, path to package folder of project,
#       Should be something like "Packages/com.companyname.packagename"
#   [$tag] - Optional, tag version to checkout before building
#       package. If provided, will create a new branch with
#       the name pattern "release/$tag"

current_branch=$(git rev-parse --abbrev-ref HEAD)
current_sha=$(git rev-parse --verify HEAD)
previous_githooks=$(git config core.hooksPath)

export_path=$1

if [ -z "$export_path" ]
then
  echo "Error: Did not provide export path as first argument" 1>&2
  exit 1
fi

if [ ! -d "$export_path" ]
then
  echo "Error: Did not find package at path: \"$export_path\"" 1>&2
fi

# Checkout specific tag if one is provided
selected_tag=$2
if [ ! -z "$selected_tag" ]
then
  echo "Attempting to make release for tag $selected_tag"
  if git rev-parse "$selected_tag" >/dev/null 2>&1; then
    git config core.hooksPath .git/hooks
    echo "Found tag $selected_tag, checking out changes"
    git checkout "$selected_tag"
  else
    echo "Error: Tag $selected_tag does not exist, aborting changes" 1>&2
    exit 1
  fi
fi

# Check if there are changes
if [ `git status --porcelain` ]; then
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
git mv "./Assets/Samples" "$export_path/Samples"
git commit -m "Moved ./Assets/Samples/ to $export_path/Samples"

# Reset all other changes
git rm -rf .
git checkout HEAD -- "$export_path"

# Keep .gitattributes for lfs files
git checkout HEAD -- .gitattributes

git commit -m "Filtered for only package files"

# Move files from _keep to root folder
git mv $export_path/* .

git commit -m "Setup files for release"

git mv "Samples" "Samples~"
git commit -m "Renamed Samples to Samples~"

# Reset some changes
git checkout . && git clean -xdf .

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
