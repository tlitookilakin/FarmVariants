using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections;

namespace FarmVariants
{
	[HarmonyPatch]
	internal class Patch
	{
		private static bool isNewGame = false;

		[HarmonyPatch(typeof(Farm), nameof(Farm.getMapNameFromTypeInt))]
		[HarmonyPostfix]
		internal static string ChangeMapPath(string existing, int type)
		{
			if (Game1.IsMasterGame && isNewGame)
			{
				if (Manager.TryGetRandomVariant(out string outmap, out var id, type))
				{
					Manager.SetVariant(id);
					return outmap;
				}
				ModEntry.monitor.Log("No variants found for selected farm type, using default.", LogLevel.Debug);
			}
			return existing;
		}

		[HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
		[HarmonyPrefix]
		[HarmonyPriority(Priority.First)]
		internal static void LoadingPrefix(bool loadedGame)
			=> isNewGame = !loadedGame;

		[HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
		[HarmonyFinalizer]
		internal static void LoadingFinal()
			=> isNewGame = false;
	}
}
