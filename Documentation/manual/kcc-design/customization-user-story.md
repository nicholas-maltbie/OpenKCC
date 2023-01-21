**Expanding on ‘Customization User’ Scenarios**

**Level 1: Planar 3D Platformer Character Controller**

————————————————————————————————————————————————————————————————————————

User Story:

As a Customization User with an existing project, I’m interested in having my development studio switch to OpenKCC from a paid asset for the following reasons
- To take advantage of arbitrary colliders
- To benefit from wide test coverage and reliable low-level code
- To contribute to, learn from, and benefit from an Open Source code base

While we are interested in the the higher level features of OpenKCC, as a pre-existing studio we already have our own high level code that enables us to reach a high velocity of output with our designers who are already accustomed to our tools

Ideally, our game architecture programmer will be able to use the API of the KCCMovementEngine to make it compatible with our high level code which already handles things such as state switching, etc.

We could probably be convinced to let KCC handle things like gravity for us if the API were well defined and the customization was robust enough, but for the foreseeable future it’s best to think that we are glued to our custom state solution.

**Use Case Level #1: A simple yet robust character that is driven by custom client-side code making use of the Customization Level API**

**Example Use Case:** For example, a character controller like this is something we would hope to be able to achieve while handling states, input, and events ourselves:
https://www.youtube.com/watch?v=ylG72jvo_2s&ab_channel=CaptainRipley
- In fact I intend to start implementing a character controller with equivalent features, that way we could focus on porting it to Open KCC and developing the KCCMovementEngine API alongside it

Features:
- A Run and Jump State that reads player input and enables planar movement around the scene, as well as custom, client defined jump physics
- A long Jump state that the player can initiatie with client side input
    - The author of the long jump state (the customization user)

Technical Requirements:
- Ability to pass client-side managed velocity values to the KCCMovementEngine for movement in the XYZ plane

- Collision Phase Callbacks, or the ability to read the state of the KCCMovementEngine after executing the move for the frame
    - Practical Example:
        - Imagine we have a charge state that starts at a speed of 0 meters per second and then slowly accelerates in a given direction
        - At one point, our charging character is moving in the positive z axis and has achieved a speed of 15 meters per second
        - We then call our move function for the frame, but this time we collide head on to a wall
        - After executing the move function, I should basically be able to read the following data from the KCCMovementEngine Object
            - “Your desired velocity for the most recent frame was 15, but your actual velocity was lowered to 0 due to the collision phase”
            - I can then read that 0 value from the KCC Movement Engine, and re-apply it to my Client Side State
        - This basically keeps the client side velocity and the KCC Movement Engine state in sync, since with this state definition the user should have to Reaccelerate again once more after colliding, they shouldn’t instantly snap back to 15 meters per second if the wall were suddenly removed

- Collision Phase Callbacks Extended
    - For example, while in the long jump state, we need to check if we collided with someting head on after executing each move
        - We would then use this information to enter a Mario style “Bonk” state where the player loses control and our character is repelled from the wall (Same UX as Mario Odyssey where if you long jump into a wall mario hits his head and is repelled)




Taking Action:

I plan to get started making a Mario Odyssey style controller with my current toolset that we can then begin porting to OpenKCC while also using it as a way to define the new API

Upon reflecting on my “Customization User” user stories, I think that the API that solves most of my immediate problems might look something like this, where I can set the velocity of the KCC Movement engine either directly or through a callback that the KCCMovementEngine provides. (Proper architecture withheld for brevity)

```c#
/*
* Class: SimpleKCC_ClientMover
* Simple Direct Usage with One Logical State defined in this classs
*/
private void FixedUpdate()
{
	// Client Object Updates Its Internal Velocity
	UpdateVelocityPreMove();

	// Set the Movemnt Engine to Client’s Updated Velocity
	_movementEngine.SetVelocity(this.Velocity);

	// Movement Engine Performs Move Based on the provided velocity
	_movementEngine.PerformMove();

	// Read Object State Post-Move to Update Client Velocity In Response to Being Groudned or Not
	var postMoveGroundedState = _movementEngine.GroundedState;

	// Read Object State Post-Move to allow us to react to events such as collisions
	var postMoveCollisions = _movementEngine.Collisions

	// Client Reacts to KCCMovementEngine state post move
	UpdateVelocity(postMoveGroundedState, postMoveCollisions);
}
```

```c#
/*
* Class: MovementStateMachine
* Similar Direct Usage of KCC but with Support for an Arbitrary Client Defined State machine
*/
private void FixedUpdate()
{
	// Client Object Updates Its Internal Velocity State
	MovementState currentState = MovementStateMachine.CurrentState;

	// Update the Current State PreMove by reading KCCMovementEngine state
	currentState.UpdateVelocityPreMove(_movementEngine.Velocity);

	Set the MovemntEngine’s Velocity to the Current State’s Velocity
	_movementEngine.SetVelocity(currentState.Velocity);

	// Movement Engine Performs Move Based on the provided velocity
	_movementEngine.PerformMove();

	// Read Object State Post-Move to Update Client Velocity In Response to Being Groudned or Not
	var postMoveGroundedState = _movementEngine.GroundedState;

	// Read Object State Post-Move to allow us to react to events such as collisions
	var postMoveCollisions = _movementEngine.Collisions

	// Client Reacts to KCCMovementEngine state post move
	currentState.UpdateVelocityPostMove(postMoveGroundedState, postMoveCollisions);

	// Update the movement state machine to check if it should transition to a different state, etc
	this.MovementStatemachine.Tick()
}
```

**Future Use Cases:**

**Use Case Level #2: Elegantly Switch Between Advanced, Client Authored KCCMovementEngine Behaviour (Rare Use Case)**

————————————————————————————————————————————————————————————————————————

Main Idea: Allow client defined movement states to use entirely different KCCMovementEngines, or perhaps let them use a single KCCMovementEngine that is configured differently for different scenarios
- For example, what if the previous player could pick up a construction hat power up, enabling them to dig in the ground and up walls like a mole by holding down the right trigger?
- This would be a new state for the player, but it would also be a state that behaves fundamentally differently than 99% of the other states in our game since it is different at the gravity and rotation level.


**Use Case Level #3: Allowing User Defined States to Benefit from Custom Gravity’s (Mario Galaxy Style) without explicitly accounting for them their code**

————————————————————————————————————————————————————————————————————————

- For example, what if my game studio defined a simple state for our game that simply moves the KCCMovementEngine object in a direction on the XZ plane based on player input
- What if through an appropriate abstraction, that state could then be reused in a mario galaxy style game by simply changing a configuration of the KCCMotor, without changing the state itself?
- This is the feature request that I’m least technically aware of what I’m asking for, but perhaps the idea is that we can somehow abstract the movement of the player to something like KCCMovementEngine.GravityRelativeMove(delta time, CharacterForwardVector), and then the idea is that in a simple game forward would just be forward in the Z axis, but if we configured a Mario Like environment and flipped a few variables, forward would be come what we expect forward to be in a Mario Galaxy Style Game
