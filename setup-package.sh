# Sets up unity package samples
git mv ./Assets/Samples ./Packages/com.nickmaltbie.openkcc/Samples~

git lfs install

git config --global user.email "github-actions[bot]@users.noreply.github.com"
git config --global user.name "github-actions[bot]"

git commit -m "Moved ./Assets/Samples to ./Packages/com.nickmaltbie.openkcc/Samples~"

# Cleanup any files not part of the package
git rm -rf .
git reset ./Packages/com.nickmaltbie.openkcc
git mv ./Packages/com.nickmaltbie.openkcc/ .

git commit -m "Moved ./Packages/com.nickmaltbie.openkcc to base dir and removed other assets"
