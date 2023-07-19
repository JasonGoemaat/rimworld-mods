namespace gs_deep_drilling
{
    using global::RimWorld;
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;
    using Verse;
    using System.Reflection;

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "gs-deep-drilling");

            // this method is overloaded, looking at this post to find the format to identify which one with the type array:
            //    https://github.com/pardeike/Harmony/issues/88
            Log.Message("gs-deep-drilling: HarmonyPatches running!");
            Log.Message("gs-deep-drilling: Patching DeepDrillUtility.GetNextResource()");

            var parameters = new Type[]
            {
                typeof(IntVec3),
                typeof(Map),
                typeof(ThingDef).MakeByRefType(),
                typeof(System.Int32).MakeByRefType(),
                typeof(IntVec3).MakeByRefType()
            };
            var originalMethod = AccessTools.Method(typeof(DeepDrillUtility), "GetNextResource", parameters);
            var transpilerMethod = new HarmonyMethod(patchType, nameof(GetNextResourceTranspile));
            harmony.Patch(originalMethod, transpiler: transpilerMethod); ; ;
            Log.Message("gs-deep-drilling: GetNextResource Patched using transpile!!!");

            // init to change the Def visiible range and calculate the loop counter for GetNextResource()
            DeepDrillingMod.Instance.UpdateValues();

            harmony.Patch(AccessTools.Method(typeof(SkillNeed_BaseBonus), "ValueFor"), postfix: new HarmonyMethod(patchType, nameof(DrillingSpeedBonusPostfix)));
        }

        /// <summary>
        /// Alter GetNextResource() method that scans a radial pattern for 21 blocks to instead scan for more blocks.
        /// Max number is 10000 which is set by the class it calls, there is an array of 10,000 cell locations that
        /// is setup in advance for rimworld just for this sort of thing.   EUREKA!   That is why there was an error
        /// with a gun with too long a range, 10,000 squares fit in a circle with a radius of 56.42, that is why 56.4
        /// is the max!
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        static IEnumerable<CodeInstruction> GetNextResourceTranspile(IEnumerable<CodeInstruction> instructions)
        {
            var method = AccessTools.Method(typeof(DeepDrillingMod), "GetLoopCounter");
            var list = new List<string>();

            // readme on seeing IL code in dnSpy, right-click on method and view IL:
            //     https://github.com/qcjxberin/dnSpy-1/blob/master/README.md
            // Harmony transpiling patching page:
            //     https://harmony.pardeike.net/articles/patching-transpiler.html
            bool found = false;

            foreach (var instruction in instructions)
            {
                // for debugging:
                // Log.Message($"{instruction}");
                
                if (instruction.LoadsConstant(21))
                {
                    // 2.6 radius (default) gives 21 squares
                    // 5.2 radius gives 89 squares
                    // 8.65 radius gives 241 squares
                    // 56.42 radius gives 10,000 squares
                    // change Ldc_I4_S (which is 8 bit) to Ldc_I4 (which is 32 bit) and change value from 21 for 2.6 range to 241 for 8.65 range
                    // yield return new CodeInstruction(OpCodes.Ldc_I4, 10000);
                    yield return new CodeInstruction(OpCodes.Call, method);
                    found = true;
                }
                else
                {
                    list.Add($"{instruction}");
                    yield return instruction;
                }
            }
            if (!found)
            {
                Log.Error("gs-deep-drilling: Cannot find load instruction for loop counter!");
                Log.Message("Instructions: ");
                foreach(var s in list)
                {
                    Log.Message($"  {s}");
                }
            }
        }

        public static void DrillingSpeedBonusPostfix(Pawn pawn, ref float __result, ref SkillNeed_BaseBonus __instance)
        {
            // class: SkillNeed_BaseBonus
            // public override float ValueFor(Pawn pawn)
            if (__instance != DeepDrillingMod.Instance.DeepDrillingSpeedBasebonusInstance)
            {
                return;
            }
            __result = __result * DeepDrillingMod.Instance.Settings.SpeedFactor;
        }

        /// <summary>
        /// Do not allow deep drills to mine 'base' resources like stones.   To do this, we return 'null'
        /// when trying to get the base resoruce.
        /// </summary>
        /// <param name="__result"></param>
        /// <returns></returns>
        //static bool GetBaseResourcePrefix(ref ThingDef __result)
        //{
        //    __result = null;
        //    return false;
        //}

        //static IEnumerable<CodeInstruction> CapacityTranspile(IEnumerable<CodeInstruction> instructions)
        //{
        //    bool found = false;

        //    foreach (var instruction in instructions)
        //    {
        //        if (instruction.LoadsConstant(35f))
        //        {
        //            // 2.6 radius (default) gives 21 squares
        //            // 5.2 radius gives 89 squares
        //            // 8.65 radius gives 241 squares
        //            // 56.42 radius gives 10,000 squares
        //            // change Ldc_I4_S (which is 8 bit) to Ldc_I4 (which is 32 bit) and change value from 21 for 2.6 range to 241 for 8.65 range
        //            yield return new CodeInstruction(OpCodes.Ldc_R4, 2000f); // 
        //            found = true;
        //        }
        //        else
        //        {
        //            yield return instruction;
        //        }
        //    }
        //    if (!found)
        //    {
        //        Log.Error("Cannot find instruction to load loop counter for deep drilling!");
        //    }
        //}
    }
}


/*
 * 
 * As recommended by: https://rimworldwiki.com/wiki/Modding_Tutorials/Harmony
 * Following this: https://github.com/erdelf/AlienRaces/blob/master/Source/AlienRace/AlienRace/HarmonyPatches.cs
 * patching this and just not calling it:: 
 *      protected void TrySpread()
 */