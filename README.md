# Open KCC

This project is a sample of the Open Kinematic Character Controller. A Kinematic Character Controller (KCC) provides a
way to control a character avatar as a kinematic object that will interact with the environment.

This is an open source project licensed under a [MIT License](LICENSE.txt). Feel free to use a build of the project for
your own work. If you see an error in the project or have any suggestions, write an issue or make a pull request, I'll
happy include any suggestions or ideas into the project.

[![Designing Character Controllers Video Introduction](Demo/sample-kcc.gif)](https://youtu.be/Hv4CQMCxSWE)

You can see a demo of the project running here: [https://nickmaltbie.com/OpenKCC/](https://nickmaltbie.com/OpenKCC/).
The project hosted on the website is up to date with the most recent version on the `main` branch of this github repo
and is automatically deployed with each update to the codebase.

This library was developed as part of the Falling Parkour Project here -
[https://github.com/nicholas-maltbie/FallingParkour](https://github.com/nicholas-maltbie/FallingParkour)

# Learning

I will be making a video series discussing how the Open KCC works and going into detail about how the various features
work, describing game design in general, and details about the unity engine and virtual environments.

As these videos are created they will be listed here:
* [Designing Character Controllers](https://youtu.be/Hv4CQMCxSWE)
* Projection Based Movement

# Features

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
[Falling Parkour](https://github.com/nicholas-maltbie/FallingParkour) project but will be added to the Open KCC soon.

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

# Development

If you want to help with the project, feel free to make some changes and submit a PR to the repo.

This project is developed using Unity Release [2021.1.19f1](https://unity3d.com/unity/whats-new/2021.1.19). Install this
version of Unity from Unity Hub using this link:
[unityhub://2021.1.19f1/d0d1bb862f9d](unityhub://2021.1.19f1/d0d1bb862f9d).

## Git LFS Setup

Ensure that you also have git lfs installed. It should be setup to auto-track certain types of files as determined in
the `.gitattributes` file. If the command to install git-lfs `git lfs install` is giving you trouble, try looking into the
[installation guide](https://git-lfs.github.com/).

```
# Run this inside the repository after cloning it
# May need to run this on linux
curl -s https://packagecloud.io/install/repositories/github/git-lfs/script.deb.sh | sudo bash
sudo apt-get install git-lfs
```

Once git lfs is installed, from in the repo, run the following command to pull objects for development.
```
git lfs pull
```

## Githooks Setup

When working with the project, make sure to setup the `.githooks` if you want to edit the code in the project. In order to
do this, use the following command to reconfigure the `core.hooksPath` for your repository 

```
git config --local core.hooksPath .githooks
```
