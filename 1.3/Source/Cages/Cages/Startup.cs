using HarmonyLib;
using Verse;

namespace Cages
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            new Harmony("Startup.Mod").PatchAll();
        }
    }
}
