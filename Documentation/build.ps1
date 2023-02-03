# Setup files for website
$dir = $PSScriptRoot
$project_dir = $(Get-Item $dir).Parent

$current_branch="$(git rev-parse --abbrev-ref HEAD)"
$current_sha="$(git rev-parse --verify HEAD)"

if ("$(git status --porcelain)" -ne "")
{
    throw "Found unstanged git changes, exiting"
}

# Cleanup any previous documentation
if (Test-Path "_site")
{
    Remove-Item -LiteralPath "_site" -Force -Recurse > $null
}

# List of supported versions
$versions = @()

foreach ($tag in $(git tag))
{
    # Check if file exists for branch
    if ($(git cat-file -t "$($tag):Documentation/docfx.json") -eq "blob")
    {
        $versions += $tag
    }
}

Write-Host "Setting up website and copying files"
Copy-Item -Force "$project_dir\README.md" "$dir\index.md"
Copy-Item -Force "$project_dir\LICENSE.txt" "$dir\LICENSE.txt"
Copy-Item -Recurse -Force "$project_dir\Demo" "$dir\Demo\"
Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md" "$dir\changelog\CHANGELOG.md"
Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md" "$dir\changelog\CHANGELOG.netcode.md"

$paramFile = Get-Content "$dir\docfx.json" | ConvertFrom-Json
$paramFile.build.globalMetadata | Add-Member -name "_version" -value "latest" -MemberType NoteProperty -Force
$paramFile.build.globalMetadata | Add-Member -name "_versionList" -value "$([System.string]::Join(",", $list))" -MemberType NoteProperty -Force
$paramFile | ConvertTo-Json -Depth 16 | Set-Content "$dir\docfx.json"

Write-Host "Building code metadata"
dotnet docfx metadata "$dir\docfx.json" --force

Write-Host "Generating website"
dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom"

# Setup documentation for each version of the api
foreach ($tag in $versions)
{
    git checkout $tag

    # ensure docfx is installed
    dotnet tool install docfx

    Write-Host "Setting up website and copying files"
    
    if (!(Test-Path "$dir"))
    {
        New-Item -Path "$dir" -ItemType Directory > $null
    }
    if (!(Test-Path "$dir\changelog"))
    {
        New-Item -Path "$dir\changelog" -ItemType Directory > $null
    }

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
        # Change the dest in the docfx.json file
        $paramFile = Get-Content "$dir\docfx.json" | ConvertFrom-Json
        $paramFile.build | Add-Member -name "dest" -value "$project_dir/_site/$tag" -MemberType NoteProperty -Force
        $paramFile.build.globalMetadata | Add-Member -name "_version" -value "$tag" -MemberType NoteProperty -Force
        $paramFile.build.globalMetadata | Add-Member -name "_versionList" -value "$([System.string]::Join(",", $list))" -MemberType NoteProperty -Force
        $paramFile | ConvertTo-Json -Depth 16 | Set-Content "$dir\docfx.json"

        # Generate website with docfx
        Write-Host "Building code metadata"
        dotnet docfx metadata "$dir\docfx.json" --force
    
        # Copy tempalte from main branch to here
        git checkout $current_sha -- "$dir/templates/custom"

        Write-Host "Generating website"
        Write-Host "dotnet docfx build `"$dir\docfx.json`" -t `"default,$dir\templates\custom`" -o `"_site\$tag`""
        dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom"
        
        # Reset template changes
        git reset "$dir/templates/custom"
    }

    # Undo any changes made in previous iteration
    git reset .
    git checkout .
    git clean -xdf Documentation Assets Packages
}

git checkout "$current_sha" && git checkout "$current_branch"
