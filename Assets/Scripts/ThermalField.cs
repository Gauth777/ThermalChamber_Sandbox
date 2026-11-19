using System;

namespace Havells.ThermalTwin
{
    // Simplified lumped-capacitance network. Not CFD or a calibrated comfort model.
    public sealed class ThermalField
    {
        readonly double[] next;
        readonly bool[,] adjacent;
        public readonly double[] Temperature;
        public readonly double[] PowerWatts;
        public readonly double CapacityJoulesPerKelvin;
        public double AmbientCelsius = 24;
        public double AmbientConductanceWattsPerKelvin = 4;
        public double MixingConductanceWattsPerKelvin = 12;
        public double FanMixingWattsPerKelvin = 60;
        public int Count { get { return Temperature.Length; } }

        public ThermalField(int count, double capacity, double initial, bool[,] links)
        {
            if (count < 1 || !Finite(capacity) || capacity <= 0 || !Finite(initial) ||
                links == null || links.GetLength(0) != count || links.GetLength(1) != count)
                throw new ArgumentException("Invalid thermal field configuration.");
            adjacent = (bool[,])links.Clone();
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                    if (adjacent[i,j] != adjacent[j,i] || (i == j && adjacent[i,j]))
                        throw new ArgumentException("Links must be symmetric, without self-links.");
            CapacityJoulesPerKelvin = capacity;
            Temperature = new double[count];
            PowerWatts = new double[count];
            next = new double[count];
            Reset(initial);
        }

        public static bool Finite(double x) { return !double.IsNaN(x) && !double.IsInfinity(x); }
        public static double AirSpeed(double percent, double maximum)
        {
            if (!Finite(percent) || !Finite(maximum) || maximum < 0)
                throw new ArgumentException("Invalid fan command.");
            return Math.Max(0, Math.Min(100, percent)) * maximum / 100;
        }

        public void DepositPower(double[] distances, double radius, double watts)
        {
            if (distances == null || distances.Length != Count || !Finite(radius) || radius <= 0 ||
                !Finite(watts) || watts < 0) throw new ArgumentException("Invalid heat source.");
            double sum = 0;
            for (int i=0;i<Count;i++)
            {
                if (!Finite(distances[i]) || distances[i] < 0) throw new ArgumentException("Invalid source distance.");
                sum += Math.Max(0,1-distances[i]/radius);
            }
            if (sum > 0) for (int i=0;i<Count;i++)
                PowerWatts[i] += watts * Math.Max(0,1-distances[i]/radius) / sum;
        }

        public void Reset(double initial)
        {
            if (!Finite(initial)) throw new ArgumentException("Invalid initial temperature.");
            for (int i = 0; i < Count; i++) { Temperature[i] = initial; PowerWatts[i] = 0; }
        }

        public double Average
        {
            get { double sum = 0; foreach (double value in Temperature) sum += value; return sum / Count; }
        }

        public void Step(double seconds, double fanPercent)
        {
            if (!Finite(seconds) || seconds < 0 || seconds > 60 || !Finite(fanPercent) ||
                !Finite(AmbientCelsius) || !Finite(AmbientConductanceWattsPerKelvin) ||
                !Finite(MixingConductanceWattsPerKelvin) || !Finite(FanMixingWattsPerKelvin) ||
                AmbientConductanceWattsPerKelvin < 0 || MixingConductanceWattsPerKelvin < 0 ||
                FanMixingWattsPerKelvin < 0)
                throw new ArgumentException("Invalid simulation input.");
            double conductance = MixingConductanceWattsPerKelvin +
                FanMixingWattsPerKelvin * AirSpeed(fanPercent, 1);
            double maxLoss = AmbientConductanceWattsPerKelvin;
            for (int i = 0; i < Count; i++)
            {
                if (!Finite(Temperature[i]) || !Finite(PowerWatts[i]) || PowerWatts[i] < 0)
                    throw new ArgumentException("Invalid zone state or power.");
                int degree = 0;
                for (int j = 0; j < Count; j++) if (adjacent[i,j]) degree++;
                maxLoss = Math.Max(maxLoss, AmbientConductanceWattsPerKelvin + degree * conductance);
            }
            // Convex explicit update: sum of outgoing weights <= 0.5.
            double required = seconds * maxLoss / CapacityJoulesPerKelvin * 2;
            if (!Finite(required) || required > 100000) throw new ArgumentException("Configuration too stiff.");
            int steps = Math.Max(1, (int)Math.Ceiling(required));
            double dt = seconds / steps;
            for (int n = 0; n < steps; n++)
            {
                for (int i = 0; i < Count; i++)
                {
                    double watts = PowerWatts[i] +
                        AmbientConductanceWattsPerKelvin * (AmbientCelsius - Temperature[i]);
                    for (int j = 0; j < Count; j++)
                        if (adjacent[i,j]) watts += conductance * (Temperature[j] - Temperature[i]);
                    next[i] = Temperature[i] + dt * watts / CapacityJoulesPerKelvin;
                    if (!Finite(next[i])) throw new ArithmeticException("Non-finite thermal state.");
                }
                Array.Copy(next, Temperature, Count);
            }
        }
    }

    public enum DataMode { Simulation, Hardware }
    public enum ExperimentState { Stopped, Running, Paused }

    [Serializable]
    public struct EnvironmentalReading
    {
        public double TemperatureCelsius, RelativeHumidityPercent, AirVelocityMetresPerSecond;
        public double FanPercent;
        public bool HeaterOn;
        public DateTime TimestampUtc;
        public string Source;
        public bool IsValid(DateTime now)
        {
            return ThermalField.Finite(TemperatureCelsius) && ThermalField.Finite(RelativeHumidityPercent) &&
                RelativeHumidityPercent >= 0 && RelativeHumidityPercent <= 100 &&
                ThermalField.Finite(AirVelocityMetresPerSecond) && AirVelocityMetresPerSecond >= 0 &&
                ThermalField.Finite(FanPercent) && FanPercent >= 0 && FanPercent <= 100 &&
                TimestampUtc.Kind == DateTimeKind.Utc && TimestampUtc <= now &&
                (now - TimestampUtc).TotalSeconds <= 5 && !string.IsNullOrWhiteSpace(Source);
        }
    }

    // Middleware implementations supply actual acknowledged state. No physical command transport is supplied.
    public interface IEnvironmentDataSource
    {
        bool TryRead(out EnvironmentalReading reading);
    }

    public sealed class ExperimentSession
    {
        public DataMode Mode { get; private set; }
        public ExperimentState State { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public void Start() { State = ExperimentState.Running; }
        public void Pause() { if (State == ExperimentState.Running) State = ExperimentState.Paused; }
        public void Stop() { State = ExperimentState.Stopped; }
        public void Reset() { State = ExperimentState.Stopped; ElapsedSeconds = 0; }
        public void SwitchMode(DataMode mode) { Reset(); Mode = mode; }
        public void Advance(double seconds)
        {
            if (!ThermalField.Finite(seconds) || seconds < 0) throw new ArgumentException("Invalid elapsed time.");
            if (State == ExperimentState.Running) ElapsedSeconds += seconds;
        }
    }
}
