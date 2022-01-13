using HarmonyLib;
using RimWorld;
using Verse;

namespace Cages
{
    [HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.InAllowedArea))]
    public static class ForbidUtility_InAllowedArea_Patch
    {
        public static void Postfix(ref bool __result, IntVec3 c, Pawn forPawn)
        {
            if (__result)
            {
                var cage = forPawn.Position.GetFirstThing<Building_Cage>(forPawn.Map);
                if (cage != null && cage.cagedPawns.Contains(forPawn))
                {
                    if (!cage.AllSlotCellsList().Contains(c))
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
