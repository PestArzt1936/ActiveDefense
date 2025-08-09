using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CombatExtended;
using UnityEngine.PlayerLoop;
using Verse.Noise;

namespace ActiveDefense
{
    public class Projectile_TempestArc : BulletCE
    {
        private static readonly HediffDef EMPStunResist = HediffDef.Named("EMP_StunResistance");
        private static readonly HediffDef EMPSlow = HediffDef.Named("EMP_SlowEffect");
        
        public override void Impact(Thing hitThing)
        {

            if (hitThing != null)
            {
                base.Impact(hitThing);
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && !pawn.Destroyed && !pawn.Dead && !pawn.Downed)
                {
                    if (pawn.stances?.stunner != null && !pawn.health.hediffSet.HasHediff(EMPStunResist))
                    {
                        pawn.stances.stunner.StunFor(Convert.ToInt32(180 * Rand.Range(0.5f, 1.5f)), launcher);
                        pawn.health.AddHediff(EMPStunResist);
                    }
                    else
                    {
                        pawn.health.AddHediff(EMPSlow);
                    }
                    if (Rand.Range(0, 100) < 30)
                    {
                        pawn.TryAttachFire(0.4f, this.launcher);
                    }
                    FleckMaker.ThrowLightningGlow(pawn.Position.ToVector3(), pawn.Map, 2.5f);
                    FleckMaker.ThrowMicroSparks(pawn.Position.ToVector3(), pawn.Map);
                }
            }
            else
                base.Impact(null);
            
        }
        
    }
    public class Verb_TempestShootCE : CombatExtended.Verb_ShootCE
    {
        public override bool CanHitTarget(LocalTargetInfo target)
        {
            if (!base.CanHitTarget(target)) return false;

            var comp = this.EquipmentSource?.GetComp<CompEnergyConsumption>();
            return comp != null && comp.HasCharge;
        }

        public override bool TryCastShot()
        {
            var comp = this.EquipmentSource?.GetComp<CompEnergyConsumption>();
            if (comp != null && comp.HasCharge)
            {
                bool result = base.TryCastShot();
                if (result)
                {
                    comp.ConsumeCharge();
                }
                return result;
            }

            return false;
        }
    }
}
