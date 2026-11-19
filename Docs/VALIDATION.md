# Validation status — 19 November 2026
## Actually executed
The pure C# ThermalField/ExperimentSession/EnvironmentalReading sources were compiled by the installed
Windows .NET Framework csc and run by Tests/Run-CoreTests.ps1. All **20 tests passed**:
heater energy balance, heater off, local occupant power, sealed diffusion energy conservation,
permutation independence, fan-enhanced mixing, no fan refrigeration, fan/airflow mapping,
stiff-system bounds, identical fixed-step schedules, pause, stop, reset, mode switching,
invalid numerical/configuration inputs, stale/future/nonfinite hardware packet rejection,
default packet rejection, total localized power conservation, out-of-field source behavior,
and invalid heat-source rejection.

These tests validate the C# core, not Unity compilation or complete scene behavior.
Tests/CoreTests.cs is executable outside Unity and is intentionally outside Assets.

## Not executed / blocked
- Unity import, full C# project compilation, Edit Mode/Play Mode tests and scene-reference validation.
- Generated EngineeringTwin/ParticipantOffice scenes: generators are implemented, but no Unity scene
  generation has been executed. Original scenes remain unchanged.
- Windows x64 build and launching/testing the executable.
- Visual appearance, FBX material fidelity, internal dimensions, rotor axis/pivot, camera framing,
  UI text/layout at target resolutions and export behavior in a player.
- Actual hardware integration, disconnect/reconnect tests and any Quest/XR run.

Unity Editor and Windows build support are the current build blocker; no exe is delivered.
Command-line repository download also failed DNS resolution; GitHub API operations worked.

## Required next validation on Unity machine
1. Restore packages with Unity 2023.2.20f1; inspect Console and resolve all compiler/import errors.
2. Create EngineeringTwin, run ThermalTwinValidation.ValidateOpenScene, inspect original-scale FBX.
3. Verify physical units/interior bounds; correct only documented mappings, not source dimensions.
4. Play: start, heat, fan %, occupant on/off, pause, resume, stop and reset. Confirm thermal gradients.
5. Check missing renderer/TMP/provider paths and mode switching; hardware without provider must show unavailable.
6. Check rotor pivots/axes, thermal toggle, selection, zoom/pan/orbit, charts and exported units/source labels.
7. Create participant office, build Windows and test both scene transitions in the standalone application.
8. Profile long runs and recording, then calibrate against measured chamber response before research claims.

Known limitations: prototype IMGUI styling, primitive office, one chamber per scene, bounded history,
no live topology rediscovery, default uncalibrated parameters and no automated Unity scene tests yet.
