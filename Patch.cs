using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;

namespace FarmVariants
{
	[HarmonyPatch(typeof(Farm))]
	internal class Patch
	{
		[HarmonyPatch(nameof(Farm.getMapNameFromTypeInt))]
		[HarmonyPostfix]
		internal static string ChangeMapPath(string existing, int type)
		{
			string outmap;
			if (Manager.TryGetSavedVariant(out var saved))
			{
				if (Manager.TryGetVariant(out outmap, saved, type))
				{
					Manager.CurrentID = saved;
					return outmap;
				}
				ModEntry.monitor.Log($"Could not find saved variant '{saved}', using default.", LogLevel.Warn);
			}
			else if (!Game1.IsClient)
			{
				if (Game1.Date.TotalDays > 1) // Don't reroll existing saves
					return existing;

				if (Manager.TryGetRandomVariant(out outmap, out var id, type))
				{
					if (Game1.MasterPlayer is not null)
						Game1.MasterPlayer.modData[Manager.FLAG] = id;
					Manager.CurrentID = id;
					ModEntry.monitor.Log($"Chose variant '{id}'.");
					return outmap;
				}
				ModEntry.monitor.Log("No variants found for selected farm type, using default.", LogLevel.Debug);
			}
			return existing;
		}
	}
}
