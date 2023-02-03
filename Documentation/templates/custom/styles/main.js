function currentVersion()
{
    return document.getElementById("version").version || "latest"
}

function versionList()
{
    versions = document.getElementById("version").versionList || ""
    return (versions).split(",")
}

function selectVersion() {
    var version = document.getElementById("version-selector").value;
    console.log("Selected version: " + version);
}