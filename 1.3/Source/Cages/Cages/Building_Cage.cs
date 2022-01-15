using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Cages
{
    public class CageExtension : DefModExtension
    {
        public bool isLethal;
        public float maxBodySizeTarget;
        public bool appliesToHumanlikes;
        public int maxPawnCount = 1;
    }
    public class Building_Cage : Building_Storage
    {
        public CageExtension Props => def.GetModExtension<CageExtension>();
        public HashSet<Pawn> cagedPawns;
        public HashSet<Pawn> releasedPawns;
        public Dictionary<Thing, IntVec3> despawnedThings;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            PreInit();
            foreach (var kvp in despawnedThings)
            {
                GenSpawn.Spawn(kvp.Key, this.Position - kvp.Value.RotatedBy(Rotation), map);
                if (kvp.Key is Pawn pawn)
                {
                    cagedPawns.Add(pawn);
                }
            }
        }

        public override IEnumerable<IntVec3> AllSlotCells()
        {
            yield return GenAdj.CellsOccupiedBy(this).OrderBy(x => x.DistanceTo(this.InteractionCell)).First();
        }
        private void PreInit()
        {
            if (cagedPawns is null)
            {
                cagedPawns = new HashSet<Pawn>();
            }
            if (releasedPawns is null)
            {
                releasedPawns = new HashSet<Pawn>();
            }
            if (despawnedThings is null)
            {
                despawnedThings = new Dictionary<Thing, IntVec3>();
            }
        }

        public bool CanWorkOn(Pawn pawn)
        {
            var props = Props;
            if (props.maxBodySizeTarget < pawn.BodySize)
            {
                Log.Message($"{pawn} body size: {pawn.BodySize} is bigger than {props.maxBodySizeTarget}, can't go to {this}");
                return false;
            }
            if (!props.appliesToHumanlikes && pawn.RaceProps.Humanlike)
            {
                Log.Message($"{pawn} is humanlike, can't go to {this}");
                return false;
            }
            if (cagedPawns.Except(pawn).Count() >= props.maxPawnCount)
            {
                Log.Message($"{pawn} cannot go to {this}, full already: {string.Join(", " , cagedPawns)}, props.maxPawnCount: {props.maxPawnCount}, cagedPawns.Except(pawn).Count(): {cagedPawns.Except(pawn).Count()}");
                return false;
            }
            Log.Message($"{pawn} can go to {this}");
            return true;
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Spawned)
            {
                var allPawns = GenAdj.CellsOccupiedBy(this).SelectMany(x => x.GetThingList(Map).OfType<Pawn>()).ToList();
                foreach (var pawn in allPawns)
                {
                    if (!cagedPawns.Contains(pawn) && !releasedPawns.Contains(pawn) && CanWorkOn(pawn))
                    {
                        if (Props.isLethal)
                        {
                            pawn.Kill(null);
                        }
                        else
                        {
                            Log.Message($"Caging {pawn} now");
                            cagedPawns.Add(pawn);
                        }
                    }
                }
                cagedPawns.RemoveWhere(x => !allPawns.Contains(x));
                releasedPawns.RemoveWhere(x => !allPawns.Contains(x));
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
            {
                yield return opt;
            }
            if (cagedPawns.Any())
            {
                yield return new FloatMenuOption("Cages.ReleaseFromCage".Translate(), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Cages_DefOf.CCK_ReleaseFromCage, this));
                });
            }
        }

        public void ReleaseAll(Pawn releaser)
        {
            foreach (var pawn in cagedPawns)
            {
                if (!pawn.Dead && !pawn.Downed)
                {
                    pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Cages_DefOf.CCK_FleeFromCage, this, releaser));
                    releasedPawns.Add(pawn);
                }
            }
            cagedPawns.RemoveWhere(x => releasedPawns.Contains(x));
        }

        private Graphic cageTopGraphic;
        public override void Draw()
        {
            base.Draw();
            if (cageTopGraphic == null)
            {
                cageTopGraphic = def.building.gibbetCageTopGraphicData.GraphicColoredFor(this);
            }
            var drawPos = this.TrueCenter();
            drawPos.y += 9;
            cageTopGraphic.Draw(drawPos, this.Rotation, this);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref cagedPawns, "cagedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref releasedPawns, "releasedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref despawnedThings, "despawnedThings", LookMode.Deep, LookMode.Value);
            PreInit();
        }

        [HarmonyPatch(typeof(MinifyUtility), "MakeMinified")]
        public static class MinifyUtility_MakeMinified_Patch
        {
            public static void Prefix(Thing thing)
            {
                if (thing is Building_Cage cage)
                {
                    cage.OnMinify();
                }
            }
        }

        public void OnMinify()
        {
            var things = new HashSet<Thing>();
            foreach (var c in GenAdj.CellsOccupiedBy(this))
            {
                things.AddRange(c.GetThingList(Map).Where(x => x.def.category == ThingCategory.Item || cagedPawns.Contains(x)).ToList());
            }
            foreach (var thing in things)
            {
                despawnedThings[thing] = (this.Position - thing.Position).RotatedBy(Rotation);
                if (thing is Pawn pawn)
                {
                    cagedPawns.Remove(pawn);
                }
                thing.DeSpawn();
            }
        }
    }
}
