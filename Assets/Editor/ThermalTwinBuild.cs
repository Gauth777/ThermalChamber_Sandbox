using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ThermalTwinBuild
{
    const string ScenePath = "Assets/Scenes/EngineeringTwin.unity";

    [MenuItem("Thermal Twin/Create Engineering Scene")]
    public static void CreateEngineeringScene()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (File.Exists(ScenePath)) throw new InvalidOperationException("EngineeringTwin already exists. Preserve it; use a new path to regenerate.");
        var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/thermal_chamber.fbx");
        if (!source) throw new InvalidOperationException("Authoritative chamber FBX is missing.");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(source);
        model.name="Engineering Chamber — original FBX scale";
        var renderers=model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) throw new InvalidOperationException("FBX has no renderers.");
        Bounds bounds=renderers[0].bounds;
        foreach(var r in renderers) bounds.Encapsulate(r.bounds);
        var root=new GameObject("Experiment");
        var controller=root.AddComponent<ChamberController>();
        var sim=root.GetComponent<ThermalSimulation>();
        if(!sim) sim=root.AddComponent<ThermalSimulation>();
        float dx=bounds.size.x/4, dz=bounds.size.z/4;
        sim.neighborDistance=Mathf.Max(dx,dz)*1.05f;
        var material=new Material(Shader.Find("Standard"));
        material.color=new Color(.76f,.75f,.70f);
        AssetDatabase.CreateAsset(material,"Assets/Materials/EngineeringOverlay.mat");
        for(int z=0;z<4;z++) for(int x=0;x<4;x++)
        {
            var tile=GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name="Zone_"+x+"_"+z;
            tile.transform.position=new Vector3(bounds.min.x+(x+.5f)*dx,bounds.min.y+.01f,bounds.min.z+(z+.5f)*dz);
            tile.transform.localScale=new Vector3(dx*.95f,.01f,dz*.95f);
            UnityEngine.Object.DestroyImmediate(tile.GetComponent<Collider>());
            tile.GetComponent<Renderer>().sharedMaterial=material;
            tile.AddComponent<ThermalZone>();
        }
        foreach(var t in model.GetComponentsInChildren<Transform>())
        {
            if(t.name.StartsWith("IR_Heater_"))
            {
                var heat=t.gameObject.AddComponent<HeaterHeatSource>();
                heat.chamberController=controller;
                heat.heatingRadius=Mathf.Max(bounds.size.x,bounds.size.z)*.6f;
                t.gameObject.AddComponent<HeaterVisual>().chamberController=controller;
            }
        }
        // Rotate each blade assembly around its housing centre, not individual blade centres.
        for(int n=1;n<=2;n++)
        {
            string prefix="Fan_"+n.ToString("00");
            Transform housing=null;
            foreach(var t in model.GetComponentsInChildren<Transform>()) if(t.name==prefix+"_Housing") housing=t;
            if(!housing) continue;
            var pivot=new GameObject(prefix+"_SimulationRotor");
            pivot.transform.SetParent(model.transform,false);
            pivot.transform.position=housing.position;
            pivot.transform.rotation=housing.rotation;
            foreach(var t in model.GetComponentsInChildren<Transform>())
                if(t.name.StartsWith(prefix+"_Blade_")) t.SetParent(pivot.transform,true);
            var fan=pivot.AddComponent<FanVisual>();
            fan.chamberController=controller;
            fan.localRotationAxis=Vector3.right; // Verify against imported housing axis during visual QA.
        }
        var occupant=GameObject.CreatePrimitive(PrimitiveType.Capsule);
        occupant.name="Simulated occupant (scale requires engineering review)";
        occupant.transform.position=new Vector3(bounds.center.x,bounds.min.y+1,bounds.center.z);
        occupant.AddComponent<OccupantHeat>().heatingRadius=Mathf.Max(dx,dz)*1.5f;
        var camera=new GameObject("Main Camera");
        camera.tag="MainCamera";
        var cam=camera.AddComponent<Camera>(); cam.farClipPlane=Mathf.Max(1000,bounds.size.magnitude*10);
        float distance=Mathf.Max(4,bounds.size.magnitude*1.4f);
        camera.transform.position=bounds.center+new Vector3(distance*.7f,distance*.5f,-distance);
        camera.transform.LookAt(bounds.center);
        camera.AddComponent<AudioListener>();
        var inspection=camera.AddComponent<InspectionCamera>(); inspection.distance=Vector3.Distance(camera.transform.position,bounds.center);
        var light=new GameObject("Key Light").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.1f; light.transform.rotation=Quaternion.Euler(45,-30,0);
        RenderSettings.ambientLight=new Color(.6f,.63f,.65f);
        EditorSceneManager.SaveScene(scene,ScenePath);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Thermal Twin/Build Windows x64")]
    public static void BuildWindows()
    {
        if (!File.Exists(ScenePath)) CreateEngineeringScene();
        if (!File.Exists(ScenePath)) throw new InvalidOperationException("Engineering scene creation was cancelled.");
        if (!File.Exists(ParticipantRoomBuilder.ScenePath)) ParticipantRoomBuilder.Create();
        Directory.CreateDirectory("Builds/Windows");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{ScenePath,ParticipantRoomBuilder.ScenePath}, locationPathName="Builds/Windows/ThermalChamber.exe",
            target=BuildTarget.StandaloneWindows64, options=BuildOptions.None
        });
        if(report.summary.result!=BuildResult.Succeeded)
            throw new InvalidOperationException("Windows build failed: "+report.summary.result);
    }
}
