using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ActiveDefense
{
    public class JobDriver_ActivateEMP : JobDriver
    {
        private const int ActivationDuration = 300;

        public Building EMPBuilding => TargetA.Thing as Building;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.GetTarget(TargetIndex.A); // здание-эмиттер EMP
                                                                        // Пытаемся зарезервировать цель (одно устройство) для текущей работы
            if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            return true;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // Действие: ждать 30 секунд
            var waitToil = Toils_General.Wait(ActivationDuration);
            waitToil.WithProgressBarToilDelay(TargetIndex.A);
            waitToil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return waitToil;
            //
            Toil activateToil = new Toil();
            activateToil.initAction = () =>
            {
                Pawn actor = activateToil.actor;
                // Получаем Thing цели (здание-эмиттер EMP)
                Thing buildingThing = actor.CurJob.GetTarget(TargetIndex.A).Thing;
                if (buildingThing != null)
                {
                    // Находим компонент CompEMPActivator у здания
                    CompEMPActivator comp = buildingThing.TryGetComp<CompEMPActivator>();
                    if (comp != null)
                    {
                        // Вызываем метод активации EMP-взрыва на найденном компоненте
                        comp.ActivateEMP();
                    }
                }
            };
            activateToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return activateToil;
        }

    }
}
