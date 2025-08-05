using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ActiveDefense
{
    public class Building_EMPEmitter : Building
    {
        private CompEMPActivator Comp => this.GetComp<CompEMPActivator>();
        private float SafeRadius => Comp.Props.SafeRadius;
        private float DangerRadius => Comp.Props.radius;
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            // первое кольцо — безопасная зона (снаружи которой всё бьётся)
            GenDraw.DrawRadiusRing(this.Position, DangerRadius);

            // второе кольцо — внутренняя безопасная область
            GenDraw.DrawRadiusRing(this.Position, SafeRadius);
        }
    }
}
