using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Havells.ThermalTwin;

public class ResearchDashboard : MonoBehaviour
{
    ChamberController controller;
    ThermalSimulation sim;
    readonly List<EnvironmentalReading> history = new List<EnvironmentalReading>();
    readonly List<double> times = new List<double>();
    double nextSample;
    string exportStatus = "";
    Vector2 scroll;
    void Start()
    {
        controller = GetComponent<ChamberController>();
        sim = controller ? controller.Simulation : null;
        if (sim) sim.ResetOccurred += ClearHistory;
        if (Camera.main && !Camera.main.GetComponent<InspectionCamera>())
            Camera.main.gameObject.AddComponent<InspectionCamera>();
    }
    void OnDestroy() { if (sim) sim.ResetOccurred -= ClearHistory; }
    void ClearHistory() { history.Clear(); times.Clear(); nextSample = 0; exportStatus = ""; }
    void Update()
    {
        if (!sim || !sim.HasReading || sim.Session.State != ExperimentState.Running ||
            sim.Session.ElapsedSeconds < nextSample) return;
        nextSample = sim.Session.ElapsedSeconds + 1;
        history.Add(sim.Reading); times.Add(sim.Session.ElapsedSeconds);
        if (history.Count > 600) { history.RemoveAt(0); times.RemoveAt(0); }
    }
    void OnGUI()
    {
        if (!sim || !controller) return;
        GUI.backgroundColor = new Color(.14f,.19f,.23f);
        GUILayout.BeginArea(new Rect(12,12,340,Mathf.Max(180,Screen.height-24)), GUI.skin.box);
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.Label("THERMAL CHAMBER  |  RESEARCH");
        GUILayout.Label(sim.Session.Mode == DataMode.Simulation ? "SIMULATION  /  simplified model, not CFD" : "HARDWARE  /  read-only middleware");
        GUILayout.Label(sim.Session.State + "   •   " + sim.Session.ElapsedSeconds.ToString("F1") + " s");
        GUILayout.Space(8);
        if (sim.HasReading)
        {
            var r = sim.Reading;
            GUILayout.Label("Temperature     " + r.TemperatureCelsius.ToString("F2") + " °C");
            GUILayout.Label("Relative humidity     " + r.RelativeHumidityPercent.ToString("F1") + " %");
            GUILayout.Label("Air velocity     " + r.AirVelocityMetresPerSecond.ToString("F2") + " m/s");
            GUILayout.Label("Fan     " + r.FanPercent.ToString("F0") + " %    |    Heater " + (r.HeaterOn ? "ON" : "OFF"));
            GUILayout.Label(r.Source, GUI.skin.box);
        }
        else GUILayout.Label("Environmental readings unavailable");
        if (!string.IsNullOrEmpty(sim.Warning)) GUILayout.Label("WARNING: " + sim.Warning, GUI.skin.box);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Simulation")) sim.SwitchMode(DataMode.Simulation);
        if (GUILayout.Button("Hardware")) sim.SwitchMode(DataMode.Hardware);
        GUILayout.EndHorizontal();
        GUI.enabled = controller.CommandsEnabled;
        GUILayout.Label("Simulated fan command");
        controller.SetFanSpeed(GUILayout.HorizontalSlider(controller.fanSpeed,0,100));
        controller.SetHeater(GUILayout.Toggle(controller.heaterOn,"Heater enabled"));
        controller.occupantEnabled = GUILayout.Toggle(controller.occupantEnabled,"Occupant heat enabled");
        sim.HeatmapVisible = GUILayout.Toggle(sim.HeatmapVisible,"Spatial temperature overlay");
        GUI.enabled = true;
        GUILayout.Label("Overlay: 20 °C blue → 24 °C neutral → 32 °C orange");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Start")) sim.StartExperiment();
        if (GUILayout.Button("Pause")) sim.PauseExperiment();
        if (GUILayout.Button("Stop")) sim.StopExperiment();
        if (GUILayout.Button("Reset")) sim.ResetExperiment();
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        GUILayout.Label("History: last 600 samples, ~1 sample/s");
        DrawChart("Temperature °C",0);
        DrawChart("Relative humidity %",1);
        DrawChart("Air velocity m/s",2);
        if (GUILayout.Button("Export displayed history to CSV")) Export();
        if (exportStatus.Length > 0) GUILayout.Label(exportStatus);
        if (Application.CanStreamedLevelBeLoaded("ParticipantOffice") && GUILayout.Button("Stop & open participant office"))
        { sim.StopExperiment(); UnityEngine.SceneManagement.SceneManager.LoadScene("ParticipantOffice"); }
        GUILayout.Label("Camera: right drag orbit • middle drag pan\nWheel zoom • left click select • F frame");
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    static double Value(EnvironmentalReading r, int series)
    { return series == 0 ? r.TemperatureCelsius : series == 1 ? r.RelativeHumidityPercent : r.AirVelocityMetresPerSecond; }
    void DrawChart(string title, int series)
    {
        Rect rect = GUILayoutUtility.GetRect(300,55);
        GUI.Box(rect, "");
        if (history.Count < 2) { GUI.Label(rect, title + " — waiting for samples"); return; }
        double min = Value(history[0],series), max = min;
        foreach (var r in history) { min = Math.Min(min,Value(r,series)); max = Math.Max(max,Value(r,series)); }
        double range = Math.Max(.1,max-min);
        GUI.Label(new Rect(rect.x+4,rect.y,rect.width,20),title+"  "+min.ToString("F1")+"–"+max.ToString("F1"));
        Color prior = GUI.color;
        GUI.color = new Color(.45f,.72f,.76f);
        for (int i=1;i<history.Count;i++)
        {
            Vector2 a = new Vector2(rect.x+(i-1)*rect.width/(history.Count-1),rect.yMax-4-(float)((Value(history[i-1],series)-min)/range)*28);
            Vector2 b = new Vector2(rect.x+i*rect.width/(history.Count-1),rect.yMax-4-(float)((Value(history[i],series)-min)/range)*28);
            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
            GUI.DrawTexture(new Rect(a.x,a.y,(b-a).magnitude,1),Texture2D.whiteTexture);
            GUI.matrix = matrix;
        }
        GUI.color = prior;
    }
    static string Cell(string text) { return "\"" + text.Replace("\"","\"\"") + "\""; }
    void Export()
    {
        try
        {
            string file = Path.Combine(Application.persistentDataPath,"thermal-experiment-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")+".csv");
            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine("elapsed_s,timestamp_utc,mode,source,temperature_C,humidity_pct,air_m_s,fan_pct,heater_on");
                for (int i=0;i<history.Count;i++)
                {
                    var r=history[i];
                    writer.WriteLine(string.Join(",",times[i].ToString("R",CultureInfo.InvariantCulture),
                        r.TimestampUtc.ToString("o"),sim.Session.Mode.ToString(),Cell(r.Source),
                        r.TemperatureCelsius.ToString("R",CultureInfo.InvariantCulture),
                        r.RelativeHumidityPercent.ToString("R",CultureInfo.InvariantCulture),
                        r.AirVelocityMetresPerSecond.ToString("R",CultureInfo.InvariantCulture),
                        r.FanPercent.ToString("R",CultureInfo.InvariantCulture),r.HeaterOn.ToString()));
                }
            }
            exportStatus="Saved: "+file;
        }
        catch (Exception ex) { exportStatus="Export failed: "+ex.Message; }
    }
}
