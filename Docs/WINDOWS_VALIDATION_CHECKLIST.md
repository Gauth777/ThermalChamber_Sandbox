# Windows Unity validation checklist — integration-readiness pass
Target: Unity **2023.2.20f1**, built-in pipeline, Windows x64.
This checklist has NOT been executed in Unity by the implementation agent.
Core/rule tests: 27 passed; Unity import, API binding, generated scenes, Play Mode and player build remain unverified.

## 1. Update and open
1. In your existing full checkout, save/commit your local changes before switching branches. Do not use reset --hard.
2. Fetch origin and switch to `codex/thermal-twin-foundation`; pull with fast-forward only.
3. Unity Hub > Installs: use 2023.2.20f1. Add the Windows build module required by your selected scripting backend.
   For IL2CPP, also install the Windows SDK/C++ toolchain Unity requests; do not change backend merely to hide an error.
4. Hub > Projects > Add > choose the repository root containing Assets, Packages and ProjectSettings.
5. Wait for package import/compilation. Open Window > General > Console. Turn Collapse OFF.
   Expected: no red compiler/import errors. Existing FindObjectOfType deprecation warnings are not compilation failures.
6. If Thermal Twin is missing from the top menu, stop and copy the FIRST compiler error, filename, line and full stack/message.
   Report package-resolution failures separately, especially the pinned MCP Git dependency; Git must be on PATH.
7. Project Settings > Player > Other Settings > Active Input Handling: Input Manager (Old) or Both.
   The project currently uses Old. Restart if Unity requests it.

## 2. Test the imported FBX before generating
1. Exit Play Mode. Click **Thermal Twin > Run Editor Readiness Tests**.
2. Expected Console:
   - [TT-MATCH] Matched 4 heaters, 2 fan housings, 6 blades, 4 SHT31 sensors, globe, anemometer and door.
   - [TT-FOOTPRINT] Interior metres: min=... max=...
   - [TT-TEST] PASS: imported component mapping, footprint and negative reference checks.
3. The negative test intentionally renames a heater in a temporary preview scene. An
   [TT-MATCH] Unmatched actuator-like object: unmatched_heater warning during this test is expected.
   No generated or original scene is saved by this test.
4. If it fails, report [TT-TEST], [TT-MATCH] or [TT-FOOTPRINT] plus the complete exception.
   Include the FBX Inspector Model tab and the named object's imported Transform values.
   Do not fix a footprint failure by stretching the source model.

## 3. Generate the engineering scene
1. If EngineeringTwin already exists from the previous revision, preserve it: in Project > Assets > Scenes,
   rename it to EngineeringTwin_PreReadiness (choose another unique name if necessary).
   Likewise preserve ParticipantOffice as ParticipantOffice_PreReadiness if it was already generated.
   Do NOT rename/edit MainScene, SampleScene or the FBX.
2. Click **Thermal Twin > Create Engineering Scene**. Save your current scene if prompted.
3. Expected Console: component match summary, interior min/max, two [TT-FAN] lines,
   [TT-VALIDATE] PASS and [TT-SCENE] EngineeringTwin saved.
4. In Hierarchy verify:
   - Engineering Chamber - original FBX scale: one scene copy; original source asset is unchanged.
   - Fan_01_SimulationRotor and Fan_02_SimulationRotor: each has three blades.
   - Four IR_Heater objects have HeaterHeatSource and HeaterVisual.
   - Experiment: ChamberController, ThermalSimulation, ResearchDashboard, EngineeringLayout.
   - Thermal Field: 16 Zone objects. One Main Camera and one AudioListener.
5. Select Experiment. EngineeringLayout must contain 16 zones, four heaters, two fans and the occupant.
   ThermalSimulation > Configured Layout must point to this same EngineeringLayout.
6. Select Thermal Field and press F in Scene view. Inspect from above using wireframe if needed.
   Expected: tiles sit just above chamber_base, between the INNER XPS wall faces, with no cells beyond the usable floor.
   The floor excludes wall thickness and door protrusions. Compare logged dimensions with engineering drawings.
   These checks establish geometry consistency, not calibrated physical dimensions.
7. Click **Thermal Twin > Validate Open Scene**. Expected [TT-VALIDATE] PASS.
   Any [TT-VALIDATE] error is a stop condition. Copy ALL preceding individual errors, not only the final count.

## 4. Generate/register the companion scene
1. Click **Thermal Twin > Create Participant Office**. Expected [TT-VALIDATE] PASS and [TT-SCENE] ParticipantOffice saved and registered.
2. File > Build Settings: EngineeringTwin and ParticipantOffice should both be enabled, with EngineeringTwin first.
   Original scenes may remain listed afterward. The dedicated build command explicitly builds only the two generated scenes.
3. In Project > Assets > Scenes, double-click EngineeringTwin to return before pressing Play.

## 5. Play Mode checks
Use a Game view at least 1280 x 720. Click inside Game view to give it keyboard focus.

| Action | Expected visible result |
|---|---|
| Press Play | Console [TT-RUNTIME] Ready: 16 zones, 4 heaters. Simulation stopped. Dashboard appears at left. Initial 24 C, 55% RH, 0 m/s, Stopped. |
| Right-drag outside dashboard, middle-drag, scroll | Orbit, pan and zoom without an initial camera flip. Left-click a visible component and press F to frame its bounds. |
| Disable occupant heat; click Reset, then Start | Near 24 C with heater off and fan zero. Experiment clock advances. Charts start collecting samples. |
| Enable heater; wait 20–30 seconds | Temperature increases; spatial color changes start near heater locations and spread. Normal comfort temperatures use muted colors. No bright-red heater glow. |
| Fan to 50%, then 100% | Readout approximately 0.60 then 1.20 m/s; both three-blade assemblies rotate about their shared shaft, not the housing center. No detached blades or stretched housing. |
| Heater off, occupant off, Reset, Start, fan 100% | Uniform ambient field stays near 24 C. Fan circulation does not refrigerate it below ambient. |
| Reset, Start, occupant on, heater off | Slower local heating around occupant. Temperature is a grid average; air speed and RH are estimates. |
| Pause for 5 seconds | Experiment clock, temperature integration and chart sampling stop advancing. Resume with Start. |
| Stop | Fan/heater commands become zero/off. Temperature is retained; experiment does not advance. |
| Reset | Clock/history clear, temperature returns to 24 C, stopped state, fan/heater off. |
| Toggle spatial overlay | Tiles hide/show; source FBX materials are not overwritten. |
| Click Hardware without a provider | Readings become unavailable; disconnected/invalid/stale warning; simulation overlay hidden; actuator controls disabled. No fabricated hardware numbers. |
| Click Simulation | Clearly labelled simulation resumes displaying field values. Start is required to advance time. Clicking the already selected mode does not reset the session. |
| Run 10 seconds; Export displayed history to CSV | Dashboard shows Saved: full path. CSV has units, timestamps, source and mode, and numeric rows for collected samples. |
| Stop & open participant office | Office loads without missing-scene error. Return button loads a fresh engineering experiment. |

Opaque chamber walls may occlude internal tiles in Game view. Inspect tile placement in Scene view first.
For a diagnostic Play Mode view, temporarily disable only an obstructing wall's MeshRenderer in the Inspector
and re-enable it afterward. Do not delete walls, change source scale or disable the entire model.
Report absent overlays separately from occlusion.

## 6. Windows build and standalone run
1. Exit Play Mode. File > Build Settings > PC, Mac & Linux Standalone:
   Target Platform Windows, Architecture Intel 64-bit. Click Switch Platform if offered; wait for compilation.
2. Save the generated scenes. Click **Thermal Twin > Build Windows x64**.
3. Expected: two scene-validation passes, then [TT-BUILD] Windows x64 build succeeded.
   Do not infer success merely from the existence of an old .exe.
4. Launch Builds/Windows/ThermalChamber.exe. Keep its entire output folder intact.
5. Repeat startup, heater/fan, pause/reset, hardware-unavailable, CSV export and both scene-navigation checks.
   Test at 1280 x 720 and your normal desktop resolution. Close with Alt+F4.
6. Report any build errors with the first underlying Console error, plus the final build result.
   [TT-BUILD] messages identify missing module, wrong target, cancelled scene generation or failed build.

## What to send if a check fails
- Branch commit, Unity exact version, Windows build target and scripting backend.
- Checklist step and actions immediately before failure.
- Full FIRST red Console message and stack trace; also all [TT-*] diagnostics around that step.
- Screenshot of Game/Scene view and relevant object Inspector for visual/reference issues.
- Editor log: %LOCALAPPDATA%/Unity/Editor/Editor.log.
- Standalone log: %USERPROFILE%/AppData/LocalLow/<CompanyName>/<ProductName>/Player.log;
  CompanyName/ProductName are shown under Project Settings > Player.
- If using batch mode, include your explicit -logFile output.
- Common signatures: CSxxxx, NullReferenceException, MissingReferenceException, missing scene,
  prefab reparenting errors, InvalidOperationException about UnityEngine.Input, shader/material import errors.

## Protection and known limits
Original FBX, importer settings, MainScene and SampleScene are unchanged.
Generated scenes are not overwritten; material helpers reuse existing assets rather than replacing them.
Only the generated FBX scene instance is unpacked to allow blade reparenting.
Source name normalization accepts namespaces and .001-style suffixes but rejects ambiguous duplicate matches.
Footprint thresholds are plausibility checks; they do not prove engineering dimensions or thermal calibration.
No Unity compilation, Editor test, Play Mode run or Windows build is claimed until these local steps actually pass.
