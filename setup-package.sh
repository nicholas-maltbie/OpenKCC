# Sets up unity package samples
mv ./Assets/Samples ./Packages/com.nickmaltbie.openkcc/Samples~

# Add new samples to git repo
git add ./Assets/Samples ./Packages/com.nickmaltbie.openkcc/Samples~

git lfs install

git config --global user.email "github-actions[bot]@users.noreply.github.com"
git config --global user.name "github-actions[bot]"

git commit -m "Moved ./Assets/Samples to ./Packages/com.nickmaltbie.openkcc/Samples~"

# Cleanup any files not part of the package
rm -rf !(./Packages/com.nickmaltbie.openkcc)

mv ./Packages/com.nickmaltbie.openkcc .

git add .

git commit -m "Moved ./Packages/com.nickmaltbie.openkcc to base dir and removed other assets"
