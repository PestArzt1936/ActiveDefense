using CombatExtended;
using CombatExtended.Utilities;
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
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

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
                    StunHediffCheck(pawn, this.launcher);
                    FireAttachRandomaizer(pawn, this.launcher,30);
                    var list = CreateListOfAims(pawn, (pawn.Position - launcher.Position).ToVector3(), 15, 5, 45);
                    List<Thing_LightningBoltTemp> bolts= new List<Thing_LightningBoltTemp>(list.Count);
                    for(int i=0;i<list.Count;i++)
                    {
                        Thing mainChar;
                        if (i == 0)
                            mainChar = pawn;
                        else
                            mainChar = list[i - 1];
                        bolts.Add((Thing_LightningBoltTemp)ThingMaker.MakeThing(ThingDef.Named("TempLightningBolt")));
                        if (bolts[i] == null)
                            Log.Message("Lightning bolts[i] is null");
                        bolts[bolts.Count-1].Setup(mainChar.Position.ToVector3Shifted(), list[i].Position.ToVector3Shifted());
                        GenSpawn.Spawn(bolts[bolts.Count - 1], list[i].Position, list[i].Map);
                        Log.Message($"{bolts.Count - 1 + 2} bolt is ok");
                        SoundDef.Named("TempestBolt_Tail").PlayOneShot(bolts[i]);
                        //Damage area
                        CustomImpact(list[i], DamageInfo.Amount - 5 * (i + 1));
                        //
                        FleckMaker.ThrowLightningGlow(list[i].Position.ToVector3Shifted(), list[i].Map, 2.5f);
                        FleckMaker.ThrowMicroSparks(list[i].Position.ToVector3Shifted(), list[i].Map);
                        StunHediffCheck(list[i],launcher);
                        FireAttachRandomaizer(list[i], launcher, 30-i*5);
                    }
                    bolts.Clear();
                    list.Clear();
                }
            }
            else
                base.Impact(null);
        }
        private void CustomImpact(Thing hitThing,float ChangedDamage)
        {
            bool flag = launcher is AmmoThing;
            LogEntry_DamageResult logEntry = null;
            if (hitThing != null)
            {
                ProjectilePropertiesCE projectilePropertiesCE = (ProjectilePropertiesCE)def.projectile;
                DamageDefExtensionCE damageDefExtensionCE = def.projectile.damageDef.GetModExtension<DamageDefExtensionCE>() ?? new DamageDefExtensionCE();
                DamageInfo damageInfo = DamageInfo;
                damageInfo.SetAmount(ChangedDamage);
                BodyPartDepth depth = (damageDefExtensionCE.harmOnlyOutsideLayers ? BodyPartDepth.Outside : BodyPartDepth.Undefined);
                BodyPartHeight collisionBodyHeight = new CollisionVertical(hitThing).GetCollisionBodyHeight(ExactPosition.y);
                damageInfo.SetBodyRegion(collisionBodyHeight, depth);
                if (damageDefExtensionCE.harmOnlyOutsideLayers)
                {
                    damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                }

                if (flag && hitThing is Pawn recipient)
                {
                    logEntry = new BattleLogEntry_DamageTaken(recipient, CookOff);
                    Find.BattleLog.Add(logEntry);
                }

                if (launcher == null && hitThing is Pawn recipient2)
                {
                    logEntry = new BattleLogEntry_DamageTaken(recipient2, Shelling);
                    Find.BattleLog.Add(logEntry);
                }

                try
                {
                    DamageWorker.DamageResult damageResult = hitThing.TakeDamage(damageInfo);
                    if (launcher != null)
                    {
                        damageResult.AssociateWithLog(logEntry);
                    }

                    if (!(hitThing is Pawn) && projectilePropertiesCE != null && !projectilePropertiesCE.secondaryDamage.NullOrEmpty())
                    {
                        foreach (SecondaryDamage item in projectilePropertiesCE.secondaryDamage)
                        {
                            if (hitThing.Destroyed || !Rand.Chance(item.chance))
                            {
                                break;
                            }

                            DamageInfo dinfo = item.GetDinfo(damageInfo);
                            hitThing.TakeDamage(dinfo).AssociateWithLog(logEntry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"CombatExtended :: BulletCE impacting thing {hitThing.LabelCap} of def {hitThing.def.LabelCap} added by mod {hitThing.def.modContentPack.Name}.\n{ex}");
                    throw ex;
                }
            }
        }
        private void FireAttachRandomaizer(Thing Aim, Thing instigator, float Chance) {
            if(Rand.Range(0,100)<Chance)
                Aim.TryAttachFire(Rand.Range(0.1f, 0.5f), instigator);

        }
        private void StunHediffCheck(Pawn Aim, Thing instigator, float StunTimer=180)
        {
            if (Aim.stances?.stunner != null && !Aim.health.hediffSet.HasHediff(EMPStunResist))
            {
                Aim.stances.stunner.StunFor(Convert.ToInt32(180 * Rand.Range(0.5f, 1.5f)), instigator);
                Aim.health.AddHediff(EMPStunResist);
            }
            else
            {
                Aim.health.AddHediff(EMPSlow);
            }
        }
        private List<Pawn> CreateListOfAims(Pawn start,Vector3 dir,float Range, float DeltaRange, float Angle)
        {
            List<Pawn> ListOfPawns = CombatExtended.Utilities.GenClosest.PawnsInRange(start.Position, start.Map, Range).Where(p=>p!=null&&!p.Destroyed&&!p.Dead&&!p.Downed&& p.Map == start.Map).ToList();
            if (ListOfPawns == null||ListOfPawns.Count==0)
            {
                return new List<Pawn>();
            }
            for (int i = ListOfPawns.Count-1; i >=0; i--)
            {
                if (Vector3.Angle(dir, ListOfPawns[i].Position.ToVector3()) > Angle || ListOfPawns[i] == start || ListOfPawns[i]==this.launcher)
                {
                    Log.Message($"I deleted 'cause angle:{Vector3.Angle(dir, ListOfPawns[i].Position.ToVector3())}");
                    ListOfPawns.RemoveAt(i);
                }
            }
            for (int i = 0; i < ListOfPawns.Count-1; i++)
            {
                for (int j = 0; j < ListOfPawns.Count-1; j++) {
                    float distCur = (ListOfPawns[j].Position - start.Position).Magnitude;
                    float distNext = (ListOfPawns[j + 1].Position - start.Position).Magnitude;
                    if (distCur > distNext) {
                        ListOfPawns.Swap(j, j + 1);
                    }
                } 
            }
            List<Pawn> Aims = new List<Pawn>();
            Vector3 tempVec = Vector3.zero;
            for(int i = 0; i < ListOfPawns.Count; i++)
            {
                float dist;
                bool angle;
                if (Aims.Count == 0)
                {
                    dist = (ListOfPawns[i].Position - start.Position).Magnitude;
                    angle = Vector3.Angle((ListOfPawns[i].Position - start.Position).ToVector3(), tempVec) <= 90 ? true : false;
                    tempVec = (ListOfPawns[i].Position - start.Position).ToVector3();
                }
                else
                {
                    dist = (ListOfPawns[i].Position - Aims[Aims.Count - 1].Position).Magnitude;
                    angle = Vector3.Angle((ListOfPawns[i].Position - Aims[Aims.Count - 1].Position).ToVector3(), tempVec)<=90?true:false;
                }
                if ((Rand.Range(0, 100) < 75 - 15 * i)&&dist<=DeltaRange&&angle)
                {
                    if (Aims.Count != 0)
                        tempVec = (ListOfPawns[i].Position - Aims[Aims.Count - 1].Position).ToVector3();
                    Aims.Add(ListOfPawns[i]);
                }
            }
            return Aims;
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
