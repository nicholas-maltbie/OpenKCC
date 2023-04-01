# OpenKCC

This project is a sample of the Open Kinematic Character Controller.
A Kinematic Character Controller (KCC) provides a
way to control a character avatar as a kinematic object that will interact with
the environment.

OpenKCC is an open source project hosted at
[https://github.com/nicholas-maltbie/OpenKCC](https://github.com/nicholas-maltbie/OpenKCC)

This is an open source project licensed under a [MIT License](LICENSE.txt).
Feel free to use a build of the project for your own work. If you see an error
in the project or have any suggestions, write an issue or make a pull request,
I'll happy include any suggestions or ideas into the project.

[![Designing Character Controllers Video Introduction](Demo/sample-kcc.gif)](https://youtu.be/Hv4CQMCxSWE)

You can see a demo of the project running here:
[https://nickmaltbie.com/OpenKCC/](https://nickmaltbie.com/OpenKCC/).
The project hosted on the website is up to date with the most recent
version on the `main` branch of this github repo
and is automatically deployed with each update to the codebase.

## Installation

Make sure to add the required dependencies to your project

* [com.unity.inputsystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)
  version 1.0.0 or newer
* [com.unity.textmeshpro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)
  version 3.0.0 or newer
* [com.nickmaltbie.screenmanager](https://nickmaltbie.com/ScreenManager/docs/index.html)
  version 3.0.0 or newer
* [com.nickmaltbie.statemachineunity](https://nickmaltbie.com/StateMachineUnity/docs/index.html)
  version 1.1.0 or newer
* [com.nickmaltbie.testutilsunity](https://nickmaltbie.com/TestUtilsUnity/docs/index.html)
  version 0.0.2 or newer

In order to use the samples in the project, make sure to also add the following
projects to your project.

Install the latest version of the project by importing a project via git
at this URL:
`git+https://github.com/nicholas-maltbie/OpenKCC.git#release/latest`

If you want to reference a specific tag of the project such as version `v1.0.1`,
add a `#release/v1.0.1` to the end of the git URL to download the package
from th auto-generated branch for that release. An example of importing `v1.0.1`
would look like this:
`git+https://github.com/nicholas-maltbie/openkcc.git#release/v1.0.1`.

To use the latest release, simply reference:

```text
git+https://github.com/nicholas-maltbie/openkcc.git#release/latest
```

For a full list of all tags, check the [OpenKCC Tags](https://github.com/nicholas-maltbie/OpenKCC/tags)
list on github. I will usually associated a tag with each release of the project.

_Note_: before I started using the package format for the project, I manually
released a unity package you needed to import. Any version before `v1.0.0`
will not work to import the project.

If you do not include a tag, this means that your project will update whenever
you reimport from main. This may cause some errors or problems due to
experimental or breaking changes in the project.

You can also import the project via a tarball if you download the source
code and extract it on your local machine. Make sure to import
via the package manifest defined at `Packages\com.nickmaltbie.openkcc\package.json`
within the project.

For more details about installing a project via git, see unity's documentation
on [Installing form a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html#:~:text=%20Select%20Add%20package%20from%20git%20URL%20from,repository%20directly%20rather%20than%20from%20a%20package%20registry.).

### Scoped Registry Install

If you wish to install the project via a
[Scoped Registry](https://docs.unity3d.com/Manual/upm-scoped.html)
and npm, you can add a scoped registry to your project from all of the
`com.nickmaltbie` packages like this:

```json
"scopedRegistries": [
  {
    "name": "nickmaltbie",
    "url": "https://registry.npmjs.org",
    "scopes": [
      "com.nickmaltbie"
    ]
  }
]
```

Then, if you want to reference a version of the project, you simply
need to include the dependency with a version string and the unity package
manager will be able to download it from the registry at
`https://registry.npmjs.org`

```json
"dependencies": {
  "com.nickmaltbie.openkcc": "1.0.1",
  "com.nickmaltbie.screenmanager": "3.0.0",
  "com.nickmaltbie.statemachineunity": "1.1.0",
  "com.nickmaltbie.testutilsunity": "1.0.0",
  "com.unity.inputsystem": "1.0.0",
  "com.unity.textmeshpro": "3.0.0"
}
```

### Tests

If you wish to include the testing code for this project, make sure to add
the `com.unity.inputsystem` and `com.nickmaltbie.openkcc` to the testables
of the project manifest.

```json
  "testables": [
    "com.nickmaltbie.openkcc",
    "com.nickmaltbie.testutilsunity",
    "com.unity.inputsystem"
  ]
```

Additionally, some of the testing code uses pro builder's api, so make
sure to import [com.unity.probuilder](https://docs.unity3d.com/Packages/com.unity.probuilder@5.0/manual/index.html)
version 5.0 or newer as well.

In order to run the tests, you will need to import the [Moq](https://github.com/moq/moq)
library. My favorite way to import the `Moq.dll` in Unity is by using
[NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity).

## Samples

In order to run the samples from the project, you must import the following
projects:

* [com.nickmaltbie.recolorshaderunity](https://nickmaltbie.com/RecolorShaderUnity/docs/)
  version 1.0.0 or newer.
* [com.unity.probuilder](https://docs.unity3d.com/Packages/com.unity.probuilder@5.0/manual/index.html)
  version 5.0 or newer
* [com.unity.render-pipelines.universal](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@11.0/manual/index.html)
  version 10.0 or newer

The samples in the project include:

* ExampleFirstPersonKCC - Example first person character controller with a basic
  test scene.
* SimplifiedDemoKCC - Simplified character controller with basic movement scripts.
* ExampleColliderShapes - Example of different collider shapes for players.

## Netcode Example

Using [Unity's netcode package](https://docs-multiplayer.unity3d.com/netcode/current/about)
I created another example package called `com.nickmaltbie.openkcc.netcode`
with a sample `NetcodeExample` for an example of
setting up the OpenKCC as a networked character controller.

To add the netcode example to your project, you can download it from
one of the release branches under the pattern `release/netcode/version`
or from the npm repo with the name `com.nickmaltbie.openkcc.netcode`.
It contains some useful utility classes in addition to the sample.

The sample is hosted online at [https://nickmaltbie.com/OpenKCC/Netcode/](https://nickmaltbie.com/OpenKCC/Netcode/)
but you will need to host a server on a windows/linux/mac machine as the
WebGL build for unity does not support opening a server socket within WebGL.

Required packages for samples that are not
already included in the project:

```json
{
  "scopedRegistries": [
    {
      "name": "nickmaltbie",
      "url": "https://registry.npmjs.org",
      "scopes": [
        "com.nickmaltbie"
      ]
    }
  ],
  "dependencies": {
    "com.community.netcode.transport.websocket": "git+https://github.com/Unity-Technologies/multiplayer-community-contributions?path=/Transports/com.community.netcode.transport.websocket",
    "com.nickmaltbie.recolorshaderunity": "1.0.0",
    "com.unity.inputsystem": "1.4.4",
    "com.nickmaltbie.testutilsunity": "1.0.0",
    "com.unity.netcode.gameobjects": "1.1.0",
    "com.unity.probuilder": "5.0.6"
  }
}
```

The samples in the project include:

* ExampleFirstPersonKCC - Example first person character controller with a basic
  test scene.
* Mole KCC Sample Character - Example mole character controller for moving
  around and climbing on objects.

## Documentation

Documentation on the project and scripting API is found at
[https://nickmaltbie.com/OpenKCC/docs/](https://nickmaltbie.com/OpenKCC/docs/)
for the latest version of the codebase.

To view the documentation from a local build of the project install
[DocFX](https://dotnet.github.io/docfx/), use the
following command from the root of the repo.

```bash
Documentation/build.sh
```

(Or this for windows)

```cmd
.\Documentation\build-validation.cmd
```

The documentation for the project is stored in the folder `/Documentation`
and can be modified and changed to update with the project.

_This documentation project is inspired by the project by Norman Erwan's
[DocFxForUnity](https://github.com/NormandErwan/DocFxForUnity)_

## Learning

I will be making a video series discussing how the Open KCC works and going
into detail about how the various features work, describing game design in
general, and details about the unity engine and virtual environments.

As these videos are created they will be listed here:

* [Designing Character Controllers Intro](https://youtu.be/Hv4CQMCxSWE)
* [Physics Behind Games and Character Interactions](https://youtu.be/rzD-Lm8pOX0)
* [Projection Based Movement of KCC](https://youtu.be/s-99Z_W8bcQ)
* [How the KCC Manages Jumping](https://youtu.be/CGsDdBZa5EM)
* [Camera controller and Dither Shader](https://youtu.be/Zw6qvOOHGC4)
* [Why Test Games](https://youtu.be/3No2mpbHJrI)

## Development

If you want to help with the project, feel free to make some
changes and submit a PR to the repo.

This project is developed using Unity Release
[2021.3.11f1](https://unity3d.com/unity/whats-new/2021.3.11).
Install this version of Unity from Unity Hub using this link:
[unityhub://2021.3.11f1/0a5ca18544bf](unityhub://2021.3.11f1/0a5ca18544bf).

### Git LFS Setup

Ensure that you also have git lfs installed. It should be setup to auto-track
certain types of files as determined in the `.gitattributes` file. If the
command to install git-lfs `git lfs install` is giving you trouble, try
looking into the [installation guide](https://git-lfs.github.com/).

Once git lfs is installed, from in the repo, run the following command to pull
objects for development.

```bash
git lfs pull
```

### Githooks Setup

When working with the project, make sure to setup the `.githooks` if
you want to edit the code in the project. In order to do this, use the
following command to reconfigure the `core.hooksPath` for your repository

```bash
git config --local core.hooksPath .githooks
```
