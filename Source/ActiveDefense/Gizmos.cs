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
            //Rect bgRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75);
            //Widgets.DrawMenuSection(bgRect);
            //GUI.BeginGroup(bgRect);
            //comp.RefreshNetworkStorage(comp.parent);
            //float netPct = comp.NetworkStorage;
            //Widgets.Label(new Rect(0, 0, bgRect.width, 20), $"Сеть: {netPct:P0} ({comp.GetTotalEnergy(comp.parent)})");
            //comp.selectedPct = Widgets.HorizontalSlider(
            //    new Rect(5, 23, bgRect.width - 10, 20), comp.selectedPct, 0f, netPct);
            //Widgets.Label(new Rect(0, 50, bgRect.width, 20),
            //    $"Использовать: {comp.selectedPct * 100f:F0}% ({comp.GetStoredEnergy(comp.parent)*comp.selectedPct})");
            //Rect StatusbarBgr = new Rect(10, 30, (bgRect.width - 15), 20);
            ////Rect StatusbarAct = new Rect(10, 30, (bgRect.width - 15)*comp.GetStoredEnergy(comp.parent)/comp.GetTotalEnergy(comp.parent), 20);
            ////Widgets.DrawRectFast(StatusbarBgr,Color.gray);
            ////Widgets.DrawRectFast(StatusbarAct, Color.yellow);
            //Widgets.FillableBar(StatusbarBgr, comp.NetworkStorage);
            //GUI.EndGroup();
            //return new GizmoResult(GizmoState.Clear);
            Rect bgRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.DrawMenuSection(bgRect);

            GUI.BeginGroup(bgRect);

            // Общий процент энергии
            float netPct = comp.GetNetworkStorage(comp.parent);
            float total = comp.GetTotalEnergy(comp.parent);
            Widgets.Label(new Rect(8, 4, bgRect.width - 16, 20),
                $"Stored: {netPct:P0} ({comp.GetStoredEnergy(comp.parent)}/{total})");


            // Слайдер
            Rect sliderRect = new Rect(8, 26, bgRect.width - 16, 20);
            comp.selectedPct = Widgets.HorizontalSlider(sliderRect, comp.selectedPct, 0, 1);
            // Прогресс-бар
            Rect barRect = new Rect(8, 32, bgRect.width - 16, 18);
            Widgets.FillableBar(barRect, netPct);


            // Текст под слайдером
            Widgets.Label(new Rect(8, 50, bgRect.width - 16, 24),
                $"Left: {comp.selectedPct:P0} ({Mathf.RoundToInt(total * comp.selectedPct)})");

            GUI.EndGroup();
            return new GizmoResult(GizmoState.Clear);
        }

    }
}
