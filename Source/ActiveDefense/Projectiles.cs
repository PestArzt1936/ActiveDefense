using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Verse;
using Verse.Noise;
using Verse.Sound;

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
                
                Thing_LightningBoltTemp bolt = (Thing_LightningBoltTemp)ThingMaker.MakeThing(ThingDef.Named("TempLightningBolt"));
                if(bolt==null)
                    Log.Message("Lightning bolt is null");
                bolt.Setup(launcher.Position.ToVector3Shifted(), hitThing.Position.ToVector3Shifted());
                GenSpawn.Spawn(bolt, hitThing.Position,hitThing.Map);
                SoundDef.Named("TempestBolt_Tail").PlayOneShot(bolt);
                base.Impact(hitThing);
                FleckMaker.ThrowLightningGlow(hitThing.Position.ToVector3(), hitThing.Map, 2.5f);
                FleckMaker.ThrowMicroSparks(hitThing.Position.ToVector3(), hitThing.Map);

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
                        pawn.TryAttachFire(Rand.Range(0.1f,0.5f), this.launcher);
                    }
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
