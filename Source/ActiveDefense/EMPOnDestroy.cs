using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ActiveDefense
{
    public class CompEMPOnDestroy : ThingComp
    {
        public CompProperties_EMPOnDestroy Props => (CompProperties_EMPOnDestroy)props;
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (previousMap == null)
            {
                return;
            }

            // Только при уничтожении или демонтаже
            if (mode != DestroyMode.KillFinalize)
            {
                return;
            }

            // Запуск EMP-взрыва как при ручной активации

            var compEmp = parent.GetComp<CompEMPActivator>();
            if (compEmp != null)
            {
                compEmp.UniEmpBlast(parent.Position,previousMap,compEmp.GetEnergyFact(parent),parent,compEmp.Props.SafeRadius,compEmp.Props.radius);
            }
            else
                return;
        }
    }
    public class CompProperties_EMPOnDestroy : CompProperties
    {
        public CompProperties_EMPOnDestroy()
        {
            compClass = typeof(CompEMPOnDestroy);
        }
    }
}
