using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ActiveDefense
{
    public class Gizmo_EmpCore : Gizmo
    {
        public CompEMPActivator comp;

        public override float GetWidth(float maxWidth) => 200f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect bgRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawMenuSection(bgRect);

            GUI.BeginGroup(bgRect);

            float netPct = comp.GetNetworkStorage(comp.parent);
            float total = comp.GetTotalEnergy(comp.parent);
            Widgets.Label(new Rect(8, 4, bgRect.width - 16, 20),
                $"Stored: {netPct:P0} ({comp.GetStoredEnergy(comp.parent)}/{total})");

            Rect sliderRect = new Rect(8, 26, bgRect.width - 16, 20);
            comp.selectedPct = Widgets.HorizontalSlider(sliderRect, comp.selectedPct, 0, 1);

            Rect barRect = new Rect(8, 32, bgRect.width - 16, 18);
            Widgets.FillableBar(barRect, netPct);

            Widgets.Label(new Rect(8, 50, bgRect.width - 16, 24),
                $"Left: {comp.selectedPct:P0} ({Mathf.RoundToInt(total * comp.selectedPct)})");

            GUI.EndGroup();
            return new GizmoResult(GizmoState.Clear);
        }

    }
    public class Gizmo_Tempest : Command
    {
        public CompEnergyConsumption comp;
        public override float GetWidth(float maxWidth) => 150f;
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect bgRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), Height);
            Rect inRect = bgRect.ContractedBy(6);
            Widgets.DrawMenuSection(bgRect);
            Text.Font = GameFont.Tiny;
            Rect textRect = inRect.TopHalf();
            Widgets.Label(textRect, $"Battery power of Tempest");

            Rect barRect = inRect.BottomHalf();
            Widgets.FillableBar(barRect, comp.CurrentCharge / comp.Props.BaseEnergyMag);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, String.Format("{0:0.00}",comp.CurrentCharge)  + " / " +String.Format("{0:0.00}",comp.Props.BaseEnergyMag));
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
