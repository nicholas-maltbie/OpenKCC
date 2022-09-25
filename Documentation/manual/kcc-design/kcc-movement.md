# KCC Movement Design

This document provides a detailed description and overview of how the
movement for the kinematic character controller works.

Here is the companion YouTube video for this design document:

<!-- markdownlint-disable MD013 -->
<!-- Disable line length lint rule for portion of embed -->
<iframe width="560" height="315"
    src="https://www.youtube.com/embed/s-99Z_W8bcQ"
    title="Moving Characters in Games – Kinematic Character Controller in Unity"
    frameborder="0"
    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
    allowfullscreen></iframe>
<!-- markdownlint-enable MD013 -->

## Goals and Objectives

Character movement is a core part of many games. How a character moves can
determine how the game feels. Whether it’s the precise and controlled movement
of platformers like Celeste, the predictable movement of shooters like
Overwatch, or even the humorous movement of rolling a rock down a hill in Rock
of Ages. How a character moves through a scene is important to set a tone
and give a player agency while playing.

The main goals of a character controller are to:

1. Move the player
1. Interact with the environment
1. Set a tone

The design of the OpenKCC is broad enough to encompass many different use
cases but has a few constraints.
The OpenKCC will create motion similar to that of Quake’s character movement
with precise and responsive controls based on player input.
We will be using Unity’s physics engine to compute how the player moves
through a 3D space.

## Reading Player Input

To move the player around based on precise controls, we must first ensure
the player moves following character expectations to avoid disorientation.

To read input, the OpenKCC project uses unity's [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)
module. For moving the player forward and backward
or up and down, we can convert the inputs from a joystick or
the `wasd` keys on the keyboard to a `Vector2` data type that
represents the players desired movement direction.

where 1 represents moving forward in that direction and -1 represents moving
backward with 0 for no movement. We also need to ensure that this vector
stays within a circle by normalizing its length, so the player won’t move
faster if they move diagonally.

![Bounded Player Movement](../../resources/design/bounded-movement.png)

## Camera Movement

In addition to the player movement, we must also consider the camera movement.
The current basic examples in the project use a first person character
controller that rotates with mouse movement.

Similar to the previous movement setup, we can create controls based
on the mouse delta to rotate the camera up or down.
To achieve this, with the given mouse input, we will modify the current target
pitch (looking up and down) and yaw (looking left and right).
Each frame, we will change this pitch and yaw based on how far the
player moves the mouse. We must also add a bound to the maximum a
player can look up or down, we don’t want them to rotate around and see
the world upside down.

Additionally, the player movement vector will need to be multiplied by
the current camera direction to move in the direction the player is expecting.

![Matrix multiplication for player movement and direction](../../resources/design/multiply-movement.png)

## Collisions and Colliders

Great, we now know where the player wants to move. However, this does not
account for colliding with surfaces. Despite how the player looks, they are
this capsule, or pill like, shape when detecting collisions with the world.
This pill shape is useful as it is roughly a humanoid shape and allows for
sliding off angled surfaces as to not get stuck on edges like a cube would.

The general version of this movement is represented in the code via
the [IColliderCast](xref:nickmaltbie.OpenKCC.Utils.IColliderCast) interface.

Unity has a function to determine if a capsule would collide with any object
in the scene when moving it a given direction. Using this function, we can
check what the play would hit and stop them before they hit that object.

The specific example of [CapsuleColliderCast](xref:nickmaltbie.OpenKCC.Utils.CapsuleColliderCast)
represents an implementation of the `IColliderCast` for a capsule shaped object.

## Bouncing and Sliding

Now we can stop a character before they clip through walls. However, this means
the player will get stuck whenever they hit something and not move any more.
This will get the characters stuck on surfaces and stop them from moving
smoothly. To get around this, we can take their remaining motion after hitting
something and have the character bounce and slide off that surface.

We don’t want them to bounce like a rubber ball as that would make it difficult
to move around precisely, instead we want to have the character slide in the
same direction as the surface they hit. So, if we run into a wall, we will
slide along that wall. If we run into a ramp, we will slide up the ramp.
We can achieve this by projecting our remaining movement onto the plane we hit
using the normal vector of that surface then scaling it to retain the remaining
distance we can move.

![Projected player movement bouncing off a wall](../../resources/design/projected-movement.png)

During any individual movement, the player should not bounce too many times, so
limiting this to three bounces should be sufficient. 
We can also decrease this remaining momentum depending on how sharp of an angle
the player makes when walking into the surface. This makes walking directly into
walls move slowly while only grazing a wall have little effect.

Most of the important movement actions are configured via the
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils) class.

## Adding Gravity

This same function we used to detect how the player hits walls can also be
used to detect if we are standing on the ground. We simply need to cast
the character in a downward direction and see if they are close enough
to the ground. We can set a threshold of one centimeter. Anything closer
than that we will be on the ground.

We can add a velocity to our player and increase that velocity
using a gravity acceleration if they player is not standing on
the ground. And set this velocity to zero when the player is on the ground.
This way the player will slowly speed up while they are not on the ground
and stop once they hit the ground.

## Algorithm Overview

Putting all these concepts we’ve discussed earlier together; we can design a
component that with move a player. This component has a defined camera angle,
velocity, and gravity and for each frame will:

1. rotate the camera based on player input
1. compute the desired movement of the player from camera angle and input
1. checking if the player is on the ground and adjusting [their velocity] accordingly
1. move the player based on their currently velocity and the desired movement

Putting all these concepts together into one class we get this basic character
movement that interacts responsively to player input.

## Simplified Example

There is an example of this basic character controller in my open source
projected called a [SimplifiedKCC](xref:nickmaltbie.OpenKCC.Demo.SimplifiedKCC)
if you want to investigate the source code for how this works with a simple
example.

The full KCC has quite a few more parameters and features that is a bit
more complex, see the [Example Usage](../usage.md) for more details.
