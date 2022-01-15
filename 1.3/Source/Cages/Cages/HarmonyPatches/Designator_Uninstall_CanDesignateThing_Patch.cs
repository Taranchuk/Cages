using HarmonyLib;
using RimWorld;
using Verse;

namespace Cages
{
    [HarmonyPatch(typeof(Designator_Uninstall), nameof(Designator_Uninstall.CanDesignateThing))]
    public static class Designator_Uninstall_CanDesignateThing_Patch
    {
        public static void Postfix(ref AcceptanceReport __result, Thing t)
        {
            if (__result && t is Building_Cage)
            {
                __result = false;
            }
        }
    }
}
