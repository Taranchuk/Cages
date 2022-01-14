using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Cages
{
    public class JobDriver_FleeFromCage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
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
            yield return new Toil
            {
                initAction = delegate
                {
                    List<Thing> threats = new List<Thing> { job.targetB.Thing };
                    IntVec3 fleeDest = CellFinderLoose.GetFleeDest(pawn, threats, pawn.Position.DistanceTo(job.targetB.Thing.Position) + 28f);
                    this.job.SetTarget(TargetIndex.C, fleeDest);
                    this.job.locomotionUrgency = LocomotionUrgency.Sprint;
                }
            };
            yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
        }
    }
}
