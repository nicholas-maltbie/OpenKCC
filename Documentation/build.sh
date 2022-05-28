BASEDIR=$(dirname "$0")

# Setup files for website
echo "Setting up website and copying files"
cp $BASEDIR/../README.md $BASEDIR/index.md
cp $BASEDIR/../LICENSE.txt $BASEDIR/LICENSE.txt
cp -r $BASEDIR/../Demo $BASEDIR/Demo

# Generate website with docfx
echo "Building code metadata"
docfx metadata $BASEDIR/docfx.json --warningsAsErrors --logLevel verbose --force

echo "Generating website"
docfx build $BASEDIR/docfx.json --warningsAsErrors --logLevel verbose
