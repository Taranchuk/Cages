using HarmonyLib;
using Verse;
using Verse.AI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using RimWorld;
using Verse.Noise;

namespace Cages
{
    [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning) })]
    public static class PathFinder_FindPath_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var found = false;
            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!found && codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder lb && lb.LocalIndex == 53)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 41);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 42);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 46);
                    yield return new CodeInstruction(OpCodes.Call, typeof(PathFinder_FindPath_Patch).GetMethod(nameof(PathFinder_FindPath_Patch.PathCostChangeIfNeeded)));
                    yield return new CodeInstruction(OpCodes.Stloc_S, 46);
                }
            }
            if (!found)
            {
                Log.Error("PathFinder.FindPath Transpiler failed. The code won't work.");
            }
        }

        //public static void Postfix(ref PawnPath __result, IntVec3 start, LocalTargetInfo dest, TraverseParms traverseParms, PathEndMode peMode = PathEndMode.OnCell, PathFinderCostTuning tuning = null)
        //{
        //    if (traverseParms.pawn != null && __result != null && __result != PawnPath.NotFound)
        //    {
        //        if (!traverseParms.pawn.CanGoTo(dest.Cell))
        //        {
        //            Log.Error($"{traverseParms.pawn} shouldn't go to {dest.Cell}");
        //        }
        //    }
        //}

        static public int PathCostChangeIfNeeded(Pawn pawn, int xCell, int zCell, int cost)
        {
            var cell = new IntVec3(xCell, 0, zCell);
            if (!pawn.CanGoTo(cell))
            {
                cost += 10000;
            }
            return cost;
        }
    }
}
