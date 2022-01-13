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
    }
    public class Building_Cage : Building_Storage
    {
        public CageExtension Props => def.GetModExtension<CageExtension>();
        public HashSet<Pawn> cagedPawns;
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

        private void PreInit()
        {
            if (cagedPawns is null)
            {
                cagedPawns = new HashSet<Pawn>();
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
            return true;
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Spawned)
            {
                foreach (var c in this.AllSlotCellsList())
                {
                    foreach (var pawn in c.GetThingList(Map).OfType<Pawn>().ToList())
                    {
                        if (!cagedPawns.Contains(pawn) && CanWorkOn(pawn))
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
                }
            }
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
            foreach (var c in this.AllSlotCellsList())
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
