using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ActiveDefense
{
    public class CompTempestArcEffect : Projectile
    {
        private static readonly HediffDef EMPStunResist = HediffDef.Named("EMP_StunResistance");
        private static readonly HediffDef EMPSlow = HediffDef.Named("EMP_SlowEffect");
        protected override void Impact(Thing hitThing, bool BlockedByShield = false)
        {
            base.Impact(hitThing);

            if (hitThing != null)
            {
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && !pawn.Destroyed && !pawn.Dead && !pawn.Downed)
                {
                    // 1. Стан
                    if (pawn.stances?.stunner != null && !pawn.health.hediffSet.HasHediff(EMPStunResist))
                    {
                        pawn.stances.stunner.StunFor(60, launcher);
                        pawn.health.AddHediff(EMPStunResist);
                    }
                    else
                    {
                        pawn.health.AddHediff(EMPSlow);
                    }
                    // 2. Шанс на огонь
                    if (Rand.Chance(0.3f))
                    {
                        FireUtility.TryStartFireIn(pawn.Position, pawn.Map, 0.1f, this.launcher);
                    }

                    // 3. Визуальный эффект
                    FleckMaker.ThrowLightningGlow(pawn.Position.ToVector3(), pawn.Map, 2.5f);
                    FleckMaker.ThrowMicroSparks(pawn.Position.ToVector3(), pawn.Map);
                }
            }
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
                Log.Message("Trying make a shot");
                bool result = base.TryCastShot();
                if (result)
                {
                    Log.Message("We created a shot");
                    comp.ConsumeCharge();
                }
                return result;
            }

            return false;
        }
    }
}
