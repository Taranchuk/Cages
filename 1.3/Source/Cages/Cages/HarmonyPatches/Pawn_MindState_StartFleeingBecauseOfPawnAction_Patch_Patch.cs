using HarmonyLib;
using Verse.AI;

namespace Cages
{
    [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
    public static class Pawn_MindState_StartFleeingBecauseOfPawnAction_Patch_Patch
    {
        public static bool Prefix(Pawn_MindState __instance)
        {
            if (__instance.pawn.InCage())
            {
                return false;
            }
            return true;
        }
    }
}
