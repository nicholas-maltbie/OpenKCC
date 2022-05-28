@REM Setup files for website
echo "Setting up website and copying files"
copy %~dp0\..\README.md %~dp0\index.md
copy %~dp0\..\LICENSE.txt %~dp0\LICENSE.txt
xcopy /E /S /Y %~dp0\..\Demo %~dp0\Demo\

@REM Generate website with docfx
echo "Building code metadata"
docfx metadata %~dp0\docfx.json --warningsAsErrors --logLevel verbose --force

echo "Generating website"
docfx build %~dp0\docfx.json --warningsAsErrors --logLevel verbose
