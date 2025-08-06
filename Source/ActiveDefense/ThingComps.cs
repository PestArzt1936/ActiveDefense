using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ActiveDefense
{
    public class CompEMPActivator : ThingComp
    {
        public CompProperties_EMPActivator Props => (CompProperties_EMPActivator)this.props;

        public float selectedPct = 1f;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref selectedPct, "selectedPct", 1f);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Mine
            var giz = new Gizmo_EmpCore { comp = this };
            giz.Order = -999f;
            yield return giz;
            //Based
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.CanReserve(parent) && GetNetworkStorage(parent)>=selectedPct)
            {
                yield return new FloatMenuOption("Activate EMP", () =>
                {
                    Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("ActivateEMP"), parent);
                    job.playerForced = true;
                    selPawn.jobs.TryTakeOrderedJob(job);
                });
            }
            else if(selPawn.CanReserve(parent) && GetNetworkStorage(parent) < selectedPct)
            {
                yield return new FloatMenuOption("Not enough energy",null,MenuOptionPriority.High);
            }
        }
        public float GetNetworkStorage(ThingWithComps parent)
        {
            return GetStoredEnergy(parent)/GetTotalEnergy(parent);
        }
        public float GetEnergyFact(ThingWithComps parent)
        {
            var compPower = parent.GetComp<CompPower>();
            
            
            if (compPower?.PowerNet?.batteryComps is List<CompPowerBattery> bats && bats.Count > 0)
            {
                float totalStored = bats.Sum(b => b.StoredEnergy);
                float temp = totalStored / parent.GetComp<CompEMPActivator>().Props.DefaultDamageEnergy;
                return temp;
            }
            else
            {
                return 0f;
            }
        }
        public float GetTotalEnergy(ThingWithComps parent)
        {
            var compPower = parent.GetComp<CompPower>();
            if (compPower?.PowerNet?.batteryComps is List<CompPowerBattery> bats && bats.Count > 0)
            {
                float totalMax = bats.Sum(b => b.Props.storedEnergyMax);
                return totalMax;
            }
            else
            {
                return 0f;
            }
        }
        public float GetStoredEnergy(ThingWithComps parent)
        {
            var compPower = parent.GetComp<CompPower>();
            if (compPower?.PowerNet?.batteryComps is List<CompPowerBattery> bats && bats.Count > 0)
            {
                float totalStored = bats.Sum(b => b.StoredEnergy);
                return totalStored;
            }
            else
            {
                return 0f;
            }
        }
        public void ResetSummaryEnergy(ThingWithComps parent,float necessaryPct=0f)
        {
            var compPower = parent.GetComp<CompPower>();
            List<CompPowerBattery> bats = compPower?.PowerNet?.batteryComps;
            if (bats == null || bats.Count == 0) return;
            foreach (CompPowerBattery i in bats)
            {
                i.SetStoredEnergyPct(necessaryPct);
            }
        }
        public void UniEmpBlast(IntVec3 pos,Map map,float EnergyFactor,ThingWithComps prevParent, float SafeRadius,float DangerRadius)
        {
            DamageDef damageDef = DamageDefOf.EMP;
            List<IntVec3> affectedCells = GenRadial.RadialCellsAround(pos, DangerRadius, true)
                .Where(cell => cell.InBounds(map) && (!cell.InHorDistOf(pos, SafeRadius)))
                .ToList();

            foreach (IntVec3 cell in affectedCells)
            {
                List<Thing> things = cell.GetThingList(map).ToList();
                float dist = (cell - pos).LengthHorizontal;
                foreach (Thing t in things)
                {
                    if (t == prevParent) continue;
                    if (t.Destroyed) continue;
                    if (t is Corpse) continue;
                    float Distfactor = 1f - (dist - SafeRadius) / (DangerRadius * 2);
                    int dmg = Mathf.Max(1, Mathf.RoundToInt(damageDef.defaultDamage * Distfactor * EnergyFactor));
                    DamageInfo dinfo = new DamageInfo(damageDef, dmg, 0, -1f, prevParent);
                    t.TakeDamage(dinfo);
                }
            }

            FleckMaker.Static(pos, map, FleckDefOf.ExplosionFlash, DangerRadius);
            SoundDefOf.MechChargerStart.PlayOneShot(new TargetInfo(pos, map));
            ResetSummaryEnergy(prevParent);
        }
        public void ActivateEMP()
        {
            var pos = parent.Position;
            var map = parent.Map;
            if (map == null) return;
            DamageDef damageDef = DamageDefOf.EMP;

            List<IntVec3> affectedCells = GenRadial.RadialCellsAround(pos, Props.radius, true)
                .Where(cell => cell.InBounds(map)&&(!cell.InHorDistOf(pos,Props.SafeRadius)))
                .ToList();

            foreach (IntVec3 cell in affectedCells)
            {
                List<Thing> things = cell.GetThingList(map).ToList();
                float dist = (cell - pos).LengthHorizontal;
                foreach (Thing t in things)
                {
                    if (t == parent) continue;
                    if(t.Destroyed) continue;
                    if (t is Corpse) continue;
                    float EnergyFactor =GetEnergyFact(parent)*(GetNetworkStorage(parent)-selectedPct);
                    float Distfactor = 1f - (dist-Props.SafeRadius) / (Props.radius * 2);
                    int dmg = Mathf.Max(1, Mathf.RoundToInt(damageDef.defaultDamage * Distfactor * EnergyFactor));
                    DamageInfo dinfo = new DamageInfo(damageDef, dmg, 0, -1f, parent);
                    t.TakeDamage(dinfo);
                }
            }

            FleckMaker.Static(pos, map, FleckDefOf.ExplosionFlash, Props.radius);
            SoundDefOf.MechChargerStart.PlayOneShot(new TargetInfo(pos,map));
            ResetSummaryEnergy(parent,selectedPct);
        }
    }
    public class CompEnergyConsumption : ThingComp
    {
        public CompProperties_EnergyConsumption Props => (CompProperties_EnergyConsumption)this.props;
        public float CurrentCharge;
        public bool HasCharge => CurrentCharge >= Props.BaseEnergyConsumptionPerShot;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            CurrentCharge = Props.BaseEnergyMag;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref CurrentCharge, "CurrentCharge", Props.BaseEnergyMag);
        }
        public void ConsumeCharge()
        {
            CurrentCharge -= Props.BaseEnergyConsumptionPerShot;
            if (CurrentCharge < 0) CurrentCharge = 0;
        }

        public void Recharge(float amount, CompPowerBattery Source)
        {
            float stEn = Source.StoredEnergy;
            float max = Source.Props.storedEnergyMax;
            Source.SetStoredEnergyPct((stEn - amount) / max);

            CurrentCharge = Math.Min(CurrentCharge + amount, Props.BaseEnergyMag);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            Log.Message("Is tick rate working?");
            // Только если в руках у пешки
            if (this.parent.ParentHolder is Pawn_EquipmentTracker tracker)
            {
                Log.Message("You are holding weapon");
                Pawn owner = tracker.pawn;

                // И если он рядом с энергосетью
                var Source = NearestPoweredConduit(owner);
                if (owner.Map != null && Source.Item1!=null)
                {
                    Recharge(Source.Item2,Source.Item1); // Зарядка с честным значением. Если идет разрядка, то неповезло иначе зарядка.
                }
            }
        }

        private (CompPowerBattery,float) NearestPoweredConduit(Pawn pawn)
        {
            var cells = GenRadial.RadialCellsAround(pawn.Position, 5f, true);
            foreach (var cell in cells)
            {
                if (!cell.InBounds(pawn.Map)) continue;

                var thingList = cell.GetThingList(pawn.Map);
                foreach (var thing in thingList)
                {
                    if (thing is Building building && building.TryGetComp<CompPowerTrader>()?.PowerOn == true) {
                        List<CompPowerBattery> bats = building.TryGetComp<CompPowerTrader>()?.PowerNet.batteryComps;
                        foreach (var i in bats)
                        {
                            if (i.StoredEnergy >= 5)
                            {
                                float Gain = i.PowerNet.CurrentEnergyGainRate();
                                return (i, Gain);
                            }
                        }
                            return (null,0); 
                    }
                }
            }

            return (null,0);
        }
        public override string CompInspectStringExtra()
        {
            return $"Energy: {CurrentCharge} / {Props.BaseEnergyMag}";
        }
    }
}
