# Middleware integration contract
Implement IEnvironmentDataSource on a MonoBehaviour and assign it to ThermalSimulation.hardwareProvider.
TryRead must be nonblocking, called on Unity's main thread, and return false until a real packet exists.
Receive/network work may run elsewhere, but publish an immutable/copied reading with thread-safe handoff.
Do not invoke Unity APIs from a background receive thread. No network transport or actuator writer is included.

EnvironmentalReading fields:
- TemperatureCelsius: measured temperature, C.
- RelativeHumidityPercent: measured RH, 0–100%.
- AirVelocityMetresPerSecond: measured nonnegative speed.
- FanPercent: actual/acknowledged fan state, 0–100%.
- HeaterOn: actual/acknowledged heater state.
- TimestampUtc: UTC measurement timestamp; host/sensor clocks must be synchronized.
- Source: nonempty real device/source identifier.

Packets are rejected when nonfinite, out of the above ranges, future-dated, older than five seconds
or missing source metadata. Invalid/disconnected sources show unavailable; stale values and simulation
are not silently presented as hardware. An implementation that throws is caught and shown as a warning.
Sensor-specific timestamps/quality/units and individual fan/heater channels are a future contract extension;
the current contract assumes a synchronized aggregate snapshot.

Simulation implements the same interface, with an explicit Simulation source label.
Synthetic test fixtures must remain marked as simulation. Never wire a mock into hardware mode for a review.

Physical actuator integration is intentionally absent. A later command transport must be separately
designed and agreed with the hardware team, including limits, acknowledgements, timeout behavior,
independent overtemperature interlocks and physical emergency stop. Software visualization is not an interlock.
