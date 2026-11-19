using System;
using Havells.ThermalTwin;
class CoreTests
{
    static int passed;
    static void Check(string name, Action test)
    { try { test(); passed++; Console.WriteLine("PASS "+name); } catch(Exception e) { Console.WriteLine("FAIL "+name+": "+e.Message); Environment.Exit(1); } }
    static void Equal(double a,double b,double eps=1e-8) { if(Math.Abs(a-b)>eps) throw new Exception(a+" != "+b); }
    static void True(bool value) { if(!value) throw new Exception("Assertion failed"); }
    static ThermalField Pair(double c=100) { return new ThermalField(2,c,24,new bool[,]{{false,true},{true,false}}); }
    static void Reject(Action action) { try { action(); } catch(ArgumentException) { return; } throw new Exception("Expected rejection"); }
    static void Main()
    {
        Check("heater on adds Q dt / C",delegate { var f=Pair(); f.AmbientConductanceWattsPerKelvin=0; f.MixingConductanceWattsPerKelvin=0; f.PowerWatts[0]=100; f.Step(1,0); Equal(f.Temperature[0],25); Equal(f.Temperature[1],24); });
        Check("heater off adds no heat",delegate { var f=Pair(); f.Step(10,0); Equal(f.Average,24); });
        Check("occupant sensible power adds local heat",delegate { var f=Pair(); f.AmbientConductanceWattsPerKelvin=0; f.PowerWatts[0]=80; f.Step(1,0); True(f.Temperature[0]>f.Temperature[1]); Equal(f.Average,24.4); });
        Check("diffusion conserves energy in sealed field",delegate { var f=Pair(); f.AmbientConductanceWattsPerKelvin=0; f.Temperature[0]=30; f.Step(10,0); Equal(f.Average,27); True(f.Temperature[0]<30&&f.Temperature[1]>24); });
        Check("permuting zone order preserves result",delegate { var a=Pair(); var b=Pair(); a.Temperature[0]=30; b.Temperature[1]=30; a.PowerWatts[0]=80; b.PowerWatts[1]=80; a.Step(1,70); b.Step(1,70); Equal(a.Temperature[0],b.Temperature[1]); Equal(a.Temperature[1],b.Temperature[0]); });
        Check("fan increases mixing",delegate { var a=Pair(); var b=Pair(); a.Temperature[0]=b.Temperature[0]=30; a.Step(.1,0); b.Step(.1,100); True(b.Temperature[0]-b.Temperature[1]<a.Temperature[0]-a.Temperature[1]); });
        Check("fan cannot cool uniform ambient field",delegate { var f=Pair(); f.Step(60,100); Equal(f.Temperature[0],24); });
        Check("fan speed maps to airflow and clamps",delegate { Equal(ThermalField.AirSpeed(50,1.2),.6); Equal(ThermalField.AirSpeed(-10,1.2),0); Equal(ThermalField.AirSpeed(150,1.2),1.2); });
        Check("stiff diffusion remains bounded",delegate { var f=Pair(1); f.Temperature[0]=32; f.Step(60,100); foreach(var t in f.Temperature) True(t>=24&&t<=32); });
        Check("fixed-step schedule independent of render grouping",delegate { var a=Pair(); var b=Pair(); a.PowerWatts[0]=b.PowerWatts[0]=80; for(int frame=0;frame<100;frame++) for(int tick=0;tick<5;tick++) a.Step(.02,30); for(int tick=0;tick<500;tick++) b.Step(.02,30); Equal(a.Average,b.Average); });
        Check("pause freezes experiment clock",delegate { var s=new ExperimentSession(); s.Start(); s.Advance(2); s.Pause(); s.Advance(3); Equal(s.ElapsedSeconds,2); });
        Check("stop prevents clock advance",delegate { var s=new ExperimentSession(); s.Start(); s.Stop(); s.Advance(2); Equal(s.ElapsedSeconds,0); });
        Check("reset restores field and session",delegate { var f=Pair(); f.PowerWatts[0]=100; f.Step(1,0); f.Reset(22); Equal(f.Average,22); Equal(f.PowerWatts[0],0); var s=new ExperimentSession(); s.Start(); s.Advance(4); s.Reset(); Equal(s.ElapsedSeconds,0); True(s.State==ExperimentState.Stopped); });
        Check("source switch stops experiment",delegate { var s=new ExperimentSession(); s.Start(); s.Advance(4); s.SwitchMode(DataMode.Hardware); True(s.Mode==DataMode.Hardware&&s.State==ExperimentState.Stopped); Equal(s.ElapsedSeconds,0); });
        Check("rejects nonfinite and invalid inputs",delegate { Reject(delegate {Pair().Step(double.NaN,0);}); Reject(delegate {Pair().Step(-1,0);}); Reject(delegate {ThermalField.AirSpeed(double.NaN,1);}); Reject(delegate {new ThermalField(0,1,24,new bool[0,0]);}); Reject(delegate {new ThermalField(2,1,24,new bool[,]{{false,true},{false,false}});}); });
        Check("hardware freshness validity",delegate { var now=DateTime.UtcNow; var r=new EnvironmentalReading {TemperatureCelsius=24,RelativeHumidityPercent=55,AirVelocityMetresPerSecond=.5,FanPercent=40,TimestampUtc=now,Source="SHT31-01"}; True(r.IsValid(now)); r.TimestampUtc=now.AddSeconds(-6); True(!r.IsValid(now)); r.TimestampUtc=now.AddSeconds(1); True(!r.IsValid(now)); r.TimestampUtc=now; r.TemperatureCelsius=double.NaN; True(!r.IsValid(now)); });
        Check("default packet is unavailable",delegate { True(!default(EnvironmentalReading).IsValid(DateTime.UtcNow)); });
        Check("localized source conserves specified total watts",delegate {var f=Pair(); f.DepositPower(new double[]{0,1},2,500); Equal(f.PowerWatts[0]+f.PowerWatts[1],500); True(f.PowerWatts[0]>f.PowerWatts[1]);});
        Check("source outside field contributes no power",delegate {var f=Pair(); f.DepositPower(new double[]{3,4},2,500); Equal(f.PowerWatts[0]+f.PowerWatts[1],0);});
        Check("invalid source rejected",delegate {Reject(delegate {Pair().DepositPower(new double[]{0,1},0,80);}); Reject(delegate {Pair().DepositPower(null,1,80);});});
        Console.WriteLine(passed+" tests passed.");
    }
}
