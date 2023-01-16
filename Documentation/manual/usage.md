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
1. @nickmaltbie.OpenKCC.CameraControls.ICameraControls -
    This controls the direction the character is looking at and which direction
    they should move when the player inputs a forward, left, or right
    input.

### Configurable Properties

There are many properties that configure the OpenKCC. These properties
are also explained in depth in the @nickmaltbie.OpenKCC.Character.KCCStateMachine
documentation page. The config variables for the KCCStateMachine are
stored in the @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig

- **Input Controls** - Controls to manage character movement.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.moveAction
        \- Action to move player along two axis input
        (forward back, left right)
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.sprintAction
        \- Button action to control sprinting speed
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.jumpAction
        \- Button action to start player jump configured via a
        @nickmaltbie.OpenKCC.Character.Action.JumpAction
- **Player Jump Settings** - Settings relevant to player jumps
    - @nickmaltbie.OpenKCC.Input.BufferedInput.inputAction
        \- @UnityEngine.InputSystem.InputActionReference for the input.
    - @nickmaltbie.OpenKCC.Input.BufferedInput.bufferTime
        \- Time in which the player's input for jumping
        will be buffered before hitting hte ground in seconds.
    - @nickmaltbie.OpenKCC.Input.BufferedInput.cooldown
        \- Minimum time between jumps in seconds.
    - @nickmaltbie.OpenKCC.Character.Action.JumpAction.jumpVelocity
        \- Vertical velocity of player when they jump.
    - @nickmaltbie.OpenKCC.Character.Action.JumpAction.maxJumpAngle
        \- Maximum angle at which the player can jump (in degrees).
    - @nickmaltbie.OpenKCC.Character.Action.JumpAction.jumpAngleWeightFactor
        \- Weight to which the player's jump is weighted towards the
        direction of the surface they are standing on.
    - @nickmaltbie.OpenKCC.Character.Action.ConditionalAction.coyoteTime
        \- Time in which the player will float before they start to
        fall when they are not grounded.
- **Ground Checking** - Configures grounded state for player.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.gravity
        \- Direction and magnitude of gravity in units per second squared.
    - The rest of the values are configured via the
    @nickmaltbie.OpenKCC.Character.Config.KCCGroundedState by the
    @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.groundedState parameter
    - @nickmaltbie.OpenKCC.Character.Config.KCCGroundedState.groundedDistance
        \- Distance from ground at which a player is considered standing on the ground.
    - @nickmaltbie.OpenKCC.Character.Config.KCCGroundedState.groundCheckDistance
        \- Distance to check player distance to ground.
    - @nickmaltbie.OpenKCC.Character.Config.KCCGroundedState.maxWalkAngle
        \- Maximum angle at which the player can walk (in degrees).
- **Motion Settings** - Settings for controlling player movement.
  Many of these settings are based on the character movement design,
  See [KCC Movement Design](kcc-design/kcc-movement.md) for more details.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.walkingSpeed
        \- Speed at which the player walks, in units per second.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.sprintSpeed
        \- Speed of player when sprinting, in units per second.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.maxBounces
        \- Maximum bounces for computing player movement.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.pushDecay
        \- Preserved momentum percentage when pushing an object. A value
        of 0 would indicate a complete stop while a value of 1 would be fully
        bouncing off an object when the player collides with it.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.anglePower
        \- Angle decay rate when sliding off surfaces.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.maxPushSpeed
        \- Maximum distance a player can be pushed when overlapping
        other objects in units per second. Useful for resolving collisions.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.verticalSnapDown
        \- Distance at which the player can "snap down" when walking.
- **Stair and Step** - Setting related to player stairs and steps.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.stepUpDepth
        \- Minimum depth required when stepping up stairs.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.verticalSnapUp
        \- Max distance the player can snap up stairs.
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.snapBufferTime
        \- Time in which the player can snap up or down
        steps even after starting to fall
- **Moving Ground** - Settings relevant to attaching to moving ground
    - @nickmaltbie.OpenKCC.Character.Config.HumanoidKCCConfig.maxDefaultLaunchVelocity
        \- Max velocity at which the player can be launched
        when gaining momentum from a floor object without
        an IMovingGround attached to it.

## Making Your Own Custom Kinematic Character Controller

If you want to configure your own custom character controller,
most of the important movement actions are configured via the
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils) class.
See the [KCC Movement Design](kcc-design/kcc-movement.md) for more details
on how the KCC movement works.

There are some simplified CharacterController examples in the samples
of the project. Feel free to use the library code to configure or build
up your own custom character controller based on your project requirements.
