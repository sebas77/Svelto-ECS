# Changelog
All notable changes to this project will be documented in this file. I created this file with Svelto.ECS version 3.1.

## [3.1.3]

### Fixed

* bumped dependency of Svelto.Common due to an important fix there.

## [3.1.2]

### Changed

* improved async entity submission code (still experimental)
* improved native entity operations debug info

## [3.1.1]

### Changed

* SubmissionEngine didn't need the EntityManager property, so it has been removed

## [3.1.0]

### Changed

* rearrange folders structures for clarity
* added DoubleEntitiesEnumerator, as seen in MiniExample 4, to allow a double iteration of the same group skipping already checked tuples
* reengineered the behaviour of WaitForSubmissionEnumerator
* removed redudant SimpleEntitySubmissionSchedulerInterface interface
* renamed BuildGroup in to ExclusiveBuildGroup
* renamed EntityComponentInitializer to EntityInitializer
* Entity Submission now can optionally be time sliced (based on number of entities to submit per slice)
* working on the Unity extension Submission Engine, still WIP
* added the possibility to hold a reference in a EntityViewComponent. This reference cannot be accesses as an object, but can be converted to the original object in OOP abstract layers
* renamed NativeEntityComponentInitializer to NativeEntityInitializer

### Fixed

