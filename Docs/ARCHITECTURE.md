# Architecture and engineering assumptions
## Data flow
ChamberController (simulated commands) → ThermalSimulation (fixed-step adapter) →
ThermalField (pure C# network) → EnvironmentalReading → dashboard / legacy TMP adapter / recorder.
ThermalZone holds cell state and applies a MaterialPropertyBlock overlay. HeaterHeatSource and
OccupantHeat are source definitions; they no longer independently mutate zones each rendered frame.
FanVisual and HeaterVisual consume the current source's acknowledged/simulated actuator state.

## Simplified model
C_i dT_i/dt = Q_i + G_ambient(T_ambient-T_i) + sum_j G_ij(T_j-T_i).
C is effective heat capacity in J/K; Q is watts; G is W/K; T is Celsius (temperature differences
are Kelvin-equivalent). Equal per-zone capacities and symmetric adjacency conserve energy during
closed-field diffusion. Double buffering removes update-order dependence. Adaptive substeps limit
outgoing integration weights to 0.5; Unity advances on FixedUpdate. Computational stiffness is bounded.

Defaults are demonstration assumptions, not Havells equipment ratings: 1200 J/K per cell,
4 W/K ambient exchange per cell, 12 W/K base neighbor exchange, up to 60 W/K extra fan exchange,
500 W per simulated heater, 80 W sensible heat per occupant. Calibrate all of these before research use.
A 4x4 field therefore has effective capacity 19200 J/K; increasing resolution requires adjusting
per-cell capacity and conductances to retain the same physical model.

Heat power is normalized across cells within a horizontal linear-falloff radius; a source outside
that radius contributes zero. Fan speed maps linearly to a configurable maximum (default 1.2 m/s)
and increases mixing, without artificial refrigeration. Envelope exchange returns air toward ambient.
Air velocity is an estimate, not local flow simulation. Occupant heat is sensible-only: no moisture,
radiation/comfort feedback or physiology. RH is an uncalibrated constant-moisture Magnus approximation.
No HVAC, CFD, PMV, calibrated radiation or real sensor inference is claimed.
Temperature is not physically clamped to the heatmap range; colors saturate outside 20–32 C.

## Compatibility and scene setup
Existing MonoBehaviour names, serialized actuator fields and GUIDs remain. Old heatOutput/rate fields
are retained for serialization but no longer drive the new power model. Existing AddHeat explicitly
accepts a Celsius increment for compatibility. Add/move/remove sources or grid topology in Edit Mode,
then reload the scene. Generated scenes use explicit EngineeringLayout references and rectangular grid adjacency. Legacy MainScene retains discovery fallback. Runtime source discovery is intentionally not continuous.
The generated scene retains FBX scale and uses chamber_base plus named XPS wall inner faces for the field. Whole-model bounds are not used for the grid.
Verify chamber interior bounds, floor height, meters, rotor pivot/axis and source positions in Unity.
The generator uses a 4x4 grid; edit cells/neighborDistance and effective capacities for other resolutions.
All simulation sources and zones currently belong to one chamber per loaded scene.

## Experiments and presentation
Start/resume advances state; pause freezes simulated time/field; stop disables simulated fan/heater
while preserving temperatures. Reset restores initial temperatures and clears history.
Mode switches stop actuators and clear history; the simulation field is retained until reset.
No physical command is ever sent. Hardware mode hides the simulation overlay.
CSV represents the last 600 displayed samples, not a lossless full-duration research logger.
Renderer-bound selection can select occluding walls; this is a desktop inspection foundation,
not mesh-accurate CAD picking. The IMGUI dashboard is functional but still needs visual polish/QA.

The separate office is a 5 x 4 x 2.7 m primitive placeholder, independent of chamber engineering
dimensions. No XR loader, headset interaction, consent workflow or participant study protocol is provided.
