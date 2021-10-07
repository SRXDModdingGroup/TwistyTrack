using HarmonyLib;
using System;
using System.Collections.Generic;

namespace TwistyTrack
{
    public class HarmonyPatches
    {
        private static Dictionary<string, List<Spline.TrackTurnAndContext>> cachedPaths = new Dictionary<string, List<Spline.TrackTurnAndContext>>();
        private static XDLevelSelectMenuBase menuBaseRef;
        private static XDCustomLevelSelectMenu menuCustomRef;
        private static bool isInCustomsMenu = false;

        private static int TwistStrength = 3;

        [HarmonyPatch(typeof(XDLevelSelectMenuBase), nameof(XDLevelSelectMenuBase.OpenMenu))]
        [HarmonyPostfix]
        private static void GetMenuRef(XDLevelSelectMenuBase __instance)
        {
            if (__instance == null) return;
            menuBaseRef = __instance;
            isInCustomsMenu = false;
        }

        [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.OpenMenu))]
        [HarmonyPostfix]
        private static void CustomsMenu(XDCustomLevelSelectMenu __instance)
        {
            if (__instance == null) return;
            menuCustomRef = __instance;
            isInCustomsMenu = true;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
        [HarmonyPostfix]
        private static void GenerateTwistyTrack()
        {
            // Logging history
            // This is a cry for help

            //if (isInCustomsMenu)
            //{
            //    Main.Logger.LogMessage("Customs Menu");
            //    Main.Logger.LogMessage($"Index: {menuCustomRef.currentDifficultyButtonIndex}");
            //    Main.Logger.LogMessage($"Difficulty: {Track.Instance.playStateFirst.CurrentDifficulty}");
            //    Main.Logger.LogMessage($"TrackDataMetadata.Count: {menuCustomRef.CurrentMetaDataHandle.TrackDataMetadata.trackDataMetadata.Count}");
            //    Main.Logger.LogMessage($"Duration: {GetDurationForCustom()}");
            //}
            //else
            //{
            //    Main.Logger.LogMessage("Not Customs");
            //    Main.Logger.LogMessage(menuBaseRef.CurrentMetaDataHandle.TrackDataMetadata.trackDataMetadata[menuBaseRef.currentDifficultyButtonIndex].Duration);
            //}

            // TODO:
            // - Get Track Metadata
            // - Seeded Random Shenanigans
            // - Generate Track Turns

            //Track.Instance.PlayHandle.spline.trackTurns.AddRange();

            // Getting Track Metadata
            TrackInfo info = Track.Instance.playStateFirst.TrackInfoRef.asset;

            // Seeded Random
            string toHash = info.artistName + info.title + info.charter;

            if (!cachedPaths.ContainsKey(toHash))
            {
                var turns = new List<Spline.TrackTurnAndContext>();
                Random random = new Random(toHash.GetHashCode());

                // Generate Track Turns
                float duration;
                if (isInCustomsMenu) duration = GetDurationForCustom();
                else duration = menuBaseRef.CurrentMetaDataHandle.TrackDataMetadata.trackDataMetadata[menuBaseRef.currentDifficultyButtonIndex].Duration;
                float currentTime = 1f;
                do
                {
                    float t = random.Next(5) + 1;
                    turns.Add(new Spline.TrackTurnAndContext()
                    {
                        trackTurn = new TrackTurn()
                        {
                            turnAmount = new UnityEngine.Vector3()
                            {
                                x = random.Next(-5, 6) * TwistStrength * t,
                                y = random.Next(-5, 6) * TwistStrength * t,
                                z = random.Next(-5, 6) * TwistStrength * t
                            },
                            length = t,
                            startTime = currentTime,
                        },
                        //data = new SplinePathData(),
                        data = UnityEngine.ScriptableObject.CreateInstance<SplinePathData>(),
                        defaultSettings = new TrackTurnDefaultSettings(),
                        indexInData = 0,
                        timeOffset = 0,
                    });
                    currentTime += t;
                }
                while (currentTime < duration);
                cachedPaths.Add(toHash, turns);
                Main.Logger.LogMessage($"Cached track path for {info.title} - {info.artistName}, charted by {info.charter}");
            }
            Track.Instance.PlayHandle.spline.trackTurns.Clear();
            foreach (Spline.TrackTurnAndContext turn in cachedPaths[toHash])
                Track.Instance.PlayHandle.spline.trackTurns.Add(turn);
        }

        private static float GetDurationForCustom()
        {
            if (menuCustomRef.CurrentMetaDataHandle.TrackDataMetadata.trackDataMetadata.Count < 5)
                return menuCustomRef.CurrentMetaDataHandle.TrackDataMetadata.trackDataMetadata[menuCustomRef.currentDifficultyButtonIndex].Duration;

            int diff = 0;
            switch (Track.Instance.playStateFirst.CurrentDifficulty)
            {
                case TrackData.DifficultyType.Easy:
                    diff = 0;
                    break;

                case TrackData.DifficultyType.Normal:
                    diff = 1;
                    break;

                case TrackData.DifficultyType.Hard:
                    diff = 2;
                    break;

                case TrackData.DifficultyType.Expert:
                    diff = 3;
                    break;

                case TrackData.DifficultyType.XD:
                    diff = 4;
                    break;

                default:
                    return 0f;
            }
            return menuCustomRef.CurrentMetaDataHandle.TrackDataMetadata.trackDataMetadata[diff].Duration;
        }
    }
}
