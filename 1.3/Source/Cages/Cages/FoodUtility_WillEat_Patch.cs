using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Analytics;
using Verse;
namespace Cages
{
    [HarmonyPatch(typeof(FoodUtility), "WillEat", new Type[] {typeof(Pawn), typeof(Thing), typeof(Pawn), typeof(bool)})]
    public static class FoodUtility_WillEat_Patch
    {
        public static void Postfix(ref bool __result, Pawn p, Thing food, Pawn getter = null, bool careIfNotAcceptableForTitle = true)
        {
            if (food?.Map != null)
            {
                var cage = food.Position.GetFirstThing<Building_Cage>(food.Map);
                if (cage != null && (!cage.CanWorkOn(p) || getter != null && getter != p && !cage.CanWorkOn(getter)))
                {
                    __result = false;
                }
            }
        }
    }
}
