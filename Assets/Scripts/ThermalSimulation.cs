using System;
using UnityEngine;
using Havells.ThermalTwin;

[DisallowMultipleComponent]
public class ThermalSimulation : MonoBehaviour, IEnvironmentDataSource
{
    [Header("Uncalibrated lumped model (not CFD)")]
    public float initialCelsius = 24, ambientCelsius = 24;
    [Min(1)] public float effectiveCapacityPerZoneJK = 1200;
    [Min(0)] public float ambientConductanceWK = 4, mixingConductanceWK = 12, fanMixingWK = 60;
    [Min(.01f)] public float neighborDistance = 2.2f;
    [Range(0,100)] public float referenceHumidityPercent = 55;
    [Min(0)] public float maximumAirVelocity = 1.2f;
    [Tooltip("Optional middleware component implementing IEnvironmentDataSource. Main-thread reads only.")]
    public MonoBehaviour hardwareProvider;
    public readonly ExperimentSession Session = new ExperimentSession();
    public EnvironmentalReading Reading { get; private set; }
    public bool HasReading { get; private set; }
    public string Warning { get; private set; }
    public ThermalZone[] Zones { get; private set; }
    public bool HeatmapVisible = true;
    ThermalField field;
    ChamberController controller;
    HeaterHeatSource[] heaters;
    OccupantHeat[] occupants;
    double[] weights;
    public event Action ResetOccurred;

    void Start()
    {
        controller = GetComponent<ChamberController>();
        Zones = FindObjectsOfType<ThermalZone>();
        heaters = FindObjectsOfType<HeaterHeatSource>();
        occupants = FindObjectsOfType<OccupantHeat>();
        Initialize();
    }

    public void Initialize()
    {
        if (Zones == null || Zones.Length == 0)
        { Warning = "No thermal zones found. Simulation unavailable."; HasReading = false; return; }
        try
        {
            bool[,] links = new bool[Zones.Length, Zones.Length];
            for (int i = 0; i < Zones.Length; i++)
                for (int j = i+1; j < Zones.Length; j++)
                {
                    Vector3 delta = Zones[i].transform.position - Zones[j].transform.position;
                    bool aligned = Mathf.Abs(delta.x) < .1f || Mathf.Abs(delta.z) < .1f;
                    bool nearest = true;
                    for (int k = 0; k < Zones.Length; k++)
                    {
                        if (k == i || k == j) continue;
                        Vector3 a = Zones[k].transform.position - Zones[i].transform.position;
                        float along = Vector3.Dot(a, delta);
                        if (Vector3.Cross(a, delta).sqrMagnitude < .0001f &&
                            along < 0 && along > -delta.sqrMagnitude) nearest = false;
                    }
                    links[i,j] = links[j,i] = aligned && nearest && delta.sqrMagnitude > .0001f &&
                        delta.sqrMagnitude <= neighborDistance * neighborDistance;
                }
            field = new ThermalField(Zones.Length, effectiveCapacityPerZoneJK, initialCelsius, links);
            weights = new double[Zones.Length];
            ResetExperiment();
        }
        catch (Exception ex) { Warning = ex.Message; HasReading = false; field = null; }
    }

    void FixedUpdate()
    {
        if (Session.Mode != DataMode.Simulation || Session.State != ExperimentState.Running || field == null) return;
        try
        {
            for (int i = 0; i < Zones.Length; i++)
            {
                if (!Zones[i]) throw new InvalidOperationException("A thermal zone was removed. Reload the scene.");
                field.Temperature[i] = Zones[i].temperature;
                field.PowerWatts[i] = 0;
            }
            if (controller && controller.heaterOn)
                foreach (var heater in heaters)
                    if (heater && heater.isActiveAndEnabled)
                        DistributePower(heater.transform.position, heater.heatingRadius, heater.powerWatts);
            if (controller && controller.occupantEnabled)
                foreach (var occupant in occupants)
                    if (occupant && occupant.isActiveAndEnabled)
                        DistributePower(occupant.transform.position, occupant.heatingRadius, occupant.sensibleHeatWatts);
            field.AmbientCelsius = ambientCelsius;
            field.AmbientConductanceWattsPerKelvin = ambientConductanceWK;
            field.MixingConductanceWattsPerKelvin = mixingConductanceWK;
            field.FanMixingWattsPerKelvin = fanMixingWK;
            field.Step(Time.fixedDeltaTime, controller ? controller.fanSpeed : 0);
            for (int i = 0; i < Zones.Length; i++) Zones[i].temperature = (float)field.Temperature[i];
            Session.Advance(Time.fixedDeltaTime);
        }
        catch (Exception ex) { Warning = ex.Message; Session.Pause(); HasReading = false; }
    }

    void DistributePower(Vector3 position, float radius, float watts)
    {
        if (!ThermalField.Finite(watts) || watts < 0 || !ThermalField.Finite(radius) || radius <= 0)
            throw new ArgumentException("Invalid heat-source power or radius.");
        
        for (int i = 0; i < Zones.Length; i++)
        {
            Vector3 p = Zones[i].transform.position;
            double distance = Math.Sqrt((p.x-position.x)*(p.x-position.x)+(p.z-position.z)*(p.z-position.z));
            weights[i] = distance;
        }
        // Outside the field: no deposited heat. Never amplify power as cell count grows.
        field.DepositPower(weights, radius, watts);
    }

    void Update()
    {
        if (Zones != null) foreach (var zone in Zones)
            if (zone) zone.HeatmapVisible = HeatmapVisible && Session.Mode == DataMode.Simulation;
        if (Session.Mode == DataMode.Hardware)
        {
            try
            {
                var provider = hardwareProvider as IEnvironmentDataSource;
                EnvironmentalReading value = default(EnvironmentalReading);
                HasReading = hardwareProvider && provider != null && provider != this &&
                    provider.TryRead(out value) && value.IsValid(DateTime.UtcNow);
                // Read exactly once: providers may consume a packet on each call.
                if (HasReading) { Reading = value; Warning = ""; }
                else { Reading = default(EnvironmentalReading); Warning = "Hardware disconnected, invalid or stale (>5 s). No simulated fallback."; }
            }
            catch (Exception ex) { HasReading = false; Reading = default(EnvironmentalReading); Warning = "Middleware error: " + ex.Message; }
            if (Session.State == ExperimentState.Running) Session.Advance(Time.deltaTime);
        }
        else
        {
            try { PublishSimulation(); }
            catch (Exception ex) { HasReading = false; Warning = ex.Message; Session.Pause(); }
        }
    }

    void PublishSimulation()
    {
        if (field == null || !string.IsNullOrEmpty(Warning)) { HasReading = false; return; }
        double t = field.Average;
        // Constant absolute moisture approximation using saturation vapour pressure (Magnus).
        double reference = Math.Exp(17.625 * initialCelsius / (243.04 + initialCelsius));
        double saturation = Math.Exp(17.625 * t / (243.04 + t));
        Reading = new EnvironmentalReading {
            TemperatureCelsius = t,
            RelativeHumidityPercent = Math.Max(0, Math.Min(100, referenceHumidityPercent * reference / saturation)),
            AirVelocityMetresPerSecond = ThermalField.AirSpeed(controller ? controller.fanSpeed : 0, maximumAirVelocity),
            FanPercent = controller ? Mathf.Clamp(controller.fanSpeed,0,100) : 0,
            HeaterOn = controller && controller.heaterOn,
            TimestampUtc = DateTime.UtcNow, Source = "Simulation: grid average / estimated RH / commanded fan airflow"
        };
        HasReading = true;
    }

    public bool TryRead(out EnvironmentalReading reading) { reading = Reading; return HasReading; }
    public void StartExperiment() { if ((Session.Mode == DataMode.Hardware ? HasReading : field != null) && string.IsNullOrEmpty(Warning)) Session.Start(); }
    public void PauseExperiment() { Session.Pause(); }
    public void StopExperiment() { Session.Stop(); if (controller) { controller.heaterOn = false; controller.fanSpeed = 0; } }
    public void ResetExperiment()
    {
        StopExperiment(); Session.Reset(); Warning = "";
        if (field != null)
        {
            field.Reset(initialCelsius);
            foreach (var zone in Zones) if (zone) zone.temperature = initialCelsius;
        }
        HasReading = false; Reading = default(EnvironmentalReading);
        if (ResetOccurred != null) ResetOccurred();
    }
    public void SwitchMode(DataMode mode)
    {
        StopExperiment(); Session.SwitchMode(mode);
        HasReading = false; Reading = default(EnvironmentalReading); Warning = "";
        if (ResetOccurred != null) ResetOccurred();
    }
}
