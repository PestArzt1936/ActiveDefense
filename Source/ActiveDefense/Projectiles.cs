using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CombatExtended;
using UnityEngine.PlayerLoop;

namespace ActiveDefense
{
    public class Projectile_TempestArc : ProjectileCE
    {
        private static readonly HediffDef EMPStunResist = HediffDef.Named("EMP_StunResistance");
        private static readonly HediffDef EMPSlow = HediffDef.Named("EMP_SlowEffect");
        
        public override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                Pawn pawn = hitThing as Pawn;
                if (pawn != null && !pawn.Destroyed && !pawn.Dead && !pawn.Downed)
                {
                    if (pawn.stances?.stunner != null && !pawn.health.hediffSet.HasHediff(EMPStunResist))
                    {
                        Log.Message("Накладываю стан");
                        pawn.stances.stunner.StunFor(60, launcher);
                        pawn.health.AddHediff(EMPStunResist);
                    }
                    else
                    {
                        Log.Message("Накладываю замедление");
                        pawn.health.AddHediff(EMPSlow);
                    }
                    if (Rand.Chance(0.3f))
                    {
                        FireUtility.TryStartFireIn(pawn.Position, pawn.Map, 0.1f, this.launcher);
                    }
                    Log.Message("Конец проверки на живую пешку");
                    FleckMaker.ThrowLightningGlow(pawn.Position.ToVector3(), pawn.Map, 2.5f);
                    FleckMaker.ThrowMicroSparks(pawn.Position.ToVector3(), pawn.Map);
                }
                Log.Message("Соси хуй, ты не прошел проверку на живую пешку");
            }
            Log.Message("Соси хуй, ты не попал?");
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
