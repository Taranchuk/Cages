using HarmonyLib;
using Verse;
using Verse.AI;

namespace Cages
{
    //[HarmonyPatch(typeof(Pawn_PathFollower), "StartPath")]
    //public static class Pawn_PathFollower_Patch
    //{
    //    public static void Postfix(Pawn ___pawn, LocalTargetInfo dest, PathEndMode peMode)
    //    {
    //        if (!___pawn.CanGoTo(dest.Cell))
    //        {
    //            Log.Error($"{___pawn} shouldn't go to {dest.Cell} 2");
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(JobGiver_ExitMap), "TryGiveJob")]
    public static class JobGiver_ExitMap_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.InCage())
            {
                return false;
            }
            return true;
        }
    }
}
