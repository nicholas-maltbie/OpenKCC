# Changelog

All notable changes to this project will be documented in this file.

## In Progress

* Switched to using com.nickmaltbie.StateMachineUnity library
* Added Finite State Machine design docs and a decorator
    based code implementation to the project.
* Added KCCStateMachine implementation and changed relevant examples to use
    this state machine.
* Updated follow object to use unity's ParentConstraint
    examples for following parent objects.
* Updated unity version of project to v2021.3.11f1
* Adding test coverage for EditMode tests

## [0.1.2] - 2022-06-26

Small patch to automated release workflow for npm

## [0.1.1] - 2022-06-21

Bumping project version to validate autoamted npm release workflow.

## [0.1.0] - 2022-06-19

Major Refactor

* Converting project to follow unity package layout
* Moved example FPS cahracter into example first person cahracter sample folder
* Moved simplified demo examples to demo sample folder
    * Also fixed dependency issues so each sample can be imported independently
* Setup package build workflow to copy `Assets/Samples` to
    `./Packages/com.nickmaltbie.openkcc/Samples~` and validating github workflow
    to ensure this works as expected.
* Created a common folder for assets shared between mulitple samples
    `./Packages/com.nickmaltbie.openkcc/Common` - will keep small files
    and assets here that are shared (but avoid large models and textures).
* Emptied all assets from `Assets\OpenKCC` to avoid complexity.

Minor Fixes

* Improved setup package script to fix an error with `git-lfs` files.
* Updated project dependencies definitions and install instructions.

Documentation

* Added notes on how to install the project via npm js registry

## [0.0.63] - 2022-06-10

Start of changelog.
