using HarmonyLib;
using System.Linq;
using Verse;

namespace Cages
{
    [StaticConstructorOnStartup]
    public static class Utils
    {
        static Utils()
        {
            new Harmony("Startup.Mod").PatchAll();
        }
        public static bool CanGoTo(this Pawn pawn, IntVec3 cell)
        {
            var cage = pawn.Position.GetFirstThing<Building_Cage>(pawn.Map);
            if (cage != null && cage.cagedPawns.Contains(pawn))
            {
                if (!GenAdj.CellsOccupiedBy(cage).Contains(cell))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool InCage(this Pawn pawn)
        {
            var cage = pawn.Position.GetFirstThing<Building_Cage>(pawn.Map);
            if (cage != null && cage.cagedPawns.Contains(pawn))
            {
                return true;
            }
            return false;
        }
    }
}
