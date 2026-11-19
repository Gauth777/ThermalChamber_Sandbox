using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class ThermalTwinValidation
{
    public static List<string> Collect(Scene scene)
    {
        var errors=new List<string>();
        var roots=scene.GetRootGameObjects();
        var transforms=roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
        foreach(var t in transforms)
            if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) errors.Add("Missing script on "+t.name);
        var cameras=transforms.Select(t=>t.GetComponent<Camera>()).Where(c=>c && c.enabled && c.gameObject.activeInHierarchy && c.CompareTag("MainCamera")).ToArray();
        if(cameras.Length!=1) errors.Add("Expected exactly one active MainCamera; found "+cameras.Length);
        if(transforms.Count(t=>t.GetComponent<AudioListener>() && t.GetComponent<AudioListener>().enabled && t.gameObject.activeInHierarchy)!=1)
            errors.Add("Expected exactly one active AudioListener.");
        foreach(var r in transforms.SelectMany(t=>t.GetComponents<Renderer>()))
            if(r.sharedMaterials.Length==0 || r.sharedMaterials.Any(m=>!m || !m.shader)) errors.Add("Missing material/shader: "+r.name);
        if(transforms.Any(t=>t.GetComponent<ParticipantRoomNavigation>()))
        {
            if(!transforms.Any(t=>t.name=="Floor")) errors.Add("Participant room missing Floor.");
            return errors;
        }
        var controllers=transforms.Select(t=>t.GetComponent<ChamberController>()).Where(c=>c && c.isActiveAndEnabled).ToArray();
        if(controllers.Length!=1) errors.Add("Expected exactly one active ChamberController.");
        var layout=transforms.Select(t=>t.GetComponent<EngineeringLayout>()).FirstOrDefault(c=>c);
        if(!layout) { errors.Add("Missing EngineeringLayout. Regenerate EngineeringTwin with the current builder."); return errors; }
        var controller=controllers.FirstOrDefault();
        var sim=layout.GetComponent<ThermalSimulation>();
        if(!sim || !sim.enabled || sim.configuredLayout!=layout) errors.Add("ThermalSimulation layout reference missing/incorrect.");
        if(!layout.GetComponent<ResearchDashboard>() || !layout.GetComponent<ResearchDashboard>().enabled)
            errors.Add("ResearchDashboard missing/disabled.");
        if(!layout.model || !layout.model.gameObject.activeInHierarchy) errors.Add("Engineering model missing/inactive.");
        if(layout.columns<1 || layout.rows<1 || layout.zones==null || layout.zones.Length!=layout.columns*layout.rows)
            errors.Add("Grid dimensions and zone count disagree.");
        if(layout.zones==null || layout.zones.Length!=16) errors.Add("Expected 16 zones in generated 4x4 field.");
        var zones=layout.zones ?? new ThermalZone[0];
        if(zones.Distinct().Count()!=zones.Length) errors.Add("Duplicate zone references.");
        foreach(var z in zones)
        {
            if(!z || !z.isActiveAndEnabled) { errors.Add("Missing/disabled thermal zone."); continue; }
            var r=z.GetComponent<Renderer>();
            if(!r) { errors.Add("Zone has no renderer: "+z.name); continue; }
            var b=r.bounds; var f=layout.interior;
            if(b.min.x<f.min.x-.001f || b.max.x>f.max.x+.001f || b.min.z<f.min.z-.001f ||
                b.max.z>f.max.z+.001f || b.min.y<f.min.y || b.max.y>f.min.y+.1f)
                errors.Add("Zone outside interior floor footprint: "+z.name);
        }
        if(layout.heaters==null || layout.heaters.Length!=4) errors.Add("Expected four heater references.");
        foreach(var h in layout.heaters ?? new HeaterHeatSource[0])
        {
            if(!h || !h.isActiveAndEnabled) { errors.Add("Missing/disabled heater."); continue; }
            if(!h.GetComponent<Renderer>()) errors.Add("HeaterVisual requires a renderer on "+h.name);
            if(h.chamberController!=controller) errors.Add("Wrong controller on heater: "+h.name);
            if(!h.GetComponent<HeaterVisual>() || h.GetComponent<HeaterVisual>().chamberController!=controller)
                errors.Add("HeaterVisual reference missing: "+h.name);
            if(!(h.powerWatts>0) || !(h.heatingRadius>0)) errors.Add("Invalid heater power/radius: "+h.name);
            if(!zones.Any(z=>z && new Vector2(z.transform.position.x-h.transform.position.x,z.transform.position.z-h.transform.position.z).magnitude<h.heatingRadius))
                errors.Add("Heater radius reaches no zones: "+h.name);
        }
        if(layout.fans==null || layout.fans.Length!=2) errors.Add("Expected two fan assemblies.");
        foreach(var fan in layout.fans ?? new FanVisual[0])
        {
            if(!fan || !fan.isActiveAndEnabled) { errors.Add("Missing/disabled fan assembly."); continue; }
            if(fan.chamberController!=controller || fan.localRotationAxis.sqrMagnitude<.5f) errors.Add("Invalid fan controller/axis: "+fan.name);
            if(fan.transform.childCount!=3) errors.Add("Expected three rotor blades: "+fan.name);
        }
        if(!layout.occupant || !layout.occupant.isActiveAndEnabled) errors.Add("Occupant source missing/disabled.");
        if(sim && (!(sim.effectiveCapacityPerZoneJK>0) || !(sim.neighborDistance>0))) errors.Add("Invalid simulation capacity/spacing.");
        return errors;
    }
    [MenuItem("Thermal Twin/Validate Open Scene")]
    public static void ValidateOpenScene()
    {
        var errors=Collect(SceneManager.GetActiveScene());
        foreach(var error in errors) Debug.LogError("[TT-VALIDATE] "+error);
        if(errors.Count>0) throw new InvalidOperationException("[TT-VALIDATE] Failed with "+errors.Count+" errors. See preceding messages.");
        Debug.Log("[TT-VALIDATE] PASS: required scene references and component counts. Runtime/visual checks still required.");
    }
}
