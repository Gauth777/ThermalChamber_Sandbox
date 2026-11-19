using UnityEngine;
using Havells.ThermalTwin;

[DisallowMultipleComponent]
public class ChamberController : MonoBehaviour
{
    [Range(0,100)] public float fanSpeed;
    public bool heaterOn;
    public bool occupantEnabled = true;
    public bool enableDesktopDashboard = true;
    public ThermalSimulation Simulation { get; private set; }
    public bool CommandsEnabled { get { return Simulation && Simulation.Session.Mode == DataMode.Simulation; } }
    void Awake()
    {
        Simulation = GetComponent<ThermalSimulation>();
        if (!Simulation) Simulation = gameObject.AddComponent<ThermalSimulation>();
        if (enableDesktopDashboard && !GetComponent<ResearchDashboard>())
            gameObject.AddComponent<ResearchDashboard>();
    }
    public void SetFanSpeed(float percent) { if (CommandsEnabled) fanSpeed = Mathf.Clamp(percent,0,100); }
    public void SetHeater(bool value) { if (CommandsEnabled) heaterOn = value; }
}
