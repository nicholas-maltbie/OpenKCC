# Introduction to OpenKCC

The Open Kinematic Character Controller is an open source kinematic character controller that allows for a player moving
as a kinematic object through a scene. This is done through using unity's physics engine and projection based movement.

# Learning

As these videos are created they will be listed here:
* [Designing Character Controllers Intro](https://youtu.be/Hv4CQMCxSWE)
* [Physics Behind Games and Character Interactions](https://youtu.be/rzD-Lm8pOX0)
* [Projection Based Movement of KCC](https://youtu.be/s-99Z_W8bcQ)
* [How the KCC Manages Jumping](https://youtu.be/CGsDdBZa5EM)
* [Camera controller and Dither Shader](https://youtu.be/Zw6qvOOHGC4)
* \[Planned\] Character Controller Case Study and Requirements Engineering

In the future, I will write up more wiki articles on how to use the KCC in your own project, how to modify the OpenKCC
to add custom features, and on more in depth summaries on how the OpenKCC works as described in the videos above.

## Organization of Project

The project is organized into a few namespaces:
* `nickmaltbie.OpenKCC.Animation` - Code related to animating the player's avatar. 
* `nickmaltbie.OpenKCC.Character` - Code for moving and controlling the character with inputs. 
* `nickmaltbie.OpenKCC.Demo` - Debug code used for demonstrations and videos. 
* `nickmaltbie.OpenKCC.Environment` - Code used for creating and interacting with the environment.
* `nickmaltbie.OpenKCC.UI` - Code used to create a user interface via buttons and on screen controls. 
* `nickmaltbie.OpenKCC.Utils` - Utility code for generic functions used across the project.
