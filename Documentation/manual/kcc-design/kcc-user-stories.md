# OpenKCC User Stories

Document lays out the design goals
for the OpenKCC library and base future
decisions off how we expect users to
interact with the product.

## Design Goals

We want a design that is simple to use, has reasonable defaults
configured, but can also be customized if an end user would like.

Simple set of minimal parameters and APIs to allow for the
most possibilities.

Ideally we want to have a simple document [Make Your Own KCC](make-your-own-kcc.md)
that has the overview of how to customize the KCC
to achieve different goals with the MoleKCC as an example.

## Stakeholders

When going over requirements, it's always good to consider
the main stakeholders of the project.

1. Developers of OpenKCC project.
1. Developers who intend to use OpenKCC API.
    This is sometimes called a consumer of the API.
1. Players who will will use characters designed
    with OpenKCC.

The developers of OpenKCC want the
project to be used by people and serve as a good
educational tool.
The consumers of the API
want an easy to use API for customizing their
characters.
The players want a character that is good to use
and doesn't act buggy.

While these stakeholders don't have competing interests,
they don't necessarily align and will
need some compromise to fit together.

I as the developer of the OpenKCC project
can best use my resources by making a robust
API that many people can easily import
into their game project as consumers of the API.
Since this is an open source project
the more developers that interact and use
the product the more feedback and progress we
can make towards the overall design.

My most effective use of resources will be to design
an easy to use and effective API for others
to add or extend in their own projects.

### Consumers of OpenKCC API

We actually need to design a few different APIs for
different audiences.

1. Audience who wants a character controller but doesn't
    want to muck with the code and only use simple
    parameters. Will call "high level".
1. Audience who wants to be able to modify how the character
    controller works to achieve new scenarios that
    aren't included out of the box (such as
    flying, swimming, climbing).
1. Audience who wants to access the whole API
    and all the complex scenarios. Will call "low level".

## Current Status

This is a description of the API's current status
as of January 19, 2023. Version `1.2.x` of
the OpenKCC library.

The `KCCUtils` class already has all the complex
functions exposed with a fairly robust API, however
it would need a different definition of `IKCCConfig`.

Right now the `IKCCConfig` is trying to satisfy all three
audiences which leads to many "low level" parameters
such as number of bounces and angle decay factor.

So basically the current API tries to satisfy
all stakeholders and does a little bit too much of
everything and doesn't have enough depth for any
individual group.

## Scenarios

So, since we have three audiences, let's define
how they might use the OpenKCC.

### High Level Users

These are users who just want to import the
existing behaviors into their project and
modify configuration values.

For now we can assume that this would be constrained
to a humanoid like character that moves around
the world in a simple 3D platformer style.

They would want scenarios such as:

* Modifying basic parameters
    * Changing the speed the player walks at
    * Changing step height for parameter tuning
    * Modifying max walk angle before slipping
* Modifying strength of gravity, assume gravity
    only goes down for simplicity. In fact,
    we can read the gravity value from Physics.Gravity
    for the project setup to make it even easier.
* Jump action configuration
    * Modifying jump height/speed
    * Modifying jump buffer time and cool down
    * Modifying coyote time for jump
* Modifying shape of character
    * Support for more than just capsule shape
    * Support capsule, sphere, box, etc...
* Pushing rigidbody objects
    * What is considered push-able object,
        rigid-body, 'pushable' label
    * Push force when running into objects
* Support for moving platforms
    * Assign something as a moving or not moving platform
        with a set default behavior for unlabeled objects
    * Allow for configurable launch velocity
        from the platform side, not the player side

### Customization Users

These are users who want to customize the project
to support other scenarios besides just the
basic humanoid walking scenario.

Maybe this is something more complex like
a mole who can dig up walls, holding onto
a glider to fly around, or climbing and
swimming with custom configurations.
We don't know what these users will
want to add so we need to ensure that the API
allow for easy modification for other scenarios.

Kinds of scenarios that should be easy to support

* Different directions of gravity/planes of movement,
    think like Super Mario Galaxy.
* Different camera perspectives, first person,
    third person, fixed viewpoint, etc...
* New movement options along other planes
    such as climbing a wall, flying
    through the air, swimming, etc...
* Configuring the jump action to run off a custom trigger.
* Custom interactions when bouncing off or hitting
    objects.
* Constraining the movement in a particular
    manner such as constricting movement
    along a 2D plane (think like paper mario)
    or constricting movement to a 1D line
    (like grabbing onto a zipline).
    * these kinds of constraints don't need to be
        simple to add, but they do need to be possible
        to modify without having to re-write the
        whole engine.
* Other kinds of movement that aren't humanoid
    such as a vehicle or animal.
* Adding new kinds of actions to select
    between akin to Super Mario Sunshine.
* Allowing the character to grab ledges and do various
    kinds of tricks or parkour.

### Low Level Users

Finally to the low level users.
This would be someone who wants to directly call
the underlying physics solving functions
for something to do something that is outside
the constraints of basic customization.
Maybe something like overriding the basic number
of bounces to simulate a player moving at a very
very fast speed and bouncing off lots of objects.
Maybe they want to have a disjoint hit box that
the player can move parts of independently like
an active ragdoll.

Basically, these end users need access
to the low level changes in the physics solver
for computing individual bounces.
They don't need to worry about things like
whether an object is pushable, the default
configuration for a step, or anything
else like that.
Instead of using the same `IKCCConfig` that
I use for the customization, this should
probably offer a different interface.

Only scenarios the low level should support
include

* Configuring bounces and slide like motion.
* Identifying steps and stepping up when needed.
* Configuring the collider shape and cast interactions.
* Support restricting movement to lower dimension.

## Designing an API

So, now that we have the user stories
defined for the project, we can get to the interesting
part, how do we design the project and interface
such that it can appeal to different audiences.
Ideally this solution should work out of the
box and be fairly easy to configure without too
many moving parts.

The different levels of the user stories are
hierarchical in that the high level API can use
the customizable API, and the customizable API
can call the low level API.
I already have a design for the low level API
fairly well finalized in the KCC Utils class
but it can still be improved further to remove
some features that should be at a higher level.
We want this to be easy to use and configure at
each level of abstraction, so we need to
design the levels as layers on top of each other.

### Low Level API

Entirely defined between the IColliderCast, KCCUtils
level. Has no concept of a character and only works
at the collider and collision solving level.

Need to support movement constraints through
this configuration such as within a fixed plane
or along a fixed line.

### Customizable API

Defined between the KCCMovementEngine and IKCConfig.
Has to support concepts including:

* character shape
    * Support any generic shape and overriding this value
* velocity
* gravity
* plane of movement
    * climbing constraints movement to along plane parallel to wall
    * holding onto a ledge constraints movement to along a line
        parallel to the ledge
* player's relative direction (forward, backward, left, right, up, down)
* pushable objects
* moving floors

### High Level API

Defined in the Unity Editor and limited to
a humanoid shape. Only supports basic
operations like run, jump, sprint, crouch.

Allows for user to configure parameters of each state.

Write some editor scripts to make selecting between
different options (like box, sphere, capsule collider)
easier.

## Implementation Notes

Once the design notes are finalized, we can write
up the implementation notes for how to update the
implementation based on this design.

### IKCCConfig

* Get rid of configurable KCCConfig.Up
    * replace it with `transform.Up`
* Move parameters related to movement such as `Up`
    into the KCCMovementEngine as most
    users don't care about configuring it.
    Users can override it if they need to.
* Move grounded state into the KCCMovementEngine
    as well for same reason as `Up`
* Bounces is override-able but isn't changeable
    in the editor.
* Combine parameters, `SnapUp * 1.25 = SnapDown`
    Only `SnapUp` is configurable.

### KCC Movement Engine

Exposed parameters

* Get rid of `Velocity`
    * Add a function to call `MovePlayer(Vector3 worldMove)`
    * Overloaded function `MovePlayerLocal(Vector3 localMove)`
        * `=> MovePlayer(transform.rotation * localMove)`
        * Relative to up vector.
    * Have it return the collisions and grounded state
        * `(GroundedState, IEnumerable<Collision>)`
* No gravity by default
    * Relative movement function relative to `Up` vector

### KCCUtils

Make a lightweight `struct` for all parameters
Remove concept of push.
