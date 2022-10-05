# Introduction to OpenKCC

The Open Kinematic Character Controller (OpenKCC)
is an open source Kinematic Character Controller (KCC)
that allows for a player moving
as a kinematic object through a scene. This is done through using unity's
physics engine and projection based movement.

See [Example Usage](usage.md) for how to use the character controller
in a unity project.

## Why Kinematic?

In the real world, every object has mass and interacts with objects around it.
However, these kinds of "real" interactions can feel unnatural in a virtual
environment. A kinematic object can push objects around it
but not be pushed itself. This is useful for a character as it allows for
moving around the virtual space with a high degree of control instead of just
bouncing randomly off anything it collides with.

See the [KCC Design Overview](kcc-design/overview.md) for more details on how
KCC moves around and different kinds of physics objects.

## Design Notes

For information detailing how the project works and the design of the project,
see the [KCC Design Overview](kcc-design/overview.md) for a review of how the
kinematic character controller works and detailed design descriptions.

## Learning

As these videos are created they will be listed here:

* [Designing Character Controllers Intro](https://youtu.be/Hv4CQMCxSWE)
* [Physics Behind Games and Character Interactions](https://youtu.be/rzD-Lm8pOX0)
* [Projection Based Movement of KCC](https://youtu.be/s-99Z_W8bcQ)
* [How the KCC Manages Jumping](https://youtu.be/CGsDdBZa5EM)
* [Camera controller and Dither Shader](https://youtu.be/Zw6qvOOHGC4)
* \[Planned\] Character Controller Case Study and Requirements Engineering

In the future, I will write up more wiki articles on how to use the KCC in your
own project, how to modify the OpenKCC
to add custom features, and on more in depth summaries on how the OpenKCC works
as described in the videos above.

## Project Organization

The project is organized into a few different folders, here are some of
the important folders for using and learning about the project:

* `Assets\Samples` - OpenKCC Sample scenes and examples for the project.
* `Packages\com.nickmaltbie.openkcc` -
    main package folder with all the code, tests, and shared assets.
    * `Packages\com.nickmaltbie.openkcc\common` -
        Common prefabs, materials, and assets used across the entire project.
    * `Packages\com.nickmaltbie.openkcc\Editor` -
        Editor specific assets for the project configuration,
        not included in builds.
    * `Packages\com.nickmaltbie.openkcc\FSM` -
        State Machine code for the project.
    * `Packages\com.nickmaltbie.openkcc\OpenKCC` -
        Main project source code and assemblies.
    * `Packages\com.nickmaltbie.openkcc\Tests` -
        EditMode and PlayMode tests for the project.

The rest of the assets folder contains code for render pipeline and
settings, feel free to look through them if you want an example
configuration.

## Testing

To see a summary of the tests included in the project and how
to run them, see the [Test Design](test-design.md) document.
