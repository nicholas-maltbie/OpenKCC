BASEDIR=$(dirname "$0")

# Setup files for website
cp $BASEDIR/../README.md index.md

# Generate website with docfx
docfx metadata $BASEDIR/docfx.json --warningsAsErrors --logLevel verbose
docfx build $BASEDIR/docfx.json --warningsAsErrors --logLevel verbose
