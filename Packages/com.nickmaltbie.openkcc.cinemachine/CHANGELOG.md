# Changelog

All notable changes to this project will be documented in this file.

## In Progress

* Removed `Moq` dependency for tests.
* Updated project version to 20202.2.16f1 and added automated tests for
    backwards compatibility for versions 2019.4, 2020.3, 2021.3

## [1.3.3] 2023-2-16

* Small update to cinemachine example settings for first person vcam.

## [1.3.1] 2023-2-13

* Added basic `SmoothAvatarPos` to smooth avatar position based on some damping
    factor to smooth out any jittery movements of the player when snapping
    up or down surfaces.
* Added basic test scene and scripts for different camera perspectives including:
    * Over the shoulder camera
    * Third person re-centering camera
    * First person camera
* Initial setup of cinemachine project.
