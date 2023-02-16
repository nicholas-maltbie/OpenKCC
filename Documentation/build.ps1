# Setup files for website
$dir = $PSScriptRoot
$project_dir = $(Get-Item $dir).Parent

$current_branch="$(git rev-parse --abbrev-ref HEAD)"
$current_sha="$(git rev-parse --verify HEAD)"
$location=Get-Location

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

foreach ($tag in $(git tag | Sort-Object -Descending { $_.substring(1) -as [version] }))
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
Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.cinemachine\CHANGELOG.md" "$dir\changelog\CHANGELOG.cinemachine.md"
Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md" "$dir\changelog\CHANGELOG.netcode.md"

$paramFile = Get-Content "$dir\docfx.json" | ConvertFrom-Json
$paramFile.build.globalMetadata | Add-Member -name "_version" -value "latest" -MemberType NoteProperty -Force
$paramFile.build.globalMetadata | Add-Member -name "_versionList" -value "$([System.string]::Join(",", $versions))" -MemberType NoteProperty -Force
$paramFile | ConvertTo-Json -Depth 16 | Set-Content "$dir\docfx.json"

Write-Host "Building code metadata"
dotnet docfx metadata "$dir\docfx.json" --force

Write-Host "Generating website"
dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom"

# Setup documentation for each version of the api
foreach ($tag in $versions)
{
    # Reset any changes and checkout tag
    git reset . > $null
    git checkout . > $null
    git checkout $tag > $null

    # ensure docfx is installed
    dotnet tool install docfx --version 2.60.2 > $null

    Write-Host "Setting up website and copying files"
    
    if (!(Test-Path "$dir"))
    {
        New-Item -Path "$dir" -ItemType Directory > $null
    }
    if (!(Test-Path "$dir\changelog"))
    {
        New-Item -Path "$dir\changelog" -ItemType Directory > $null
    }

    Copy-Item -Force "$project_dir\README.md" "$dir\index.md" > $null

    if (Test-Path "$project_dir\LICENSE.txt")
    {
        Copy-Item -Force "$project_dir\LICENSE.txt" "$dir\LICENSE.txt" > $null
    }
    if (Test-Path "$project_dir\Demo")
    {
        Copy-Item -Recurse -Force "$project_dir\Demo" "$dir\Demo\" > $null
    }
    if (Test-Path "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md")
    {
        Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md" "$dir\changelog\CHANGELOG.md" > $null
    }
    if (Test-Path "$project_dir\Packages\com.nickmaltbie.openkcc.cinemachine\CHANGELOG.md")
    {
        Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.cinemachine\CHANGELOG.md" "$dir\changelog\CHANGELOG.cinemachine.md" > $null
    }
    if (Test-Path "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md")
    {
        Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md" "$dir\changelog\CHANGELOG.netcode.md" > $null
    }

    if (Test-Path "$dir\docfx.json")
    {
        # Change the dest in the docfx.json file
        $paramFile = Get-Content "$dir\docfx.json" | ConvertFrom-Json
        $paramFile.build | Add-Member -name "dest" -value "$project_dir/_site/$tag" -MemberType NoteProperty -Force
        $paramFile.build.globalMetadata | Add-Member -name "_version" -value "$tag" -MemberType NoteProperty -Force
        $paramFile.build.globalMetadata | Add-Member -name "_versionList" -value "$([System.string]::Join(",", $versions))" -MemberType NoteProperty -Force
        $paramFile | ConvertTo-Json -Depth 16 | Set-Content "$dir\docfx.json"

        # In previous versions of website, I built to /latest/api
        # let's just build to api fora ll these vesrions
        foreach ($filePath in @("$dir\docfx.json", "$dir\toc.yml"))
        {
            (Get-Content $filePath).Replace("latest/api","api") | Set-Content $filePath
        }

        # Generate website with docfx
        Write-Host "Building code metadata"
        dotnet docfx metadata "$dir\docfx.json" --force
    
        # Copy tempalte from main branch to here
        if (!(Test-Path "$dir\templates\custom"))
        {
            New-Item -Path "$dir\templates\custom" -ItemType Directory > $null
        }
        git checkout $current_sha -- "$dir/templates/custom"

        Write-Host "Generating website"
        Write-Host "dotnet docfx build `"$dir\docfx.json`" -t `"default,$dir\templates\custom`" -o `"_site\$tag`""
        dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom"
        
        # Reset template changes
        Remove-Item -LiteralPath "$dir\templates\custom" -Force -Recurse > $null
        git reset "$dir/templates/custom" > $null
    }

    # Undo any changes made in previous iteration
    git reset .
    git checkout .
    git clean -xdf --exclude "_site" > $null
}

# Do some work to cleanup duplicate files in exported _site folder to
# reduce size of export.
$Duplicates = Get-ChildItem -Path _site -File -Recurse | Get-FileHash | Group-Object -Property Hash | Where-Object Count -gt 1

foreach ($d in $Duplicates)
{
    # Replace all files with symlink to file with shortest path
    $shortest = $d.Group.Path | Sort-Object length -desc | Select-Object -last 1

    foreach ($path in $d.Group.Path)
    {
        if ($path -ne $shortest)
        {
            Remove-Item $path
            New-Item -ItemType SymbolicLink -Path $path -Target $shortest > $null
        }
    }
}

git checkout "$current_sha"
git checkout "$current_branch"
Set-Location $location
