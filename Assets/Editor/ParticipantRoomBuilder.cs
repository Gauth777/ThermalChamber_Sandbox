using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class ParticipantRoomBuilder
{
    public const string ScenePath="Assets/Scenes/ParticipantOffice.unity";
    [MenuItem("Thermal Twin/Create Participant Office")]
    public static void Create()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new System.InvalidOperationException("[TT-SCENE] Exit Play Mode.");
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if(File.Exists(ScenePath)) { ThermalEditorAssets.RegisterScenes(); return; }
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        // Independent 5 x 4 x 2.7 metre placeholder, not engineering chamber geometry.
        var material=ThermalEditorAssets.Material("Assets/Materials/ParticipantOffice.mat",new Color(.75f,.73f,.68f));
        Box("Floor",new Vector3(0,-.05f,0),new Vector3(5,.1f,4),material);
        Box("Back wall",new Vector3(0,1.35f,2),new Vector3(5,2.7f,.1f),material);
        Box("Side wall",new Vector3(-2.5f,1.35f,0),new Vector3(.1f,2.7f,4),material);
        Box("Desk",new Vector3(0,.75f,.8f),new Vector3(1.4f,.07f,.7f),material);
        for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2)
            Box("Desk leg",new Vector3(x*.6f,.36f,.8f+z*.25f),new Vector3(.05f,.72f,.05f),material);
        Box("Chair seat",new Vector3(0,.45f,-.1f),new Vector3(.5f,.08f,.5f),material);
        Box("Chair back",new Vector3(0,.75f,-.35f),new Vector3(.5f,.6f,.06f),material);
        Box("Chair pedestal",new Vector3(0,.2f,-.1f),new Vector3(.1f,.4f,.1f),material);
        Box("Monitor",new Vector3(0,1.05f,1),new Vector3(.55f,.35f,.05f),material);
        Box("Monitor stand",new Vector3(0,.85f,1),new Vector3(.05f,.2f,.05f),material);
        var camera=new GameObject("Main Camera"); camera.tag="MainCamera";
        camera.AddComponent<Camera>(); camera.AddComponent<AudioListener>();
        camera.transform.position=new Vector3(1.8f,1.6f,-1.7f);
        camera.transform.LookAt(new Vector3(0,1,.7f));
        var inspection=camera.AddComponent<InspectionCamera>(); inspection.distance=3; inspection.reservedLeftPixels=0;
        camera.AddComponent<ParticipantRoomNavigation>();
        var light=new GameObject("Daylight").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=.9f; light.transform.rotation=Quaternion.Euler(50,-20,0);
        RenderSettings.ambientLight=new Color(.65f,.65f,.65f);
        ThermalTwinValidation.ValidateOpenScene();
        if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new System.IO.IOException("[TT-SCENE] Could not save ParticipantOffice.");
        AssetDatabase.SaveAssets(); ThermalEditorAssets.RegisterScenes();
        Debug.Log("[TT-SCENE] ParticipantOffice saved and registered.");
        } catch { EditorSceneManager.RestoreSceneManagerSetup(setup); throw; }
    }
    static void Box(string name,Vector3 position,Vector3 scale,Material material)
    {
        var box=GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name=name; box.transform.position=position; box.transform.localScale=scale;
        box.GetComponent<Renderer>().sharedMaterial=material;
    }
}
