using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Cages
{
    public class JobDriver_ReleaseFromCage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }
        public override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoInteractionSpot = new Toil();
            gotoInteractionSpot.initAction = delegate
            {
                Pawn actor = gotoInteractionSpot.actor;
                actor.pather.StartPath(job.targetA, PathEndMode.InteractionCell);
            };
            gotoInteractionSpot.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            gotoInteractionSpot.AddEndCondition(delegate
            {
                LocalTargetInfo target = job.targetA;
                Thing thing = target.Thing;
                if (thing == null && target.IsValid)
                {
                    return JobCondition.Ongoing;
                }
                return (thing != null && thing.Spawned && thing.Map == gotoInteractionSpot.actor.Map) ? JobCondition.Ongoing : JobCondition.Incompletable;
            });
            yield return gotoInteractionSpot;
            yield return Toils_General.Wait(60, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    var cage = job.targetA.Thing as Building_Cage;
                    cage.ReleaseAll(pawn);
                }
            };
        }
    }
}
