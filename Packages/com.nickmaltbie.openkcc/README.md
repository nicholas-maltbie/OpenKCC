# OpenKCC

This project is a sample of the Open Kinematic Character Controller.
A Kinematic Character Controller (KCC) provides a
way to control a character avatar as a kinematic object that will interact with
the environment.

OpenKCC is an open source project hosted at
[https://github.com/nicholas-maltbie/OpenKCC](https://github.com/nicholas-maltbie/OpenKCC)

This is an open source project licensed under a [MIT License](LICENSE.md).
Feel free to use a build of the project for your own work. If you see an error
in the project or have any suggestions, write an issue or make a pull request,
I'll happy include any suggestions or ideas into the project.


## Installation

Install the latest version of the project by importing a project via git
at this URL:
`https://github.com/nicholas-maltbie/OpenKCC.git?path=/Packages/com.nickmaltbie.openkcc/`

If you want to reference a specific tag of the project such as version `v1.0.0`,
add a `#v1.0.0` to the end of the git URL. An example of importing `v1.0.0`
would look like this:
`https://github.com/nicholas-maltbie/openkcc.git?path=/Packages/com.nickmaltbie.openkcc/#1.0.0`

For a full list of all tags, check the [OpenKCC Tags](https://github.com/nicholas-maltbie/ScreenManager/tags)
list on github. I will usually associated a tag with each release of the project.

_Note_: before I started using the package format for the project, I manually
released a unity package you needed to import. Any version before `v0.1.0`
will not work to import the project.

If you do not include a tag, this means that your project will update whenever
you reimport from main. This may cause some errors or problems due to
experimental or breaking changes in the project.

You can also import the project via a tarball if you download the source
code and extract it on your local machine. Make sure to import
via the package manifest defined at `Packages\com.nickmaltbie.openkcc\package.json`
within the project.

For more details about installing a project via git, see unity's documentation
on [Installing form a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html#:~:text=%20Select%20Add%20package%20from%20git%20URL%20from,repository%20directly%20rather%20than%20from%20a%20package%20registry.).

## Demo

You can see a demo of the project running here:
[https://nickmaltbie.com/OpenKCC/](https://nickmaltbie.com/OpenKCC/).
The project hosted on the website is up to date with the most recent
version on the `main` branch of this github repo
and is automatically deployed with each update to the codebase.

## Samples

The samples in the project include:
* ExampleFirstPersonKCC - Example first person character controller with a basic
    test scene.
* SimplifiedDemoKCC - Simplified character controller with basic movement scripts.

## Documentation

Documentation on the project and scripting API is found at
[https://nickmaltbie.com/OpenKCC/docs/](https://nickmaltbie.com/OpenKCC/docs/)
for the latest version of the codebase.
