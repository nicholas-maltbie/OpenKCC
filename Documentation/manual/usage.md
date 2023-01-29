# Example Usage

The OpenKCC can be added as a basic character controller to your project
via using the
@nickmaltbie.OpenKCC.Character.KCCStateMachine
Mono Behaviour. This example character controller allows you to configure
a character controller with a unique camera controller and collider cast.

The default example in the samples has a character controller that
uses the @nickmaltbie.OpenKCC.CameraControls.CameraController
and a @nickmaltbie.OpenKCC.Utils.ColliderCast.CapsuleColliderCast.

- This creates a character with a hybrid first and third person camera
  perspective and has a capsule collider shape similar to unity's basic
  @UnityEngine.CharacterController

![Example panel of OpenKCC](../resources/example-usage-openkcc.png)

## Use Cases

The KCCStateMachine serves as a basic character controller
for movement in 3D space tied to a camera controller.

For examples on how to use the project, check out the
[Samples](https://github.com/nicholas-maltbie/OpenKCC/tree/main/Assets/Samples)
in the OpenKCC GitHub project.

Some use cases of the kinematic character controller over the built-in
or rigidbody based character controllers include:

- Using a non capsule shape via other
    @nickmaltbie.OpenKCC.Utils.IColliderCast behaviors and settings.
- Using a kinematic physics object over the built in character controller
    which is not tied to a physics object.
- Supporting a rotating player model that does not have to follow the vertical
    Y-axis.
- Supported interactions with
    @nickmaltbie.OpenKCC.Environment.MovingGround.IMovingGround surfaces.

## Setup and Required Behaviors

To configure the OpenKCC requires a few attached components to function
properly. The example from the Samples in the project have
a basic use case.

1. [Rigidbody](https://docs.unity3d.com/ScriptReference/Rigidbody.html) -
    This is to manage the _Kinematic_ part of the kinematic
    character controller as well as behaviour when the character goes into
    rag doll/prone mode. This should also correspond with an attached
    collider(s) to the character.
1. @nickmaltbie.OpenKCC.Utils.ColliderCast.CapsuleColliderCast -
    This manages how the character bounces off objects and navigates the 3D
    scene. It should align with the attached collider shapes.
    As of right now, the only supported collider shape is a
    [CapsuleCollider](https://docs.unity3d.com/ScriptReference/CapsuleCollider.html)
    via the @nickmaltbie.OpenKCC.Utils.ColliderCast.CapsuleColliderCast
    but there are plans to expand this to include all primitives as well
    as composited colliders of multiple primitives together.
1. @nickmaltbie.OpenKCC.Character.KCCMovementEngine -
    \- The calls to the lower level APIs to move the player,
    follow moving platforms, ands other configurations.
1. @nickmaltbie.OpenKCC.CameraControls.ICameraControls
    \- This controls the direction the character is looking at and which
    direction they should move when the player inputs a forward, left,
    or right input. An example of this has been implemented
    in the @nickmaltbie.OpenKCC.CameraControls.CameraController
    for a hybrid first person/third person camera controller.

### Configurable Properties

There are many properties that configure the OpenKCC. These properties
are also explained in depth in the @nickmaltbie.OpenKCC.Character.KCCStateMachine
documentation page.

- **Input Controls** - Controls to manage character movement.
    - @nickmaltbie.OpenKCC.Character.KCCStateMachine.moveActionReference
        \- Action to move player along two axis input
        (forward back, left right)
    - @nickmaltbie.OpenKCC.Character.KCCStateMachine.sprintActionReference
        \- Button action to control sprinting speed
    - @nickmaltbie.OpenKCC.Character.KCCStateMachine.jumpAction
        \- Button action to start player jump configured via a
        @nickmaltbie.OpenKCC.Character.Action.JumpAction
- **Player Movement Settings** - Settings relevant to player movement
    - @nickmaltbie.OpenKCC.Character.KCCStateMachine.walkingSpeed
        \- player speed while walking.
    - @nickmaltbie.OpenKCC.Character.KCCStateMachine.sprintSpeed
        \- Player speed while sprinting.
    - @nickmaltbie.OpenKCC.Character.KCCStateMachine.jumpVelocity
        \- Velocity of player when Jumping.

The rest of the movement settings are controlled
via the @nickmaltbie.OpenKCC.Character.KCCMovementEngine
with the following configurable parameters:

- @nickmaltbie.OpenKCC.Character.KCCMovementEngine.stepHeight
    \- Maximum height of steps the player can climb up.
- @nickmaltbie.OpenKCC.Character.KCCMovementEngine.maxWalkAngle
    \- Maximum angle the player can walk up before
    they start slipping back down.

## Making Your Own Custom Kinematic Character Controller

If you want to configure your own custom character controller,
most of the important movement actions are configured via the
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils) class.
See the [KCC Movement Design](kcc-design/kcc-movement.md) for more details
on how the KCC movement works.

There are some simplified CharacterController examples in the samples
of the project. Feel free to use the library code to configure or build
up your own custom character controller based on your project requirements.
