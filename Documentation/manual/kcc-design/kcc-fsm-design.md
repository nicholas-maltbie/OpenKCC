# State Machine Design

In order to manage the movement and controls of the
kinematic character controller, the OpenKCC will use a
[Finite State Machine](https://en.wikipedia.org/wiki/Finite-state_machine)
(FSM) based design to manage character actions.

The character controller will have high level "states"
that can be changed based on external input or internal
changes. The character will act differently
based on these states.

## State Overview

The state machine for the OpenKCC is composed
of a few different states.

* Idling - When the player is standing still on the ground
* Walking - When the player is moving around
* Falling - When the player is not standing on something
* Sliding - When the player is standing on a steep surface

Each of these states will have a different set of
capabilities and behaviors and will transition to
other states based on external input.

### Transition Diagram

```mermaid
stateDiagram
    [*] --> Idling

    Idling --> Walking : Move Input
    Idling --> Falling : Not Grounded
    Idling --> Sliding : Steep Slope

    Walking --> Idling : Stop Input
    Walking --> Falling : Not Grounded
    Walking --> Sliding : Steep Slope

    Sliding --> Falling : Not Grounded
    Sliding --> Idling : Flat Ground

    Falling --> Idling : Grounded + Flat
    Falling --> Sliding : Grounded + Steep
```

### State Attributes

Each state of the KCC will have a few different
attributes:

* State name and transitions
* Animation to play during state (if any)
* Behavior on entry, exit, and update
* Available player inputs
    such as movement, camera control, etc...

In addition, each transition will also
have a set of configurations that include:

* Conditions to trigger transition
* Animation to play on transition (if any)
* Transition time and configuration
    such as allow early exit/cancellation

## Code Design

The code for this is still in development
but it will be managed via a few different classes.
The main design of these classes will be managed
by a set of [C# Attributes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/)
to configure and manage controls for the state machine
directly from the C# code.

* [IStateMachine](xref:nickmaltbie.OpenKCC.FSM.IStateMachine) - interface to
    manage a set of states and transitions.

    * [StateMachine](xref:nickmaltbie.OpenKCC.FSM.StateMachine)
        \- concrete implementation of state machine
        with cached transitions and events from decorators from [FSMUtils](xref:nickmaltbie.OpenKCC.FSM.FSMUtils).
    * [StateMachineMonoBehaviour](xref:nickmaltbie.OpenKCC.FSM.StateMachineMonoBehaviour)
        \- concrete implementation of state machine with cached transitions and events
        from decorators from [FSMUtils](xref:nickmaltbie.OpenKCC.FSM.FSMUtils)
        in addition to firing off events for Unity Messages and supports
        attributes such as
        [OnUpdate](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnUpdateAttribute)

* [State](xref:nickmaltbie.OpenKCC.FSM.State) - A state for a given FSM.
* [TransitionAttribute](xref:nickmaltbie.OpenKCC.FSM.Attributes.TransitionAttribute)
    \- Attribute to define and manage
    the transitions for a given state.
* AnimationAttribute - Attribute to configure an animation
    or set of animations to play based on a configuration.

    * Still under development...

* Entry and exit behaviors defined via the attributes:

    * [OnEnterState](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnEnterStateAttribute)
        \- Called when stateis entered
    * [OnExitState](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnExitStateAttribute)
        \- Called when the state is exited

* Update Attributes to be triggered on various [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html)
    functions including the following subset. There are other
    messages defined for the unity MonoBehaviour but these
    are the only planned ones as of now, feel free to extend
    the code or add your own events if you wish.

    * [OnUpdate](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnUpdateAttribute)
        : Called each frame.
    * [OnFixedUpdate](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnFixedUpdateAttribute)
        : Called each fixed update.
    * [OnLateUpdate](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnLateUpdateAttribute)
        : Called at the end of each frame.
    * [OnGUI](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnGUIAttribute)
        : Called each GUI update.
    * [OnEnable](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnEnableAttribute)
        : Called when object is enabled.
    * [OnDisable](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnDisableAttribute)
        : Called when object is disabled.
    * [OnAnimatorIK](xref:nickmaltbie.OpenKCC.FSM.Attributes.OnAnimatorIKAttribute)
        : Callback for setting up animation IK (inverse kinematics).

## Customization

You may want to design your own character controller or
other object based on this state machine design, feel
free to use or extend the code however you see fit.
As of right now, there is only one state machine example
in the project of the character controller
but you can extend or change it however you wish.

Some examples of custom state machines are added
in the test code under

* [DemoStateMachine](xref:nickmaltbie.OpenKCC.Tests.EditMode.FSM.DemoStateMachine)
    \- Example implementation of a
    [StateMachine](xref:nickmaltbie.OpenKCC.FSM.StateMachine).
* [DemoStateMachineMonoBehaviour](xref:nickmaltbie.OpenKCC.Tests.EditMode.FSM.DemoStateMachineMonoBehaviour)
    \- Example implementation of a
    [StateMachineMonoBehaviour](xref:nickmaltbie.OpenKCC.FSM.StateMachineMonoBehaviour).
