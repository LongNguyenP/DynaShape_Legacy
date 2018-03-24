# Changelog
All notable changes to this project will be documented in this file.

## [0.5.0.0] - 2018-03-24
### Changed
- No longer use AForge.Math and other external libraries for linear algebra. SVD Computation is now done inside the class FastSvd3x3

## [0.4.0.0] - 2018-02-27
### Added
- ConstantPressureGoal and ConstantVolumePressureGoal
- Silent execution mode (*Solver.ExecuteSilently* node) that does not output/display the intermediate results
- Some utilties for working with MeshToolkit meshes

### Changed
- Use AForge.Math for matrix math (Previous version uses Math.NET Numerics)