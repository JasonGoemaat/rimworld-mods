using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace gs_inventory_space
{
    public class GsInventorySpaceModSettings : ModSettings
    {
        public float BaseCapacity = 35.0f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref BaseCapacity, "BaseCapacity", 35f);
            base.ExposeData();
        }
    }
}
