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

Make sure to add the required dependcies to your project

* [com.unity.inputsystem](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)
  version 1.0 or newer
* [com.unity.textmeshpro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)
  version 3.0 or newer
* [com.nickmaltbie.screenmanager](https://nickmaltbie.com/ScreenManager/docs/index.html)
  version 3.0 or newer

In order to use the samples in the project, make sure to also add the following
projects to your project.

Install the latest version of the project by importing a project via git
at this URL:
`https://github.com/nicholas-maltbie/OpenKCC.git#release/latest`

If you want to reference a specific tag of the project such as version `v0.1.0`,
add a `release/#v1.0.0` to the end of the git URL to download the package
from th auto-generated branch for that release. An example of importing `v0.1.0`
would look like this:
`https://github.com/nicholas-maltbie/openkcc.git#release/v0.1.0`.

To use the latest release, simply reference:

```text
https://github.com/nicholas-maltbie/openkcc.git#release/latest
```

For a full list of all tags, check the [OpenKCC Tags](https://github.com/nicholas-maltbie/ScreenManager/tags)
list on github. I will usually associated a tag with each release of the project.

_Note_: before I started using the package format for the project, I manually
released a unity package you needed to import. Any version before `v0.1.0`
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
  // ... other dependencies
  "com.nickmaltbie.openkcc": "0.1.0",
  "com.nickmaltbie.screenmanager": "3.0.0",
  // ... other dependencies
  "com.unity.inputsystem": "1.0.2",
  "com.unity.textmeshpro": "3.0.6"
}
```

### Tests

If you wish to include the testing code for this project, make sure to add
the `com.unity.inputsystem` and `com.nickmaltbie.openkcc` to the testables
of the project manifest.

```json
  "testables": [
    "com.unity.inputsystem",
    "com.nickmaltbie.openkcc"
  ]
```

Additionally, some of the testing code uses pro builder's api, so make
sure to import [com.unity.probuilder](https://docs.unity3d.com/Packages/com.unity.probuilder@5.0/manual/index.html)
version 5.0 or newer as well.

## Samples

In order to run the samples from the project, you must import the following
projects:

* [com.unity.probuilder](https://docs.unity3d.com/Packages/com.unity.probuilder@5.0/manual/index.html)
  version 5.0 or newer
* [com.unity.render-pipelines.universal](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@11.0/manual/index.html)
  version 10.0 or newer

The samples in the project include:

* ExampleFirstPersonKCC - Example first person character controller with a basic
  test scene.
* SimplifiedDemoKCC - Simplified character controller with basic movement scripts.

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
.\Documentation\build.cmd
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
* \[Planned\] Character Controller Case Study and Requirements Engineering

## Features

Movement in a 3D space including

* Physics based movement off dynamic surfaces
* Configurable jump and speed
* Multiplayer support
* Ground detection
* Maximum slope for walking
* Moving and rotating platforms
* Snapping up and down stairs
* Rag-doll mode
* Animation and inverse kinematics
* First and third person camera
* Adjustable camera zoom
* Fading character model
* Configurable controls
* Changing player model
* Interactable objects

Some of these features are still  in the previous
[Falling Parkour](https://github.com/nicholas-maltbie/FallingParkour)
project but will be added to the Open KCC soon.

## Future Improvements

Future improvements that are in development

* Slippery floors
* First person character model
* Ladders and vertical movement
* Climbing surfaces
* Procedural animations using inverse kinematics
* Swimming and floating in water
* Non-humanoid avatars and shapes
* Automated unit and integration testing

## Development

If you want to help with the project, feel free to make some
changes and submit a PR to the repo.

This project is developed using Unity Release [2021.1.19f1](https://unity3d.com/unity/whats-new/2021.1.19).
Install this version of Unity from Unity Hub using this link:
[unityhub://2021.1.19f1/d0d1bb862f9d](unityhub://2021.1.19f1/d0d1bb862f9d).

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
