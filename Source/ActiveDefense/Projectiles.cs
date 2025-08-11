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

namespace ActiveDefense
{
    public class Projectile_TempestBolt : Projectile
    {
        public float segmentLength = 0.6f;
        public float glowSize = 2.5f;

        protected override void Impact(Thing hitThing,bool blockedByShield = false)
        {
            // стандартная обработка (включая звук)
            base.Impact(hitThing, blockedByShield);

            Vector3 start;
            if (this.launcher != null)
                start = this.launcher.DrawPos;
            else
                start = this.ExactPosition;

            Vector3 end;
            if (hitThing != null)
                end = hitThing.DrawPos;
            else
                end = this.ExactPosition;

            var map = base.Map;
            if (map == null)
                return;

            // разбиваем отрезок на сегменты и кидаем Fleck / Glow
            float dist = (end - start).magnitude;
            int segments = Mathf.Max(1, Mathf.CeilToInt(dist / segmentLength));
            for (int i = 0; i <= segments; i++)
            {
                Vector3 pos = start + (end - start) * (i / (float)segments);
                // визуальный эффект: свет / искры
                FleckMaker.ThrowLightningGlow(pos, map, glowSize * Rand.Range(0.8f, 1.2f));
                FleckMaker.ThrowMicroSparks(pos, map);
            }

            // дополнительно можно кинуть небольшой импульс/удар по hitThing здесь (или в другом месте)
        }
    }
    public class Projectile_TempestArc : BulletCE
    {
        private static readonly HediffDef EMPStunResist = HediffDef.Named("EMP_StunResistance");
        private static readonly HediffDef EMPSlow = HediffDef.Named("EMP_SlowEffect");
        //private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

        public override void Impact(Thing hitThing)
        {

            if (hitThing != null)
            {
                
                Thing_LightningBoltTemp bolt = (Thing_LightningBoltTemp)ThingMaker.MakeThing(ThingDef.Named("TempLightningBolt"));
                if(bolt==null)
                    Log.Message("Lightning bolt is null");
                bolt.Setup(launcher.Position.ToVector3Shifted(), hitThing.Position.ToVector3Shifted());
                Log.Message("Spawn lightning bolt");
                GenSpawn.Spawn(bolt, hitThing.Position,hitThing.Map);
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
                    //var lightning = new WeatherEvent_LightningStrike(pawn.Map, pawn.Position);
                    //Map.weatherManager.eventHandler.AddEvent(lightning);
                    
                }
                FleckMaker.ThrowLightningGlow(hitThing.Position.ToVector3(), hitThing.Map, 2.5f);
                FleckMaker.ThrowMicroSparks(hitThing.Position.ToVector3(), hitThing.Map);
            }
            else
                base.Impact(null);
            
            //Vector3 shooterPos = launcher.Position.ToVector3Shifted();
            //Vector3 targetPos = hitThing.Position.ToVector3Shifted();
            //Mesh customBolt = LightningBoltMeshMakerCustom.NewBoltMesh(shooterPos, targetPos);
            //Graphics.DrawMesh(LightningBoltMeshMakerCustom.NewBoltMesh(launcher.Position.ToVector3(), hitThing.Position.ToVector3()), Vector3.zero, Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, 0.6f), 0);

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
