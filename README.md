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

This library was developed as part of the Falling Parkour Project here -
[https://github.com/nicholas-maltbie/FallingParkour](https://github.com/nicholas-maltbie/FallingParkour)

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
