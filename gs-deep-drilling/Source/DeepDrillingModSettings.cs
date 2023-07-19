using Verse;

namespace gs_deep_drilling
{
    public class DeepDrillingModSettings : ModSettings
    {
      public float SpeedFactor = 1.0f;
      public float Range = 2f;

      public override void ExposeData()
      {
        Scribe_Values.Look(ref SpeedFactor, "SpeedFactor", 1.0f);
        Scribe_Values.Look(ref Range, "Range", 2f);
        base.ExposeData();
      }
    }
}
