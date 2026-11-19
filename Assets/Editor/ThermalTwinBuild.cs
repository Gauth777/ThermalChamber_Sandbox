using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ThermalTwinBuild
{
    public const string ScenePath="Assets/Scenes/EngineeringTwin.unity";
    [MenuItem("Thermal Twin/Create Engineering Scene")]
    public static void CreateEngineeringScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("[TT-SCENE] Exit Play Mode first.");
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if(File.Exists(ScenePath)) throw new InvalidOperationException("[TT-SCENE] EngineeringTwin exists. Rename the generated scene in Project before regenerating; it will not be overwritten.");
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Models/thermal_chamber.fbx");
        if(!source) throw new InvalidOperationException("[TT-ASSET] Missing authoritative FBX.");
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(source);
            // Only this generated scene copy is unpacked. Imported model bytes/settings stay untouched.
            PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            model.name="Engineering Chamber - original FBX scale";
            var discovery=new ChamberModelDiscovery(model.transform);
            discovery.RequireComponents();
            Bounds interior=discovery.Interior();
            var root=new GameObject("Experiment");
            var sim=root.AddComponent<ThermalSimulation>();
            var controller=root.AddComponent<ChamberController>();
            root.AddComponent<ResearchDashboard>();
            var layout=root.AddComponent<EngineeringLayout>();
            layout.model=model.transform; layout.interior=interior;
            layout.zones=new ThermalZone[16]; layout.heaters=new HeaterHeatSource[4]; layout.fans=new FanVisual[2];
            float dx=interior.size.x/4, dz=interior.size.z/4;
            sim.neighborDistance=Mathf.Max(dx,dz)*1.05f;
            var material=ThermalEditorAssets.Material("Assets/Materials/EngineeringOverlay.mat",new Color(.76f,.75f,.70f));
            var grid=new GameObject("Thermal Field");
            for(int z=0;z<4;z++) for(int x=0;x<4;x++)
            {
                var tile=GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name="Zone_"+x+"_"+z; tile.transform.SetParent(grid.transform);
                tile.transform.position=new Vector3(interior.min.x+(x+.5f)*dx,interior.min.y+.015f,interior.min.z+(z+.5f)*dz);
                tile.transform.localScale=new Vector3(dx*.95f,.01f,dz*.95f);
                UnityEngine.Object.DestroyImmediate(tile.GetComponent<Collider>());
                tile.GetComponent<Renderer>().sharedMaterial=material;
                layout.zones[z*4+x]=tile.AddComponent<ThermalZone>();
            }
            for(int n=1;n<=4;n++)
            {
                var t=discovery.Require("IR_Heater_"+n.ToString("00"));
                var heat=t.gameObject.AddComponent<HeaterHeatSource>();
                heat.chamberController=controller; heat.heatingRadius=Mathf.Max(interior.size.x,interior.size.z)*.6f;
                t.gameObject.AddComponent<HeaterVisual>().chamberController=controller;
                layout.heaters[n-1]=heat;
            }
            for(int n=1;n<=2;n++)
            {
                string prefix="Fan_"+n.ToString("00");
                var housing=discovery.Require(prefix+"_Housing");
                var blades=new Transform[3]; Vector3 center=Vector3.zero;
                for(int b=0;b<3;b++) { blades[b]=discovery.Require(prefix+"_Blade_"+(b+1).ToString("00")); center+=blades[b].position/3; }
                foreach(var b in blades) if(Vector3.Distance(b.position,center)>.005f)
                    throw new InvalidOperationException("[TT-FAN] Blade origins do not share a shaft: "+prefix);
                // Source FBX: housing and all blade origins differ along the shaft; use that observed direction.
                Vector3 axis=center-housing.position;
                if(axis.sqrMagnitude<.000001f) throw new InvalidOperationException("[TT-FAN] Ambiguous shaft direction: "+prefix);
                var pivot=new GameObject(prefix+"_SimulationRotor");
                pivot.transform.SetParent(model.transform,false); pivot.transform.position=center;
                foreach(var blade in blades) blade.SetParent(pivot.transform,true);
                var fan=pivot.AddComponent<FanVisual>(); fan.chamberController=controller;
                fan.localRotationAxis=pivot.transform.InverseTransformDirection(axis.normalized);
                layout.fans[n-1]=fan;
                Debug.Log("[TT-FAN] "+prefix+": 3 blades, shaft="+axis.normalized.ToString("F3")+" pivot="+center.ToString("F3"));
            }
            var occupant=GameObject.CreatePrimitive(PrimitiveType.Capsule);
            occupant.name="Simulated occupant";
            float occupantHeight=Mathf.Min(1.7f,interior.size.y-.1f);
            occupant.transform.localScale=new Vector3(.45f,occupantHeight/2,.45f);
            occupant.transform.position=new Vector3(interior.center.x,interior.min.y+occupantHeight/2,interior.center.z);
            layout.occupant=occupant.AddComponent<OccupantHeat>(); layout.occupant.heatingRadius=Mathf.Max(dx,dz)*1.5f;
            sim.configuredLayout=layout;
            var camera=new GameObject("Main Camera"); camera.tag="MainCamera";
            var cam=camera.AddComponent<Camera>(); cam.nearClipPlane=.01f; cam.farClipPlane=200;
            float distance=Mathf.Max(4,interior.size.magnitude*1.4f);
            camera.transform.position=interior.center+new Vector3(distance*.7f,distance*.5f,-distance);
            camera.transform.LookAt(interior.center); camera.AddComponent<AudioListener>();
            camera.AddComponent<InspectionCamera>().distance=Vector3.Distance(camera.transform.position,interior.center);
            var light=new GameObject("Key Light").AddComponent<Light>();
            light.type=LightType.Directional; light.intensity=1.1f; light.transform.rotation=Quaternion.Euler(45,-30,0);
            RenderSettings.ambientLight=new Color(.6f,.63f,.65f);
            ThermalTwinValidation.ValidateOpenScene();
            Directory.CreateDirectory("Assets/Scenes");
            if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new IOException("[TT-SCENE] Could not save EngineeringTwin.");
            AssetDatabase.SaveAssets(); ThermalEditorAssets.RegisterScenes();
            Debug.Log("[TT-SCENE] EngineeringTwin saved. Enter Play Mode for runtime validation.");
        }
        catch { EditorSceneManager.RestoreSceneManagerSetup(setup); throw; }
    }

    [MenuItem("Thermal Twin/Build Windows x64")]
    public static void BuildWindows()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("[TT-BUILD] Exit Play Mode.");
        if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.StandaloneWindows64)
            throw new InvalidOperationException("[TT-BUILD] Select Windows / Intel 64-bit in File > Build Settings and Switch Platform first.");
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64))
            throw new InvalidOperationException("[TT-BUILD] Windows Build Support is missing from this Editor installation.");
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if(!File.Exists(ScenePath)) CreateEngineeringScene();
        if(!File.Exists(ScenePath)) throw new InvalidOperationException("[TT-BUILD] Engineering scene creation cancelled.");
        if(!File.Exists(ParticipantRoomBuilder.ScenePath)) ParticipantRoomBuilder.Create();
        if(!File.Exists(ParticipantRoomBuilder.ScenePath)) throw new InvalidOperationException("[TT-BUILD] Participant scene creation cancelled.");
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach(var p in new[]{ScenePath,ParticipantRoomBuilder.ScenePath})
            { EditorSceneManager.OpenScene(p,OpenSceneMode.Single); ThermalTwinValidation.ValidateOpenScene(); }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        ThermalEditorAssets.RegisterScenes();
        Directory.CreateDirectory("Builds/Windows");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{ScenePath,ParticipantRoomBuilder.ScenePath}, locationPathName="Builds/Windows/ThermalChamber.exe",
            target=BuildTarget.StandaloneWindows64, options=BuildOptions.None
        });
        if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("[TT-BUILD] Failed: "+report.summary.result);
        Debug.Log("[TT-BUILD] Windows x64 build succeeded: Builds/Windows/ThermalChamber.exe");
    }
}
