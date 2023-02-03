function currentVersion()
{
    return document.getElementById("version").getAttribute("version") || "latest"
}

function versionList()
{
    versions = document.getElementById("version").getAttribute("versionlist") || ""
    return (versions).split(",")
}

function selectVersion() {
    selectedVersion = document.getElementById("version-selector").value
    prefix = currentVersion() == "latest" ? "/../" : "/../../"
    root = new URL(document.location + prefix + document.querySelector('meta[property="docfx:rel"]').content).href

    versionPath = selectVersion == "latest" ? "" : selectVersion
    window.location.href = root + "/" + versionPath
}