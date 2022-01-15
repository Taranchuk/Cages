using HarmonyLib;
using RimWorld;
using Verse;
using static UnityEngine.GraphicsBuffer;
using Verse.Noise;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Verse.AI.Group;

namespace Cages
{
    [HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.InAllowedArea))]
    public static class ForbidUtility_InAllowedArea_Patch
    {
        public static void Postfix(ref bool __result, IntVec3 c, Pawn forPawn)
        {
            if (__result && forPawn?.Map != null)
            {
                __result = forPawn.CanGoTo(c);
            }
        }
    }
}
