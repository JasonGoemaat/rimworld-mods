using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace gs_inventory_space
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("GsInventorySpace:HarmonyPatches() running static constructor");

            // create Harmony instance
            Harmony harmony = new Harmony(id: "Goemaat.GsInventorySpace");

            // find the RimWorld method we want to alter: MassUtility.Capacity(...)
            var originalMethod = AccessTools.Method(typeof(MassUtility), "Capacity");

            // find the method we want to call to transpile the instructions in the original method
            var transpilerMethod = new HarmonyMethod(typeof(HarmonyPatches), nameof(TranspileMassUtilityCapacity));

            // apply the patch using our Harmony instance
            harmony.Patch(originalMethod, transpiler: transpilerMethod);
        }

        static IEnumerable<CodeInstruction> TranspileMassUtilityCapacity(IEnumerable<CodeInstruction> instructions)
        {
            Log.Message("GsInventorySpace:TranspileMassUtilityCapacity() - Transpiling MassUtility.Capacity");

            // need MethodInfo for method to call
            var getCapacityMethod = AccessTools.Method(typeof(GsInventorySpaceMod), "GetBaseCapacity");
            if (getCapacityMethod == null)
            {
                Log.Error("GsInventorySpace:TranspileMassUtilityCapacity() - cannot find GetBaseCapacity!");
                foreach (var instruction in instructions) yield return instruction;
            } else
            {
                bool found = false;
                foreach (var instruction in instructions)
                {
                    if (instruction.LoadsConstant(35f))
                    {
                        found = true;
                        yield return new CodeInstruction(OpCodes.Call, getCapacityMethod);
                    } else
                    {
                        yield return instruction;
                    }
                }
                if (!found) Log.Error("GsInventorySpace:TranspileMassUtilityCapacity() - cannot find instruction in MassUtility.Capacity to change!");
            }
        }
    }
}
