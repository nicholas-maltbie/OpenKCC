# Changelog

All notable changes to this project will be documented in this file.

## In Progress

* Small fixes to mole example movement, need to re-design the camera and mole
    movement interactions so they work together better.
* Significantly improved the mole example and added color changing particles.
* Fixed a bug where the mole would fall off walls.

## [1.3.3] 2023-2-16

* Updated package to use statemachineunity.netcode instead of statemachineunity.netcode
    package.

## [1.3.0] 2023-1-29

* Added documentation to the `MoleMovementEngine`.
* Refactored the `NetworkKCC` and Mole Sample to use the new `KCCMovementEngine`.
    Also simplified parameters to follow the same design as `KCCStateMachine`.
* Added giant to climb on for the mole demo
* Touched up MoleKCCSample to be better configured
* Updated NetworkKCC and MoleCharacter to use newer KCCMovementEngine APIs.

## [1.2.3] 2023-1-17

* Updated readme with notes on how to install required libraries.
* Refactored samples to fix import errors and make downloading dependencies
    easier.
* Added simple `OfflineNetworkTransport` and added ability to play
    the demo in an offline for netcode.

## [1.2.2] 2023-1-15

* Corrected some issues with samples path for export.

## [1.2.1] 2023-1-15

* Removed rotation synchronization from the network relative transform.
    Will overhaul the class in the future.
* Small fixes to mole multiplayer synchronization and tails.

## [1.2.0] 2023-1-15

* Setup basic mole character controller using the `MoleMovementEngine`
    in the sample `MoleKCCSample`.
* Refactored `NetworkKCC` to use the newly added `KCCMovementEngine`.

## [1.1.3] 2023-1-1

* Refactored code to use com.nickmaltbie.recolorshaderunity
* Fixed code reference to IEvent for backwards compatibility.

## [1.1.2] 2022-12-31

* Fixed materials in samples to be included in the package folder.
* Fixed basic animations for network kcc so they sync properly.

## [1.1.0] 2022-12-18

* Updated NetworkMovingPlatform and FixedRigidbodySet to move in the `Update`
    function to synchronize with server tick properly.
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
