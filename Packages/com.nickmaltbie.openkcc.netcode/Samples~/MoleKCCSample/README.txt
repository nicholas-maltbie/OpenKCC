Mole KCC Sample

This requires some common code in the NetcodeExample sample to work properly.

Also requires pro builder, unity input system, and a few other libraries
to work properly.

Update your manifest to include them:

```json
{
  "scopedRegistries": [
    {
      "name": "nickmaltbie",
      "url": "https://registry.npmjs.org",
      "scopes": [
        "com.nickmaltbie"
      ]
    }
  ],
  "dependencies": {
    "com.community.netcode.transport.websocket": "git+https://github.com/Unity-Technologies/multiplayer-community-contributions?path=/Transports/com.community.netcode.transport.websocket",
    "com.nickmaltbie.recolorshaderunity": "1.0.0",
    "com.unity.inputsystem": "1.4.4",
    "com.nickmaltbie.testutilsunity": "1.0.0",
    "com.unity.netcode.gameobjects": "1.1.0",
    "com.unity.probuilder": "5.0.6"
  }
}
```
