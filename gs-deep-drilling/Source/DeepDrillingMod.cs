using RimWorld;
using UnityEngine;
using Verse;

namespace gs_deep_drilling
{
    /// <summary>
    /// See: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    public class DeepDrillingMod : Mod
    {
        public static DeepDrillingMod Instance;

        public DeepDrillingModSettings Settings;

        public DeepDrillingMod(ModContentPack content) : base(content)
        {
            Log.Message("GsInventorySpaceMod.GsInventorySpaceMod()");
            Instance = this;
            this.Settings = GetSettings<DeepDrillingModSettings>();

            // now that we have settings, calclate values to use and update the def
            // NOTE: This is called before the defs are ready, so I'm calling it from HarmonyPatches
            // this.UpdateValues();

            // for testing:
            // CountCells(2.6f); // should be 21
            // CountCells(56.4f); // should be 9997
        }

        /// <summary>
        /// Maybe from HugsLib?
        /// </summary>
        //public override void DefsLoaded()
        //{

        //}

        public void UpdateValues()
        {
            // TODO: Move this somewhere after settings are changed and update the def as well
            float currentRange = LoadedModManager.GetMod<DeepDrillingMod>().GetSettings<DeepDrillingModSettings>().Range;
            if (currentRange < 1f) currentRange = 1f;
            if (currentRange > 56.4f) currentRange = 56.4f;
            if (currentRange != LastRange)
            {
                var def = DefDatabase<ThingDef>.GetNamed("DeepDrill");
                Log.Message($"gs-deep-drilling: changing DeepDrill.specialDisplayRadius from {def.specialDisplayRadius} to {currentRange}");
                def.specialDisplayRadius = currentRange;

                int counter = CountCells(currentRange);
                if (counter < 21) counter = 21;
                if (counter > 10000) counter = 10000;
                if (LoopCounter != counter)
                {
                    Log.Message($"gs-deep-drilling: changing LoopCounter from {LoopCounter} to {counter}");
                    LoopCounter = counter;
                }
                LastRange = currentRange;
            }

            var defSpeed = DefDatabase<StatDef>.GetNamed("DeepDrillingSpeed");
            var skillBonus = defSpeed.skillNeedFactors.Find(x => x.skill.defName == "Mining") as SkillNeed_BaseBonus;
            // TODO: in harmony Postfix for `float ValueFor(Pawn pawn)`, check if instance of the class is this instance
            // and multiply the return value by the SpeedFactor if so.
            Log.Message($"UpdateValues() Instance is type {skillBonus.GetType()}");
            DeepDrillingSpeedBasebonusInstance = skillBonus;

            // ALT: StatDef.Named("DeepDrillingSpeed")
            // Per def in xml, statFactors are 'WorkSpeedglobal'
            // skillDeefFactors contains 'SkillNeed_BaseBonus' where skill = 'Mining", baseValue is 0.04 and bonusPerLevel is 0.12
            // this is possibly in 'abilityStatFactors'?
            // might be set in PopulateMtuableStats()?
            // Chain of stuff, possibly used?
            StatDefOf.DeepDrillingSpeed.statFactors.Find(x => x.defName == "DeepDrillingSpeed");

            // ok, REALLY need to change CompDeepDrill.DrillWorkDone(Pawn driller) using transpile with this:
            // 			float statValue = driller.GetStatValue(StatDefOf.DeepDrillingSpeed, true, -1);
            // OR, change that (Pawn.GetStatValue) using postfix and checking for StatDefOf.DeepDrillingSpeed




            //// NOTE: This can't be done via def since the SkillNeed_BaseBonus properties 'baseValue' and 'bonusPerLevel' are private!
            //var defSpeed = DefDatabase<StatDef>.GetNamed("DeepDrillingSpeed");
            //if (InitialSpeedBaseValue == 0f || InitialBonusPerLevel == 0f)
            //{
            //    //InitialSpeedBaseValue = defSpeed.defaultBaseValue;
            //    var skillBonus = defSpeed.skillNeedFactors.Find(x => x.skill.defName == "Mining") as SkillNeed_BaseBonus;
            //}
            //float currentSpeedFactor = LoadedModManager.GetMod<DeepDrillingMod>().GetSettings<DeepDrillingModSettings>().SpeedFactor;
        }

        public float InitialSpeedBaseValue = 0f;
        public float InitialBonusPerLevel = 0f;

        public SkillNeed_BaseBonus DeepDrillingSpeedBasebonusInstance = null;

        /// <summary>
        /// This is called many times when changing mod settings, possibly after any change is made?
        /// </summary>
        /// <param name="inRect"></param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Speed Factor (0.1 - 10.0)");
            Settings.SpeedFactor = listingStandard.Slider(Settings.SpeedFactor, 0.1f, 10f);
            string buffer = $"{Settings.SpeedFactor}";
            listingStandard.TextFieldNumericLabeled<float>("SpeedFactor", ref Settings.SpeedFactor, ref buffer, min: 0.1f, max: 10f);

            listingStandard.Label("Range (2.6 - 56.4)");
            Settings.Range = listingStandard.Slider(Settings.Range, 2.6f, 56.4f);
            buffer = $"{Settings.Range}";
            listingStandard.TextFieldNumericLabeled<float>("Range", ref Settings.Range, ref buffer, min: 2.6f, max: 56.4f);

            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "gs-deep-drilling";
        }

        public override void WriteSettings()
        {
            Log.Message("gs-deep-drilling: WriteSettings()");
            base.WriteSettings();
            UpdateValues();
        }

        public float LastRange = 0.0f;
        public int LoopCounter = 0;

        public int CountCells(float range)
        {
            // need to re-calculate how many loops to use, this quick fiddle I wrote seems to work well, using 56.4 which
            // RimWorld claims is the max to get 10,000 cells in their Radial find method giving me 9997 visible blocks:
            // https://jsfiddle.net/ecbrfpag/6/
            float rangeSquared = range * range;
            int minInt = (int)(-range - 1);
            int maxInt = (int)(range + 1);
            int count = 0;
            for (int x = minInt; x <= maxInt; x++)
            {
                int xSquared = x * x;
                for (int y = minInt; y <= maxInt; y++)
                {
                    int ySquared = y * y;
                    float distanceSquared = (float)(xSquared + ySquared);
                    if (distanceSquared <= rangeSquared)
                    {
                        count++;
                    }
                }
            }
            Log.Message($"gs-deep-drilling: Range {range} calculated {count} cells");
            return count;
        }

        /// <summary>
        /// This method will be called to get the loop counter for how many squares to search
        /// for on the map in a radial pattern.   10000 equates to 54.6 range.
        /// </summary>
        /// <returns></returns>
        public static int GetLoopCounter()
        {
            return Instance.LoopCounter;
        }

        public static float GetSpeedFactor()
        {
            return Instance.Settings.SpeedFactor;
        }
    }
}
