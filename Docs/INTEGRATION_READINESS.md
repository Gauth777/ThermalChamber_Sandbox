# Unity integration-readiness review
Branch baseline: 228bd511fba1a7b755af65f79b1004ebb927f7aa.
Unity target: 2023.2.20f1; built-in pipeline; legacy Input Manager.

## Corrections
- Scene-only FBX instance is fully unpacked before reparenting blades. Original model/importer unchanged.
- Required component names are normalized for namespace prefixes and numeric duplicate suffixes.
  Missing required names or ambiguous duplicates fail explicitly; extra actuator-like objects warn.
- Original FBX records show three blades sharing the same origin, offset from the housing center.
  Rotor center now uses the shared blade origin; shaft direction uses the housing-to-blade offset.
  Coincident/ambiguous geometry aborts instead of choosing a guessed axis.
- Usable floor uses chamber_base intersected with opposing XPS wall inner faces; ceiling defines height.
  Full-FBX bounds no longer define the thermal grid. Implausible units/orientation fail with diagnostics.
- Generated EngineeringLayout holds all source/grid references. Grid links use row/column adjacency.
- Missing/disabled sources and malformed grid references fail initialization/validation clearly.
- Existing material assets are reused. Shader availability and file-type conflicts are checked.
- Generation checks save results and restores prior scene setup on failure; generated scenes are never overwritten.
- Both scenes are registered for Play Mode navigation. Return navigation is disabled if target is unavailable.
- Build preflight requires Windows x64 target/module and validates both saved scenes before building.
- Camera pitch is normalized; participant view no longer reserves an invisible left dashboard strip.
- Dashboard restores global IMGUI state; scene transitions use asynchronous loading; export creates its output directory.
- Default occupant marker fits below the derived ceiling.

## Reviewed runtime files
ChamberController, ThermalSimulation, ThermalField, ThermalZone, HeaterHeatSource, OccupantHeat,
FanVisual, HeaterVisual, EnvironmentSimulator, EnvironmentUI, ResearchDashboard, InspectionCamera,
ParticipantRoomNavigation, plus new EngineeringLayout and ChamberLayoutRules.
Legacy MonoBehaviour names and GUIDs remain compatible. No XR/visual-feature expansion was added.

## Executed
27 pure C# tests compiled and passed, including seven new matching/footprint regression tests.
Roslyn syntax parsing: all 21 project C# files passed. This does NOT bind Unity assemblies.
Unity Editor was again absent from PATH/conventional install location.
No Unity import, Editor readiness tests, scene generation, Play Mode or Windows build was executed.

## Supplied but not executed
Thermal Twin > Run Editor Readiness Tests validates actual imported FBX mapping/footprint
and negative missing-object/duplicate-name checks in a disposable preview scene.
Thermal Twin > Validate Open Scene reports required references, counts, grid placement,
heater reach, rotor children and cameras/materials with [TT-VALIDATE] messages.

## Versioned API references reviewed
- https://docs.unity3d.com/2023.2/Documentation/ScriptReference/PrefabUtility.UnpackPrefabInstance.html
- https://docs.unity3d.com/2023.2/Documentation/ScriptReference/PrefabUtility.InstantiatePrefab.html
- https://docs.unity3d.com/2023.2/Documentation/ScriptReference/SceneManagement.EditorSceneManager.NewPreviewScene.html

The next acceptance gate is the [Windows checklist](WINDOWS_VALIDATION_CHECKLIST.md).
Footprint dimensions still require comparison with engineering drawings; thermal constants remain uncalibrated.
