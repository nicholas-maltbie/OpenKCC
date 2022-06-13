# Sets up unity package samples
mv ./Assets/Samples ./Packages/com.nickmaltbie.openkcc/Samples~

git add ./Packages/com.nickmaltbie.openkcc/Samples~

git lfs install

git config --global user.email "github-actions[bot]@users.noreply.github.com"
git config --global user.name "github-actions[bot]"

git commit -m "Moved ./Assets/Samples to ./Packages/com.nickmaltbie.openkcc/Samples~"
