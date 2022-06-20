current_branch=$(git rev-parse --abbrev-ref HEAD)
current_sha=$(git rev-parse --verify HEAD)
previous_githooks=$(git config core.hooksPath)

export_path="./Packages/com.nickmaltbie.openkcc"

# Checkout specific tag if one is provided
if [ ! -z "$1" ]
then
  echo "Attempting to make release for tag $1"
  if git rev-parse "$1" >/dev/null 2>&1; then
    git config core.hooksPath .git/hooks
    echo "Found tag $1, checking out changes"
    git checkout "$1"
  else
    echo "Tag $1 does not exist, aborting changes" 1>&2
    exit 1
  fi
fi

# Check if there are changes
if [[ `git status --porcelain` ]]; then
  echo "Will not setup package if branch has changes" 1>&2
  exit 1
fi

# Move to temporary branch
git branch -D temp-branch
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
if [ ! -z "$1" ]
then
  # Push changes to original repo
  git branch -D "release/$1"
  git branch -m "release/$1"
  git push --set-upstream origin "release/$1" --force

  git config core.hooksPath "$previous_githooks"
  # Cleanup any files in the repo we don't care about
  git checkout . && git clean -xdf .
  git checkout "$current_sha" && git checkout "$current_branch"
fi
