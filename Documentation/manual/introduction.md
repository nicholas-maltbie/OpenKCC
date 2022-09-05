# Introduction to OpenKCC

The Open Kinematic Character Controller (OpenKCC)
is an open source Kinematic Character Controller (KCC)
that allows for a player moving
as a kinematic object through a scene. This is done through using unity's
physics engine and projection based movement.

See [Example Usage](usage.md) for how to use the Kinematic Character Controller
in the project.

## Design Notes

For information detailing how the project works and the design of the project,
see the [KCC Design Overview](design/overview.md) for a review of how the
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

The project is organized into a few different folders.

* `Assets\Samples` - OpenKCC Sample scenes and examples for the project.
* `Packages\com.nickmaltbie.openkcc` -
  main package folder with all the code, tests, and shared assets.
  * `Packages\com.nickmaltbie.openkcc\common` -
    Common prefabs, materials, and assets used across the entire project.
  * `Packages\com.nickmaltbie.openkcc\Editor` -
    Editor specific assets for the project configuration,
    not included in builds.
  * `Packages\com.nickmaltbie.openkcc\OpenKCC` -
    Main project source code and assemblies.
  * `Packages\com.nickmaltbie.openkcc\Tests` -
    EditMode and PlayMode tests for the project.
