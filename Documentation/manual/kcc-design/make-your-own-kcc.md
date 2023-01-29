# Make Your Own KCC

Summary on how to modify the parameters of
the OpenKCC and how to customize the KCC
for your own goals.

There are different was to the OpenKCC
api depending on what you want to do with the
library. They are described in greater
detail in the [KCC User Stories](kcc-user-stories.md)
document.

- Humanoid Character - Use the pre-existing character
    controller for @nickmaltbie.OpenKCC.Character.KCCStateMachine
    or write your own using the provided
    @nickmaltbie.OpenKCC.Character.KCCMovementEngine.
    If you use a different implementation of
    @nickmaltbie.OpenKCC.Utils.IColliderCast you can support
    other kinds of character shapes such as sphere,
    box or composite.
- Custom Movement - Modify the @nickmaltbie.OpenKCC.Character.KCCMovementEngine
    to support new types of movement such as climbing,
    flying or falling. See the
    @nickmaltbie.OpenKCC.MoleKCCSample.MoleMovementEngine
    and @nickmaltbie.OpenKCC.MoleKCCSample.MoleCharacter
    for an example of mole character controller that
    can climb up walls.
- Low Level Customization - Make calls directly
    to the [KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils)
    API to allow for very custom interactions that aren't
    provided by the existing movement engine implementations.

## Humanoid Character Case

Use the provided @nickmaltbie.OpenKCC.Character.KCCStateMachine
for a basic humanoid character.

Supports basic movement for a humanoid character
and optional jump, max walking speed, and a
few other supported options.

See the [Usage](../usage.md) document for more
details on how to use the existing @nickmaltbie.OpenKCC.Character.KCCStateMachine.

If you want to use a new kind of movement
that's not supported in the @nickmaltbie.OpenKCC.Character.KCCStateMachine,
you can make calls directly to the
@nickmaltbie.OpenKCC.Character.KCCMovementEngine
for basic kinds of humanoid movement such as climbing,
gliding, flying, etc... that would require some
custom management of velocity
and grounded state.

## Custom Movement

If you wish to add new kinds of custom movement
or change what a character does on collision
with objects, you can modify the @nickmaltbie.OpenKCC.Character.KCCMovementEngine
and write a custom handler for managing
the player movement.

An example of this is implemented in
the mole sample and you can check the files
@nickmaltbie.OpenKCC.MoleKCCSample.MoleMovementEngine
and @nickmaltbie.OpenKCC.MoleKCCSample.MoleCharacter
for further details.

You can modify how the @nickmaltbie.OpenKCC.Character.KCCMovementEngine
makes calls to the
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils)
and change what happens after each bounce or how
different parts of movement and grounded state are
calculated.
The @nickmaltbie.OpenKCC.MoleKCCSample.MoleMovementEngine
has a simple check to allow the character to slide
up walls if they run into a sharp angle
and allows for the mole to be considered
"grounded" no matter what angle they
are standing at.

## Low Level Customization

If you want to be entirely custom in the character
and don't want to rely on the
@nickmaltbie.OpenKCC.Character.KCCMovementEngine
you can make calls directly to
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils).

Most of the functions only require
the player's current position, rotation,
and some configuration parameters
provided by the @nickmaltbie.OpenKCC.Utils.IKCCConfig ,
For an example implementation of the
@nickmaltbie.OpenKCC.Utils.IKCCConfig ,
look at the @nickmaltbie.OpenKCC.Character.KCCMovementEngine
for example parameters for a humanoid character.

The publicly exposed APIs in the
[KCCUtils](xref:nickmaltbie.OpenKCC.Utils.KCCUtils)
allow for collision computation and sliding off
objects with support for operations
like snapping up stairs.
