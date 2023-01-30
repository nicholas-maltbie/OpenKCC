# Setup files for website
$dir = $PSScriptRoot
$project_dir = $(Get-Item $dir).Parent

if ("$(git status --porcelain)" -ne "")
{
    throw "Found unstanged git changes, exiting"
}

$doc_paths = @("Packages", "Assets/Samples", "Documentation/manual", "Documentation/resources", "README.md", "Demo", "LICENSE.txt")

# Cleanup any previous documentation
if (Test-Path "$dir\versions")
{
    Remove-Item "$dir\versions" -Force -Recurse > $null
}

# Setup documentation for each version of the api
foreach ($tag in @('v0.0.61', 'v0.1.0', 'v0.1.2', 'v1.0.0', 'v1.1.0', 'v1.2.0'))
{
    Write-Host "Setting up docs for version '$tag'"
    New-Item -Path "$dir\versions\$tag" -ItemType Directory > $null

    # Make a small toc.yml for each version
    Add-Content -Path "$dir\versions\$tag\toc.yml" -value @(
      "- name: Manual",
      "  href: manual/",
      "- name: Scripting API",
      "  href: api/")

    foreach ($path in $doc_paths)
    {
        if (Test-Path "$project_dir\$path")
        {
            Remove-Item "$project_dir\$path" -Force -Recurse > $null
        }

        # Check if resource exists for path
        if (git cat-file -t "$($tag):$($path)" && $true || $false)
        {
            Write-Host "Setting up resources for tag '$tag' at path: '$project_dir\$path'"
            git checkout "$tag" -- "$project_dir/$path" > $null

            if (Test-Path "$project_dir\$path")
            {
                $parent = "$(Split-Path -parent "$dir\versions\$tag\$path")"
                if (!$(Test-Path "$parent")) 
                {
                    New-Item -Path "$parent" -Type Directory > $null
                }

                Move-Item "$project_dir\$path" "$dir\versions\$tag\$path" > $null

                # If the file name is README.md, rename to index.md
                if ("$path" -eq "README.md")
                {
                    Rename-Item -Path "$dir\versions\$tag\$path" -NewName "$dir\versions\$tag\index.md"
                }
            }
        }
    }
}

# Restore packages and samples file from master branch
foreach ($path in $doc_paths)
{
    git checkout "$project_dir/$path" > $null

    if (Test-Path "$project_dir\$path")
    {
        git reset "$project_dir/$path" > $null
        Remove-Item -LiteralPath "$project_dir\$path" -Force -Recurse > $null
    }

    git checkout "$project_dir/$path" > $null
}

Write-Host "Setting up website and copying files"
Copy-Item -Force "$project_dir\README.md" "$dir\index.md"
Copy-Item -Force "$project_dir\LICENSE.txt" "$dir\LICENSE.txt"
Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc\CHANGELOG.md" "$dir\changelog\CHANGELOG.md"
Copy-Item -Force "$project_dir\Packages\com.nickmaltbie.openkcc.netcode\CHANGELOG.md" "$dir\changelog\CHANGELOG.netcode.md"
Copy-Item "$project_dir\Demo" "$dir\Demo\"-Recurse -Force 

# Generate website with docfx
Write-Host "Building code metadata"
dotnet docfx metadata "$dir\docfx.json" --force && (
    Write-Host "Successfuly generated metadata for C# code formatting"
) || (
    throw "Could not properly generate metadata for C# code formatting"
)

Write-Host "Generating website"
dotnet docfx build "$dir\docfx.json" -t "default,$dir\templates\custom" && (
    Write-Host "Successfuly generated website for documentation"
) || (
    throw "Could not properly website for documentation"
)
