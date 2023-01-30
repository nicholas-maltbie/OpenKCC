# Setup files for website
$dir = $PSScriptRoot
$project_dir = $(Get-Item $dir).Parent

if ("$(git status --porcelain)" -ne "")
{
    throw "Found unstanged git changes, exiting"
}

# Cleanup any previous documentation
if (Test-Path "_site")
{
    Remove-Item -LiteralPath "_site" -Force -Recurse > $null
}

# Write-Host "Setting up website and copying files"
# Copy-Item -Force "$project_dir\README.md" "$dir\index.md"
# Copy-Item -Force "$project_dir\LICENSE.txt" "$dir\LICENSE.txt"
# Copy-Item -Recurse -Force "$project_dir\Demo" "$dir\Demo\"
# Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md" "$dir\changelog\CHANGELOG.md"
# Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md" "$dir\changelog\CHANGELOG.netcode.md"

# Write-Host "Building code metadata"
# dotnet docfx metadata "$dir\docfx.json" --force

# Write-Host "Generating website"
# dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom" -o "_site"

# Setup documentation for each version of the api
foreach ($tag in $(git tag))
{
    git checkout $tag

    # ensure docfx is installed
    dotnet tool install docfx

    Write-Host "Setting up website and copying files"
    New-Item -Path "$dir" -ItemType Directory > $null
    New-Item -Path "$dir" -ItemType Directory > $null
    Copy-Item -Force "$project_dir\README.md" "$dir\index.md"

    if (Test-Path "$project_dir\LICENSE.txt")
    {
        Copy-Item -Force "$project_dir\LICENSE.txt" "$dir\LICENSE.txt"
    }
    if (Test-Path "$project_dir\Demo")
    {
        Copy-Item -Recurse -Force "$project_dir\Demo" "$dir\Demo\"
    }
    if (Test-Path "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md")
    {
        Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md" "$dir\changelog\CHANGELOG.md"
    }
    if (Test-Path "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md")
    {
        Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md" "$dir\changelog\CHANGELOG.netcode.md"
    }

    if (Test-Path "$dir\docfx.json")
    {
        # Generate website with docfx
        Write-Host "Building code metadata"
        dotnet docfx metadata "$dir\docfx.json" --force
    
        Write-Host "Generating website"
        Write-Host "dotnet docfx build `"$dir\docfx.json`" -t `"default,$dir\templates\custom`" -o `"_site\$tag`""
        dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom" -o "_site\$tag"
    }

    # Undo any changes made in previous iteration
    git reset .
    git checkout .
    git clean -xdf Documentation Assets Packages
}
