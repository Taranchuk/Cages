using HarmonyLib;
using Verse;

namespace Cages
{
    [HarmonyPatch(typeof(CellFinder), "RandomClosewalkCellNearNotForbidden")]
    public static class CellFinder_RandomClosewalkCellNearNotForbidden_Patch
    {
        public static void Postfix(ref IntVec3 __result, Pawn pawn, int radius)
        {
            if (!pawn.CanGoTo(__result))
            {
                __result = pawn.Position;
            }
        }
    }
}
