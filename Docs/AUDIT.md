# Repository audit — 19 November 2026
Baseline: 408adef780d0000e8fbd6a179fe3f4ea84ed4d04 (main).

- Unity 2023.2.20f1, revision 0e25a174756c. Built-in render pipeline; GraphicsSettings has no custom pipeline.
- Project scenes: MainScene and SampleScene; a further 30 scenes belong to imported TMP examples.
  The user's currently open Editor scene cannot be determined from repository data.
- MainScene contains 16 thermal-zone prefab instances, prototype fan/heater/occupant, a TMP dashboard,
  an imported FBX instance, and an unpacked thermal_chamber_LengthTest with local scale (1,1,1.5).
- Original EditorBuildSettings had an empty scene list.
- Eight custom scripts: ChamberController, EnvironmentSimulator, EnvironmentUI, FanVisual,
  HeaterHeatSource, HeaterVisual, OccupantHeat, ThermalZone. No FanAirflowSource script was present.
- Prefabs: ThermalZone and EnvironmentSensor. EnvironmentSensor still contains serialized fields
  from an earlier oscillating sensor implementation; those do not define a hardware source.
- The FBX was retrieved and its binary node/property records inspected. It contains separately named
  two fan housings/six blades, four IR heaters, four SHT31 sensors, globe sensor/stand,
  anemometer components, door/hinges/handle and chamber structure.
  Global properties report Y up and UnitScaleFactor=1. Unity importer uses file units/globalScale=1.
  Actual imported dimensions, visual axis conversion and geometry require Editor verification.
- Custom script GUIDs referenced by MainScene resolve to the repository's existing script metas.
  UI/TMP package references require package restore and Unity's missing-script validation.
  No claim is made that all scene references resolve in Unity.
- Existing thermal integration ran independently per zone in Update, used neighbors' mutable
  temperatures, added heater/occupant values directly as Celsius rates, clamped temperatures
  to 20–32 C, and interpreted fan circulation as direct cooling.
- HeaterVisual used bright red and dereferenced an optional renderer. EnvironmentUI dereferenced
  optional TMP fields without guards. No experiment lifecycle, recorder, middleware interface,
  production desktop camera or build entry point existed in the eight project scripts.
- Repository main was accessed through connected GitHub tooling; command-line clone failed with
  'Could not resolve host: github.com'. The implementation branch preserves the full base Git tree.
  Local sources are an inspection/implementation subset, not a complete cloned Unity checkout.
- Unity was absent from PATH, conventional Program Files locations, observed processes and
  queried installed-application entries. Nonstandard installations cannot be excluded.
  dotnet --list-sdks returned none. Windows .NET Framework csc was available and executed.

Original scene, prefabs, model bytes and existing script GUIDs are preserved.
No remote main changes or destructive scene transformations are made.
