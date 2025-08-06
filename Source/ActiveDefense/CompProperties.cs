using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ActiveDefense
{
    public class CompProperties_EMPActivator : CompProperties
    {
        public float radius;
        public float SafeRadius;
        public float DefaultDamageEnergy;
        public CompProperties_EMPActivator()
        {
            this.compClass = typeof(CompEMPActivator);
        }
    }
    public class CompProperties_TempestArcEffect : CompProperties
    {
        public CompProperties_TempestArcEffect()
        {
            this.compClass = typeof(Projectile_TempestArc);
        }
    }
    public class CompProperties_EnergyConsumption : CompProperties
    {
        public float BaseEnergyMag;
        public float BaseEnergyConsumptionPerShot;
        public CompProperties_EnergyConsumption()
        {
            this.compClass = typeof(CompEnergyConsumption);
        }
    }
}
