@REM Setup files for website
echo Setting up website and copying files
copy %~dp0\..\README.md %~dp0\index.md
copy %~dp0\..\LICENSE.txt %~dp0\LICENSE.txt
copy %~dp0\..\Packages\com.nickmaltbie.openkcc\CHANGELOG.md %~dp0\changelog\CHANGELOG.md
xcopy /E /S /Y %~dp0\..\Demo %~dp0\Demo\

@REM Generate website with docfx
echo Building code metadata
docfx metadata %~dp0\docfx.json --warningsAsErrors --logLevel verbose --force && (
    echo Successfuly generated metadata for C# code formatting
) || (
    echo Could not properly generate metadata for C# code formatting
    exit /b 1
)

echo Generating website
docfx build %~dp0\docfx.json -t default,%~dp0\templates\custom --warningsAsErrors --logLevel verbose && (
    echo Successfuly generated website for documentation
) || (
    echo Could not properly website for documentation
    exit /b 1
)
