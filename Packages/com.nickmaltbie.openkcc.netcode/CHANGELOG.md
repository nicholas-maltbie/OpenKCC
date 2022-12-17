# Changelog

All notable changes to this project will be documented in this file.

## In Progress

* Setup a `NetworkRelativeParent` and `NetworkRelativeTransform` classes
    to manage the relative position of an object to a parent object
    without having to actually parent the objects as is required
    by the `NetworkTransform` class.
* Added a basic NetworkMovingPlatform class.
* Added tests for basic network behaviours and test framework.
* Setup class for managing netcode tests `NetcodeInputRuntimeTest`
* Added basic tests for the `NetworkKCC`
* Added controls for loading and configuring ssl connection
    * Also added field for supplying a private key for the server hosting.
* Change port configuration for web socket to use secure connection over https.
* Added example using unity Netcode and sub package com.nickmaltbie.openkcc.netcode
    * Added example NetworkKCC for a networked kinematic character controller.
    * Included Networked camera controls and networked camera follow.
    * Setup basic NetworkKCC based on the NetworkSMAnim.
