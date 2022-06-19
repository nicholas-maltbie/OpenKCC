# Changelog

All notable changes to this project will be documented in this file.

## In Progress

Major Refactor
* Converting project to follow unity package layout
* Moved example FPS cahracter into example first person cahracter sample folder
* Moved simplified demo examples to demo sample folder
    * Also fixed dependency issues so each sample can be imported independently
* Setup package build workflow to copy `Assets/Samples` to
    `./Packages/com.nickmaltbie.openkcc/Samples~` and validating github workflow
    to ensure this works as expected.
* Reorganized assets to have all folders in `Assets` directory
* Created a common folder for assets shared between mulitple samples
    `./Packages/com.nickmaltbie.openkcc/Common` - will keep small files
    and assets here that are shared (but avoid large models and textures).

Minor Fixes
* Improved setup package script to fix an error with `git-lfs` files.
* Updated project dependencies definitions and install instructions.

## [0.0.63] - 2022-06-10

Start of changelog.
