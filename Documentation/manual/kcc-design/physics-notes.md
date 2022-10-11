# Physics for Character Controllers

This design document goes over how Character Controllers manage physics
interactions with the virtual environment as well as well as a basic
overview of physics objects in Unity.

Here is the companion YouTube video for this design document:

<!-- markdownlint-disable MD013 -->
<!-- markdownlint-disable MD033 -->
<!-- Disable line length lint rule for portion of embed -->
<div class="videoWrapper">
<iframe
    src="https://www.youtube.com/embed/rzD-Lm8pOX0"
    title="Explaining The Physics Behind Character Controllers - OpenKCC"
    frameborder="0"
    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
    allowfullscreen></iframe>
</div>
<!-- markdownlint-enable MD013 -->

## Physics in Virtual Environment

In almost every game, a player will interact somehow with the environment
whether it is simply not clipping through objects and moving through a
static scene such as in Celeste. Or a more complex interaction with any
moving objects such as in Fall Guys.

When designing a character controller, it’s key that their interactions are
well defined with the physics environment to create a natural response.
You wouldn’t want to have a car in racing game struggle to scale a small hill
nor would you want a player to be shoved through the floor when opening a door.

Computers resources are not infinite, objects in a virtual environment
are much more simplified than their real world counterparts. This allows
for a computer to save time and resources when rendering objects or simulating
interactions. This is essential if a game wants to maintain a high refresh
rate. Additionally, some concepts in the real world aren't useful for
virtual environments.

For example, whenever you stand or sit on something, you technically push
down on that object and that object pushes up on you. However, if you
create a virtual scene with a table, you probably don't need to simulate
the weight of each object pressing down on the table and instead just want to
worry about where to place the objects on the table.

In this scenario the table would be a static while the objects placed on top
of it would be considered dynamic.

## Physics Colliders

An important concept to differentiate here is a collider versus the render mesh
of an object. The collider is how it will interact with the physics engine
while the render mesh is how it is displayed to the end user.

For example, think of an item like a wine glass. In most scenarios in a video
game, you probably want the glass to sit upright on a table and fall on its
side when it is knocked over.

In this scenario, the render mesh for the glass may be a complex object that has
many curves and faces, but the collision mesh could be a simple cube to help
it stay upright. This way, when the computer has to calculate what the wine
glass is touching, it only needs to think of it like a cube instead of
the very complex custom object.

![Example Wine Glass Box Collider and complex Shape](../../resources/design/winglass-demo.png)

## Physics Objects

Modern game engines and 3D editing and simulation software such as Unity or
blender contain a physics system for virtual
environments that support moving objects. Within these systems, there are
generally three different kinds of physics objects:

1. Dynamic - Has a collider, velocity and mass.
1. Static - Has a collider.
1. Kinematic - Has a collider and velocity but does not have mass.

### Physics Types - Pool Example

Talking about these objects in an abstract world is hard to understand.
A simple example to visualize them together is a game of pool.

The pool balls are dynamic objects that move around. They have a mass, shape,
and deflect off each other when they collide. These can be pushed by other
objects and other objects can push them.

When the balls hit the sides of the table they bounce off.
No matter how fast the pool balls move, they won’t push the table around.
In real life, this is achieved through large objects or a sturdy support,
but in games it’s easier to simply cheat and mark static
objects which don’t move and save the computer some effort.

Also, whenever a ball is hit with a pool cue, the cue pushes the ball around.
For the most part, the balls should never push the cue around. Since this cue
can push the balls but the balls cannot push the cue, the cue would be a
dynamic object.

For me, it’s helpful to visualize these objects in a grid with the horizontal
axis defining if other objects push this, and the vertical axis defining if
this can push other objects.

![Visualization of different types of physics objects by interaction with other objects](../../resources/design/physics-types.png)

Dynamic objects like the pool balls would be in the top left both pushing each
other and being pushed by other objects.
The pool table is in the bottom right and both does not move and cannot be moved
by other objects. And the pool cue which can push objects but not be pushed
itself would be in the top right where it can push the pool balls around but
should not be pushed by other objects.  

## Character Controller Options

As discussed earlier, if a game has moving objects, and the player needs to
interact with those objects, the character controller interactions with
the physics system must be well defined.

Each of the physics object types we went over earlier can be converted
into a character controller using assets in the Unity game engine.

1. Built-In Character Controller - Static
1. Rigidbody Character Controller - Dynamic
1. Kinematic Character Controller - Kinematic

### Built-In Character Controller

The Built-In [Character Controller](https://docs.unity3d.com/Manual/class-CharacterController.html)
is unity's default character controller component that has some basic
capabilities for movement but does not have any defined interactions
with the world around the player besides having a collider.

For a use case for a static character controller, look at the game
Portal 2. It has very few moving and colliding objects. Great, we can use
a static character controller where you simply have the character
move via teleporting short distances.

This is the normally the built-in default character controller in most game
engines. Unity’s is simply called a “Character Controller” and will be
referred to as a “built-in” character controller from here on.

This built-in character controller won’t interact with physics objects unless
you write some custom code. The built-in character controller has some nice
features like gravity, walking up and down stairs, and not sliding down small
hills.

### Rigidbody Character Controller

For an example of a rigidbody character controller, let’s look at another game,
Rocket League. where each player controls a car rocking around hitting a giant
soccer balls into goals around an arena.

This would be an example of a dynamic object. In Unity, dynamic objects are
achieved using a [Rigidbody](https://docs.unity3d.com/Manual/class-Rigidbody.html)
component, since this component give an object mass, gravity,
and collisions. This will be referred to as a rigidbody character controller
from now on.

Rigidbody controllers sometimes come with a few caveats such as not being
able to stand still on slopes due to gravity pulling them down. And
getting stuck on the edge of steps when attempting to walk up and down them.
This is great for some games like Rock of Ages where you roll a boulder down
a hill at fast speeds but not as fun for platforming
or first-person shooters where precision is key.

### Kinematic Character Controller

Let’s move to the last kind of physics object, kinematic.
Kinematic Character Controllers, or KCC for short, are another important
addition. Just as with kinematic objects, these can push objects around them,
but other objects do not push them.
They are very similar to the built-in character controller but has a
few key advantages:

1. They can use different shapes the built-in character controller.
1. They come with physics interactivity.
1. They can be any kind of object and aren’t limited to humanoids.
    you could even use a kinematic character controller to make something
    like a pool cue.

## Selecting a Character Controller

Now we get to the hard part, how do you decide what kind of character
controller to use in your game?

First, we can look at the physics interactions, should the character
push other objects, and can it be pushed by other objects. This we can
fill out with information from our physics types we discussed earlier.
Remember, the built-in character is static, the rigidbody is dynamic,
and kinematic character controller is kinematic.

The rigidbody and kinematic character controllers can easily push other
objects and the built-in character controller can push objects but would
require some custom modifications. Both the built-in and kinematic character
controller will not be pushed by other objects while the rigidbody will be
pushed by other objects. This might be enough for determining how your
player will interact with the world around them but if it’s not, there
are a few more criteria you could consider.

|  | Built-In (static) | Rigidbody (dynamic) | KCC (kinematic) |
|:-|:------------------|:--------------------|-----------------|
| Push Other Objects | It’s Complex | Yes | Yes |
| Pushed by other Objects | No | Yes | No |
| Must be Humanoid | Yes | No | No |

This description is not exhaustive and there is still more left to cover.
Consider how objects in a game you like to play may be divided up.

## Extra Info

Not all character controllers are physics based, there are entirely
UI based such as one of my favorites, A Dark Room. I also love playing base
building and strategy games like Star Craft and the Civilization series,
they usually have an omniscient perspective and UI based controls by clicking
on elements in the environment or tapping with touch screens.
