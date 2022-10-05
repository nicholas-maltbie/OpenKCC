# Example Usage

The OpenKCC can be added as a basic character controller to your project
via using the
[KinematicCharacterController](xref:nickmaltbie.OpenKCC.Character.KinematicCharacterController)
Mono Behaviour. This example character controller allows you to configure
a character controller with a unique camera controller and collider cast.

The default example in the samples has a character controller that
uses the [CameraController](xref:nickmaltbie.OpenKCC.Character.CameraController)
and a
[CapsuleColliderCast](xref:nickmaltbie.OpenKCC.Utils.CapsuleColliderCast).

- This creates a character with a hybrid first and third person camera
  perspective and has a capsule collider shape similar to unity's basic
  [CharacterController](https://docs.unity3d.com/ScriptReference/CharacterController.html)

![Example panel of OpenKCC](../resources/example-usage-openkcc.png)

## Use Cases

The KinematicCharacterController serves as a basic character controller
for movement in 3D space tied to a camera controller.

For examples on how to use the project, check out the
[Samples](https://github.com/nicholas-maltbie/OpenKCC/tree/main/Assets/Samples)
in the OpenKCC GitHub project.

Some use cases of the kinematic character controller over the built-in
or rigidbody based character controllers include:

- Using a non capsule shape via other
    [IColliderCast](xref:nickmaltbie.OpenKCC.Utils.IColliderCast)
    behaviors and settings.
- Using a kinematic physics object over the built in character controller
    which is not tied to a physics object.
- Supporting a rotating player model that does not have to follow the vertical
    Y-axis.
- Supported interactions with
    [IMovingGround](xref:nickmaltbie.OpenKCC.Environment.MovingGround.IMovingGround)
    surfaces.

## Setup and Required Behaviors

To configure the OpenKCC requires a few attached components to function
properly. The example from the Samples in the project have
a basic use case.

1. [Rigidbody](https://docs.unity3d.com/ScriptReference/Rigidbody.html) -
    This is to manage the _Kinematic_ part of the kinematic
    character controller as well as behaviour when the character goes into
    rag doll/prone mode. This should also correspond with an attached
    collider(s) to the character.
1. [IColliderCast](xref:nickmaltbie.OpenKCC.Utils.CapsuleColliderCast) -
    This manages how the character bounces off objects and navigates the 3D
    scene. It should align with the attached collider shapes.
    As of right now, the only supported collider shape is a
    [CapsuleCollider](https://docs.unity3d.com/ScriptReference/CapsuleCollider.html)
    via the [CapsuleColliderCast](xref:nickmaltbie.OpenKCC.Utils.CapsuleColliderCast)
    but there are plans to expand this to include all primitives as well
    as composited colliders of multiple primitives together.
1. [ICameraControls](xref:nickmaltbie.OpenKCC.Character.ICameraControls) -
    This controls the direction the character is looking at and which direction
    they should move when the player inputs a forward, left, or right
    input.

### Configurable Properties

There are many properties that configure the OpenKCC. These properties
are also explained in depth in the
[KinematicCharacterController](xref:nickmaltbie.OpenKCC.Character.KinematicCharacterController)
documentation page.

- **Input Controls** - Controls to manage character movement.
    - _Move Action_ - Action to move player along two axis input
        (forward back, left right)
    - _Jump Action_ - Button action to start player jump
    - _Sprint Action_ - Button action to control sprinting speed
    - **Ground Checking** - Configures grounded state for player.
    - _Grounded Distance_ - Threshold distance for when player is considered
        to be "on the ground". Used to detect if the player is falling or sliding.
    - _Standing Distance_ - Distance to ground at which player is considered
        standing on something. Used to identify what the player is standing on.
    - _Ground Check Distance_ - Distance to create raycast for checking grounded
        state. Sometimes useful to be slightly farther than grounded distance
        or standing distance to catch edge cases.
    - _Max Walk Angle_ - Maximum slope a player can walk without sliding
        or falling down.
    - _Gravity_ - Direction and magnitude of gravity in units per second squared.
- **Motion Settings** - Settings for controlling player movement.
  Many of these settings are based on the character movement design,
  See [KCC Movement Design](kcc-design/kcc-movement.md) for more details.
    - _Walking Speed_ - Speed at which the player walks, in units per second.
    - _Sprint Speed_ - Speed of player when sprinting, in units per second.
    - _Max Bounces_ - Maximum bounces for computing player movement.
    - _Push Decay_ - Preserved momentum percentage when pushing an object. A value
        of 0 would indicate a complete stop while a value of 1 would be fully
        bouncing off an object when the player collides with it.
    - _Angle Power_ - Angle decay rate when sliding off surfaces.
    - _Max Push Speed_ - Maximum distance a player can be pushed when overlapping
        other objects in units per second. Useful for resolving collisions.
    - _Vertical Snap Down_ - Distance at which the player can "snap down" when
        walking.
- **Stair and Step** - Setting related to player stairs and steps.
    - _Step Up Depth_ - Minimum depth required when stepping up stairs.
    - _Vertical Snap Up_ - Max distance the player can snap up stairs.
    - _Snap Buffer Time_ - Time in which the player can snap up or down
        steps even after starting to fall
- **Player Jump Settings** - Settings relevant to player jumps
    - _Jump Velocity_ - Vertical velocity of player when they jump.
    - _Max Jump Angle_ - Maximum surface angle when the player wants to jump.
    - _Jump Angle Weight Factor_ - How much does surface angle impact jump
        direction.
    - _Jump Cooldown_ - Minimum time between jumps in seconds.
    - _Coyote Time_ - Time in which the player will float before they start to
        fall when they are not grounded.
    - _Jump Buffer Time_ - Time in which the player's input for jumping
        will be buffered before hitting hte ground in seconds.
- **Player Prone Settings** - Settings to control player behavior when knocked
  prone/rag doll
    - _Early Stop Prone Threshold_ - Threshold time in which player is not moving
        to exit prone state.
    - _Threshold Angular Velocity_ - Threshold angular velocity in degrees per
        second for existing prone early.
    - _Threshold Velocity_ - Threshold linear velocity in units per seconds for
        exiting prone early.

## Making Your Own Custom Kinematic Character Controller

If you want to configure your own custom character controller,
most of the important movement actions are configured via the
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils) class.
See the [KCC Movement Design](kcc-design/kcc-movement.md) for more details
on how the KCC movement works.

There are some simplified CharacterController examples in the samples
of the project. Feel free to use the library code to configure or build
up your own custom character controller based on your project requirements.
