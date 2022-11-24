# OpenKCC Netcode

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

You can see a demo of the project running here:
[https://nickmaltbie.com/OpenKCC/NetcodeExample](https://nickmaltbie.com/OpenKCC/NetcodeExample).

The project hosted on the website is up to date with the most recent
version on the `main` branch of this github repo
and is automatically deployed with each update to the codebase.

## Installation

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
  "com.nickmaltbie.openkcc": "1.0.1",
  "com.nickmaltbie.openkcc.netcode": "1.0.1",
  "com.nickmaltbie.screenmanager": "3.0.0",
  "com.nickmaltbie.statemachineunity": "1.1.0",
  "com.nickmaltbie.networkstatemachineunity": "1.0.0",
  "com.nickmaltbie.testutilsunity": "0.0.2",
  "com.unity.inputsystem": "1.0.0",
  "com.unity.textmeshpro": "3.0.0"
}
```

### Tests

If you wish to include the testing code for this project, make sure to add
the required projects to the testables
of the project manifest.

```json
  "testables": [
    "com.nickmaltbie.openkcc",
    "com.nickmaltbie.openkcc.netcode",
    "com.nickmaltbie.testutilsunity",
    "com.unity.inputsystem",
    "com.unity.netcode.gameobjects"
  ]
```

## Demo

You can see a demo of the project running here:
[https://nickmaltbie.com/OpenKCC/NetcodeExample](https://nickmaltbie.com/OpenKCC/NetcodeExample).
The project hosted on the website is up to date with the most recent
version on the `main` branch of this github repo
and is automatically deployed with each update to the codebase.

## Samples

The samples in the project include:

* ExampleFirstPersonKCC - Example first person character controller with a basic
  test scene.

## Documentation

Documentation on the project and scripting API is found at
[https://nickmaltbie.com/OpenKCC/docs/](https://nickmaltbie.com/OpenKCC/docs/)
for the latest version of the codebase.
