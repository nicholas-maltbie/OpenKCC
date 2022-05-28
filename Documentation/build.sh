BASEDIR=$(dirname "$0")

# Setup files for website
cp $BASEDIR/../README.md $BASEDIR/index.md
cp $BASEDIR/../LICENSE.txt $BASEDIR/LICENSE.txt
cp -r $BASEDIR/../Demo $BASEDIR/Demo

# Generate website with docfx
docfx metadata $BASEDIR/docfx.json --warningsAsErrors --logLevel verbose --force
docfx build $BASEDIR/docfx.json --warningsAsErrors --logLevel verbose
