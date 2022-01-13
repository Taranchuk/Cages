using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Cages
{
    [HarmonyPatch(typeof(JobDriver_Ingest), "PrepareToIngestToils_NonToolUser")]
    public static class JobDriver_Ingest_PrepareToIngestToils_NonToolUser_Patch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Ingest __instance)
        {
            var food = __instance.job.targetA.Thing;
            if (food.Map != null)
            {
                var cage = food.Position.GetFirstThing<Building_Cage>(food.Map);
                if (cage == null)
                {
                    foreach (var toil in __result)
                    {
                        yield return toil;
                    }
                }
                else
                {
                    yield return __instance.ReserveFood();
                    Toil gotoInteractionSpot = new Toil();
                    gotoInteractionSpot.initAction = delegate
                    {
                        Pawn actor = gotoInteractionSpot.actor;
                        actor.pather.StartPath(cage, PathEndMode.InteractionCell);
                    };
                    gotoInteractionSpot.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                    gotoInteractionSpot.AddEndCondition(delegate
                    {
                        LocalTargetInfo target = cage;
                        Thing thing = target.Thing;
                        if (thing == null && target.IsValid)
                        {
                            return JobCondition.Ongoing;
                        }
                        return (thing != null && thing.Spawned && thing.Map == gotoInteractionSpot.actor.Map) ? JobCondition.Ongoing : JobCondition.Incompletable;
                    });
                    yield return gotoInteractionSpot;

                    Toil gotoFoodNear = new Toil();
                    gotoFoodNear.initAction = delegate
                    {
                        var posToGo = IntVec3.Invalid;
                        var cells = GenSight.PointsOnLineOfSight(food.Position, __instance.pawn.Position).ToList();
                        if (cells.Count > 1)
                        {
                            posToGo = cells[1];
                        }
                        if (!posToGo.IsValid || !posToGo.InBounds(food.Map) || posToGo.GetFirstThing<Building_Cage>(food.Map) is null)
                        {
                            posToGo = food.Position;
                        }
                        gotoFoodNear.actor.pather.StartPath(posToGo, PathEndMode.OnCell);
                    };
                    gotoFoodNear.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                    yield return gotoFoodNear;
                    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                }
            }
            else
            {
                foreach (var toil in __result)
                {
                    yield return toil;
                }
            }
        }
    }

}
