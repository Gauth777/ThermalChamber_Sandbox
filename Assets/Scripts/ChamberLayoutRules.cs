using System;
using System.Text.RegularExpressions;
namespace Havells.ThermalTwin
{
    public static class ChamberLayoutRules
    {
        public static string Normalize(string name)
        {
            if (name == null) return "";
            int prefix = Math.Max(name.LastIndexOf(':'), name.LastIndexOf('|'));
            name = name.Substring(prefix + 1).Trim();
            name = Regex.Replace(name, @"\.\d{3}$", "");
            return name.ToLowerInvariant();
        }
        // Floor footprint intersected with opposing wall inner faces, in world metres.
        public static double[] Interior(double floorMinX, double floorMaxX, double floorMinZ,
            double floorMaxZ, double leftInner, double rightInner, double frontInner, double backInner)
        {
            double[] input={floorMinX,floorMaxX,floorMinZ,floorMaxZ,leftInner,rightInner,frontInner,backInner};
            foreach(double v in input) if(!ThermalField.Finite(v)) throw new ArgumentException("Non-finite footprint.");
            double x0=Math.Max(floorMinX,leftInner), x1=Math.Min(floorMaxX,rightInner);
            double z0=Math.Max(floorMinZ,frontInner), z1=Math.Min(floorMaxZ,backInner);
            if(x1-x0<.5 || z1-z0<.5 || x1-x0>20 || z1-z0>20)
                throw new ArgumentException("Interior footprint outside 0.5-20 metres; verify FBX units and wall axes.");
            return new[]{x0,x1,z0,z1};
        }
    }
}
