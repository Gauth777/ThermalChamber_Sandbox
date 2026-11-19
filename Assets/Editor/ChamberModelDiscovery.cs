using System;
using System.Collections.Generic;
using UnityEngine;
using Havells.ThermalTwin;
public sealed class ChamberModelDiscovery
{
    readonly Dictionary<string,Transform> objects=new Dictionary<string,Transform>();
    public ChamberModelDiscovery(Transform root)
    {
        foreach(var t in root.GetComponentsInChildren<Transform>(true))
        {
            string key=ChamberLayoutRules.Normalize(t.name);
            if(objects.ContainsKey(key)) throw new InvalidOperationException("[TT-MATCH] Duplicate normalized name: "+t.name);
            objects.Add(key,t);
        }
        foreach(var pair in objects)
            if((pair.Key.Contains("heater") || pair.Key.StartsWith("fan_")) &&
                !System.Text.RegularExpressions.Regex.IsMatch(pair.Key,@"^(ir_heater_0[1-4]|fan_0[12]_(housing|blade_0[1-3]))$"))
                Debug.LogWarning("[TT-MATCH] Unmatched actuator-like object: "+pair.Value.name);
    }
    public Transform Require(string name)
    {
        Transform result;
        if(!objects.TryGetValue(ChamberLayoutRules.Normalize(name),out result))
            throw new InvalidOperationException("[TT-MATCH] Missing FBX object: "+name);
        return result;
    }
    public Bounds BoundsOf(string name)
    {
        var t=Require(name);
        var renderers=t.GetComponentsInChildren<Renderer>(true);
        if(renderers.Length==0) throw new InvalidOperationException("[TT-MATCH] No renderer: "+name);
        Bounds b=renderers[0].bounds;
        foreach(var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }
    public Bounds Interior()
    {
        Bounds floor=BoundsOf("chamber_base");
        Bounds left=BoundsOf("XPS_Left_Wall"), right=BoundsOf("XPS_Right_Wall");
        Bounds back=BoundsOf("XPS_Back_Wall"), front=BoundsOf("XPS_Front_Left");
        front.Encapsulate(BoundsOf("XPS_Front_Right"));
        // Names may face opposite world directions after FBX conversion. Sort opposing planes.
        Bounds lowX=left.center.x<right.center.x?left:right, highX=left.center.x<right.center.x?right:left;
        Bounds lowZ=front.center.z<back.center.z?front:back, highZ=front.center.z<back.center.z?back:front;
        if(lowX.size.x>floor.size.x*.25f || highX.size.x>floor.size.x*.25f ||
           lowZ.size.z>floor.size.z*.25f || highZ.size.z>floor.size.z*.25f ||
           floor.size.y>Mathf.Min(floor.size.x,floor.size.z)*.25f)
            throw new InvalidOperationException("[TT-FOOTPRINT] Named walls/floor are not aligned with Unity X/Z floor. Check importer axes.");
        double[] r;
        try { r=ChamberLayoutRules.Interior(floor.min.x,floor.max.x,floor.min.z,floor.max.z,
            lowX.max.x,highX.min.x,lowZ.max.z,highZ.min.z); }
        catch(Exception ex) { throw new InvalidOperationException("[TT-FOOTPRINT] "+ex.Message); }
        float top=BoundsOf("XPS_Ceiling").min.y;
        if(top-floor.max.y<1 || top-floor.max.y>10)
            throw new InvalidOperationException("[TT-FOOTPRINT] Interior height outside 1-10 metres; verify imported orientation/scale.");
        Bounds result=new Bounds();
        result.SetMinMax(new Vector3((float)r[0],floor.max.y,(float)r[2]),new Vector3((float)r[1],top,(float)r[3]));
        Debug.Log("[TT-FOOTPRINT] Interior metres: min="+result.min.ToString("F3")+" max="+result.max.ToString("F3")+
            "; derived from chamber_base and XPS inner faces. Verify against engineering drawings.");
        return result;
    }
    public void RequireComponents()
    {
        for(int i=1;i<=4;i++) { BoundsOf("IR_Heater_"+i.ToString("00")); BoundsOf("SHT31_Sensor_"+i.ToString("00")); }
        BoundsOf("MRT_Globe_Sensor"); BoundsOf("Anemometer_Head"); BoundsOf("Chamber_Door");
        for(int i=1;i<=2;i++)
        {
            BoundsOf("Fan_"+i.ToString("00")+"_Housing");
            for(int j=1;j<=3;j++) BoundsOf("Fan_"+i.ToString("00")+"_Blade_"+j.ToString("00"));
        }
        Debug.Log("[TT-MATCH] Matched 4 heaters, 2 fan housings, 6 blades, 4 SHT31 sensors, globe, anemometer and door.");
    }
}
