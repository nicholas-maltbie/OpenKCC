# OpenKCC Cinemachine

This project is a sample of the Open Kinematic Character Controller.
A Kinematic Character Controller (KCC) provides a
way to control a character avatar as a kinematic object that will interact with
the environment.

OpenKCC is an open source project hosted at
[https://github.com/nicholas-maltbie/OpenKCC](https://github.com/nicholas-maltbie/OpenKCC)

This is an open source project licensed under a [MIT License](LICENSE.txt).
Feel free to use a build of the project for your own work. If you see an error
in the project or have any suggestions, write an issue or make a pull request,
I'll happy include any suggestions or ideas into the project.

## Installation

Make sure to add the required dependencies to your project

* [com.nickmaltbie.openkcc](https://nickmaltbie.com/OpenKCC/docs/index.html)
  version 1.3.0 or 

In order to use the samples in the project, make sure to also add the following
projects to your project.

Install the latest version of the project by importing a project via git
at this URL:

```
git+https://github.com/nicholas-maltbie/OpenKCC.git#release/cinemachine/latest
```

If you want to reference a specific tag of the project such as version `v1.3.1`,
add a `#release/cinemachine/v1.3.1` to the end of the git URL to download the package
from th auto-generated branch for that release. An example of importing `v1.3.1`
would look like this:

```text
git+https://github.com/nicholas-maltbie/OpenKCC.git#release/cinemachine/v1.3.1
```

### Scoped Registry Install

If you wish to install the project via a
[Scoped Registry](https://docs.unity3d.com/Manual/upm-scoped.html)
and npm, you can add a scoped registry to your project from all of the
`com.nickmaltbie` packages like this:

```json
"scopedRegistries": [
  {
    "name": "nickmaltbie",
    "url": "https://registry.npmjs.org",
    "scopes": [
      "com.nickmaltbie"
    ]
  }
]
```

Then, if you want to reference a version of the project, you simply
need to include the dependency with a version string and the unity package
manager will be able to download it from the registry at
`https://registry.npmjs.org`

```json
"dependencies": {
  "com.nickmaltbie.openkcc": "1.3.1",
  "com.nickmaltbie.openkcc.cinemachine": "1.3.1",
  "com.unity.cinemachine": "2.8.9"
}
```

### Tests

If you wish to include the testing code for this project, make sure to add
the `com.unity.inputsystem` and `com.nickmaltbie.openkcc` to the testables
of the project manifest.

```json
  "testables": [
    "com.nickmaltbie.openkcc",
    "com.nickmaltbie.testutilsunity",
    "com.unity.inputsystem"
  ]
```

In order to run the tests, you will need to import the [Moq](https://github.com/moq/moq)
library. My favorite way to import the `Moq.dll` in Unity is by using
[NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity).

## Samples

* CinemachineExample - Example of using cinemachine with the openkcc to control a character.

## Documentation

Documentation on the project and scripting API is found at
[https://nickmaltbie.com/OpenKCC/docs/](https://nickmaltbie.com/OpenKCC/docs/)
for the latest version of the codebase.
