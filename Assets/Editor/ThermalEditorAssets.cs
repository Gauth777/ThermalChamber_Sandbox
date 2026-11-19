using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class ThermalEditorAssets
{
    public static Material Material(string assetPath,Color color)
    {
        var existing=AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if(existing) return existing;
        if(File.Exists(assetPath)) throw new InvalidOperationException("[TT-ASSET] Existing non-material asset: "+assetPath);
        var shader=Shader.Find("Standard");
        if(!shader) throw new InvalidOperationException("[TT-ASSET] Standard shader unavailable. Use the built-in render pipeline.");
        Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
        AssetDatabase.Refresh();
        var material=new Material(shader); material.color=color;
        AssetDatabase.CreateAsset(material,assetPath);
        return material;
    }
    public static void RegisterScenes()
    {
        var list=new System.Collections.Generic.List<EditorBuildSettingsScene>();
        foreach(var p in new[]{ThermalTwinBuild.ScenePath,ParticipantRoomBuilder.ScenePath})
            if(File.Exists(p)) list.Add(new EditorBuildSettingsScene(p,true));
        foreach(var old in EditorBuildSettings.scenes)
            if(old.path!=ThermalTwinBuild.ScenePath && old.path!=ParticipantRoomBuilder.ScenePath) list.Add(old);
        EditorBuildSettings.scenes=list.ToArray();
    }
}
