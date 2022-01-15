using HarmonyLib;
using Verse;
using Verse.AI;
using System;

namespace Cages
{
    [HarmonyPatch(typeof(ReachabilityImmediate), nameof(ReachabilityImmediate.CanReachImmediate), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(Map), typeof(PathEndMode), typeof(Pawn) })]
    public static class ReachabilityImmediate_CanReachImmediate_Patch
    {
        public static void Postfix(ref bool __result, IntVec3 start, LocalTargetInfo target, Map map, PathEndMode peMode, Pawn pawn)
        {
            if (__result && pawn?.Map != null)
            {
                __result = pawn.CanGoTo(target.Cell);
            }
        }
    }
}
