using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class ThermalTwinValidation
{
    [MenuItem("Thermal Twin/Validate Open Scene")]
    public static void ValidateOpenScene()
    {
        int count=0;
        foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                count+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
        if(count>0) throw new InvalidOperationException(count+" missing scripts in scene.");
        if(!UnityEngine.Object.FindObjectOfType<ChamberController>()) throw new InvalidOperationException("Missing ChamberController.");
        if(UnityEngine.Object.FindObjectsOfType<ThermalZone>().Length==0) throw new InvalidOperationException("Missing thermal field.");
        if(!Camera.main) throw new InvalidOperationException("Missing main camera.");
        Debug.Log("Scene reference checks passed. This is not a Play Mode or visual validation.");
    }
}
