# Thermal Chamber Digital Twin
Unity 2023.2.20f1 · built-in render pipeline · Windows desktop first.

## Current status
This branch is an implemented engineering foundation, **not a validated industrial release**.
The pure C# solver passes 27 executable regression tests. Unity compilation, imported model appearance,
Play Mode checks and Windows standalone build have not been executed in the implementation environment:
Unity Editor/Hub was not found. Hardware and XR are not implemented.

## Open and run
1. Clone this branch with Git on a machine with network access.
2. Install Unity **2023.2.20f1** and Windows Build Support through Unity Hub, then open the project.
3. Allow package restore. The MCP Git dependency is pinned to the already locked commit.
4. Use **Thermal Twin > Create Engineering Scene**. This generates
   `Assets/Scenes/EngineeringTwin.unity` from the original FBX, without scaling the source model.
   It deliberately refuses to replace an existing generated engineering scene.
5. Run **Thermal Twin > Validate Open Scene**, enter Play Mode, then click Start.
6. Use **Thermal Twin > Build Windows x64**. This also creates the independent participant office
   scene if absent and builds `Builds/Windows/ThermalChamber.exe`.
7. Ship the entire Windows output directory, not only the executable.

The original MainScene and FBX are retained. MainScene uses the updated simulation via its existing
ChamberController and keeps the old TMP display. Its length-test object is still stretched; use the
generated engineering scene for model inspection. Build Settings now includes MainScene as a fallback;
the dedicated build entry point explicitly uses the generated EngineeringTwin and ParticipantOffice.

## Desktop controls
- Right drag: orbit; middle drag: pan; mouse wheel: zoom.
- Left click: select renderer bounds; F: frame the selected component.
- Dashboard: start/resume, pause, stop, reset, fan %, heater, occupant heat, thermal overlay.
- Charts retain the last 600 samples, approximately one sample per experiment second.
- Export writes the displayed samples to CSV under Unity's persistentDataPath.
- Opening the participant office stops the experiment; returning starts a fresh engineering scene.
- Hardware mode is read-only, shows unavailable until valid middleware readings arrive,
  and hides the simulated spatial overlay. It never substitutes simulated readings.

## Tests and batch build
Run from PowerShell:
```powershell
.\Tests\Run-CoreTests.ps1
& 'C:\Program Files\Unity\Hub\Editor\2023.2.20f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod ThermalTwinBuild.BuildWindows -logFile unity-build.log
```
The core suite requires the Windows .NET Framework C# compiler, not a downloaded NuGet test runner.
The batch command requires an installed, licensed Unity Editor and Windows build support.

Start with the [Windows Unity validation checklist](Docs/WINDOWS_VALIDATION_CHECKLIST.md), including exact menu clicks and expected diagnostics.

See [audit](Docs/AUDIT.md), [architecture and assumptions](Docs/ARCHITECTURE.md),
[middleware contract](Docs/MIDDLEWARE.md) and [validation status](Docs/VALIDATION.md).
