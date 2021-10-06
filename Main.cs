using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace TwistyTrack
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class Main : BasePlugin
    {
        public const string MOD_ID = "TwistyTrack";
        public const string MOD_NAME = "Twisty Track";
        public const string MOD_VERSION = "0.1.1";

        public static BepInEx.Logging.ManualLogSource Logger;

        public override void Load()
        {
            Logger = Log;
            Harmony harmony = new Harmony(MOD_ID);
            harmony.PatchAll(typeof(HarmonyPatches));
        }
    }
}
