using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using xTile;

namespace FarmVariants
{
	internal class Manager
	{
		internal static Dictionary<string, Dictionary<string, string>> registeredVariants;
		internal static Dictionary<string, Dictionary<string, string>> packVariants = new();
		internal static Dictionary<string, IContentPack> packs = new();
		internal static List<string> validPackMaps = new();
		private static readonly string[] idmap = { "Standard", "Riverlands", "Forest", "Hilltop", "Wilderness", "FourCorners", "Beach", "Custom" };
		internal static string CurrentID = "Default";

		internal const string FLAG = "tlitoo.farmVariant.variant";
		internal const string DATAPATH = "Mods/FarmVariants/Data";
		internal const string MAPPATH = "Maps/FARMVARIANT_";

		internal static void Init()
		{
			ModEntry.helper.Events.Content.AssetsInvalidated += ReloadAssets;
			ModEntry.helper.Events.GameLoop.ReturnedToTitle += LeftGame;
			ModEntry.helper.Events.Content.AssetRequested += AssetRequested;
		}
		private static void AssetRequested(object sender, AssetRequestedEventArgs ev)
		{
			if (ev.NameWithoutLocale.IsEquivalentTo(DATAPATH))
				ev.Edit(AddPacks, AssetEditPriority.Early);
			else if (validPackMaps.ContainsAsset(ev.NameWithoutLocale))
				ev.LoadFrom(() => GetPackMap(ev.NameWithoutLocale.ToString()), AssetLoadPriority.Medium);
		}
		private static Map GetPackMap(string which)
		{
			var path = which[MAPPATH.Length..].Split('|', 3);
			return packs[path[0]].ModContent.Load<Map>($"Maps/{path[1]}/{path[2]}.tmx");
		}
		private static void AddPacks(IAssetData asset)
		{
			var data = (Dictionary<string, Dictionary<string, string>>)asset.Data;
			foreach((var key, var val) in packVariants)
				if (data.TryGetValue(key, out var dict))
					dict.Concat(val);
				else
					data[key] = val;

			// add empty dicts for known custom farms to help out CP
			var farms = ModEntry.helper.GameContent.Load<List<ModFarmType>>("Data/AdditionalFarms");
			foreach (var farm in farms)
				if (!data.ContainsKey(farm.ID))
					data[farm.ID] = new();
			if (Game1.whichModFarm is not null && !data.ContainsKey(Game1.whichModFarm.ID))
				data[Game1.whichModFarm.ID] = new();
		}
		private static void ReloadAssets(object sender, AssetsInvalidatedEventArgs ev)
		{
			foreach (var name in ev.NamesWithoutLocale)
				if (name.IsEquivalentTo(DATAPATH))
					registeredVariants = null;
		}
		private static void LeftGame(object sender, ReturnedToTitleEventArgs ev)
		{
			CurrentID = "Default";
		}
		internal static bool TryGetSavedVariant(out string id)
		{
			id = "";
			if (Game1.MasterPlayer is null)
				return false;
			if (Game1.MasterPlayer.modData.TryGetValue(FLAG, out var saved))
			{
				id = saved;
				return true;
			}
			return false;
		}
		internal static bool TryGetVariant(out string map, string id, int which = -1, string whichCustom = null)
		{
			map = "";
			
			if(!TryGetSelector(out var selector, which, whichCustom))
				return false;

			registeredVariants ??= ModEntry.helper.GameContent.Load<Dictionary<string, Dictionary<string, string>>>(DATAPATH);
			if (registeredVariants.TryGetValue(selector, out var variants) && variants.TryGetValue(id, out var found))
			{
				map = found;
				return true;
			}
			ModEntry.monitor.Log($"Variant '{id}' not registered for '{selector}'.");
			return false;
		}
		internal static bool TryGetRandomVariant(out string map, out string id, int which = -1, string whichCustom = null)
		{
			map = "";
			id = "Default";

			if (!TryGetSelector(out var selector, which, whichCustom))
				return false;

			registeredVariants ??= ModEntry.helper.GameContent.Load<Dictionary<string, Dictionary<string, string>>>(DATAPATH);
			if (registeredVariants.TryGetValue(selector, out var variants))
			{
				var i = Game1.random.Next(variants.Count + 1);
				if (i >= variants.Count)
					return false;

				foreach((var key, var val) in variants)
				{
					if (i == 0)
					{
						id = key;
						map = val;
						return true;
					}
					i--;
				}
			}
			ModEntry.monitor.Log($"No variants found for '{selector}'.");
			return false;
		}
		private static bool TryGetSelector(out string selector, int which, string whichCustom)
		{
			selector = "";

			which = which < 0 ? Game1.whichFarm : which;
			whichCustom ??= Game1.whichModFarm.ID;

			if (which is < 0 or >= 8)
			{
				ModEntry.monitor.Log($"Unknown farm type index {which}.", LogLevel.Trace);
				return false;
			}
			selector = idmap[which];
			if (which == 7)
			{
				if (whichCustom is null)
				{
					ModEntry.monitor.Log($"Requested a custom farm type, but did not provide an ID, and an ID could not be retrieved from the game.");
					return false;
				}
				if (!Utils.CustomFarmExists(whichCustom))
				{
					ModEntry.monitor.Log($"Requested a custom farm type, but did not provide a valid ID", LogLevel.Trace);
					return false;
				}
				selector = whichCustom;
			}
			return true;
		}
	}
}
