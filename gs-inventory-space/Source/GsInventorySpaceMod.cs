using UnityEngine;
using Verse;

namespace gs_inventory_space
{
    /// <summary>
    /// See: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    public class GsInventorySpaceMod : Mod
    {
        public GsInventorySpaceModSettings Settings;

        public GsInventorySpaceMod(ModContentPack content) : base(content)
        {
            Log.Message("GsInventorySpaceMod.GsInventorySpaceMod()");
            this.Settings = GetSettings<GsInventorySpaceModSettings>();
        }

        /// <summary>
        /// This is called many times when changing mod settings, possibly after any change is made?
        /// </summary>
        /// <param name="inRect"></param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Base Capacity (35kg - 1000kg)");
            Settings.BaseCapacity = listingStandard.Slider(Settings.BaseCapacity, 35f, 1000f);
            string buffer = $"{Settings.BaseCapacity}";
            listingStandard.TextFieldNumericLabeled<float>("Base Capacity", ref Settings.BaseCapacity, ref buffer, min: 35f, max: 1000f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "gs-inventory-space";
        }

        public override void WriteSettings()
        {
            Log.Message("GsInventorySpaceMod.WriteSettings()");
            base.WriteSettings();
        }

        public static float GetBaseCapacity()
        {
            return LoadedModManager.GetMod<GsInventorySpaceMod>().GetSettings<GsInventorySpaceModSettings>().BaseCapacity;
        }
    }
}