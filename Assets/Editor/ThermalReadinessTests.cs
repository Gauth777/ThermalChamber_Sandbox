using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
// Executable in Unity via menu/batchmode; does not save scenes or modify source assets.
public static class ThermalReadinessTests
{
    [MenuItem("Thermal Twin/Run Editor Readiness Tests")]
    public static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("[TT-TEST] Exit Play Mode.");
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/thermal_chamber.fbx");
            Assert(source,"FBX imports as GameObject");
            var model=(GameObject)PrefabUtility.InstantiatePrefab(source,scene);
            var discovery=new ChamberModelDiscovery(model.transform);
            discovery.RequireComponents(); var bounds=discovery.Interior();
            Assert(bounds.size.x>.5f && bounds.size.z>.5f,"non-degenerate interior");
            PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            var heater=discovery.Require("IR_Heater_04");
            string original=heater.name;
            heater.name="unmatched_heater";
            bool rejected=false;
            try { new ChamberModelDiscovery(model.transform).RequireComponents(); }
            catch(InvalidOperationException) { rejected=true; }
            Assert(rejected,"missing heater rejected");
            heater.name=original;
            var duplicate=new GameObject(original+".001"); duplicate.transform.SetParent(model.transform);
            rejected=false;
            try { new ChamberModelDiscovery(model.transform); }
            catch(InvalidOperationException) { rejected=true; }
            Assert(rejected,"duplicate normalized object rejected");
            UnityEngine.Object.DestroyImmediate(duplicate);
            var errors=ThermalTwinValidation.Collect(scene);
            Assert(errors.Any(e=>e.Contains("ChamberController")),"missing controller reported");
            Assert(errors.Any(e=>e.Contains("EngineeringLayout")),"missing layout reported");
            Assert(errors.Any(e=>e.Contains("MainCamera")),"missing camera reported");
            Debug.Log("[TT-TEST] PASS: imported component mapping, footprint and negative reference checks. Play Mode/build still required.");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
    }
    static void Assert(bool condition,string label)
    { if(!condition) throw new InvalidOperationException("[TT-TEST] FAIL: "+label); }
}
