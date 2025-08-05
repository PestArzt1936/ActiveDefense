using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ActiveDefense
{
    public class PlaceWorker_EMPZone : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var Props = def.GetCompProperties<CompProperties_EMPActivator>();
            if (Props == null) return;
            var dangerous = Props.radius;
            var safe = Props.SafeRadius;

            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, dangerous, true).ToList(), Color.red);
            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, safe, true).ToList(), Color.green);
        }
    }
}
