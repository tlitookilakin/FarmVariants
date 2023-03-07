using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FarmVariants
{
	public class ModEntry : Mod
	{
		internal static ITranslationHelper i18n;
		internal static IMonitor monitor;
		internal static IModHelper helper;
		internal static Harmony harmony;
		internal static string ModID = "";

		public override void Entry(IModHelper helper)
		{
			Monitor.Log("Starting up...", LogLevel.Debug);

			monitor = Monitor;
			ModEntry.helper = Helper;
			harmony = new(ModManifest.UniqueID);
			ModID = ModManifest.UniqueID;
			i18n = Helper.Translation;

			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += OnWorldLoaded;
			helper.ConsoleCommands.Add("farm_variant", "Master command for Farm Variants. Use 'farm_variants help' for more info.", Commands.ProcessCommand);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void OnGameLaunched(object sender, GameLaunchedEventArgs ev)
		{
			LoadContentPacks();
			Manager.Init();
			harmony.PatchAll();
		}

		private static void LoadContentPacks()
		{
			foreach (var pack in helper.ContentPacks.GetOwned())
			{
				Manager.packs[pack.Manifest.UniqueID] = pack;
				var dirpath = Path.Combine(pack.DirectoryPath, "Maps");
				if (Directory.Exists(dirpath))
					foreach (var dir in Directory.EnumerateDirectories(dirpath))
						AddMapDirectory(dir, pack.Manifest.UniqueID);
				else
					monitor.Log($"No Maps folder in content pack '{pack.Manifest.Name}'! This pack will not be loaded!", LogLevel.Warn);
			}
		}
		private static void AddMapDirectory(string path, string id)
		{
			var name = Path.GetFileName(path);
			var variants = Manager.packVariants.TryGetValue(name, out var edict) ? edict : Manager.packVariants[name] = new();
			foreach(var file in Directory.EnumerateFiles(path))
			{
				if (".tmx".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
				{
					var vname = Path.GetFileNameWithoutExtension(file);
					var asset = $"FARMVARIANT_{id}>{name}>{vname}";
					Manager.validPackMaps.Add("Maps/" + asset);
					variants.Add(id + '/' + vname, asset);
				} else if(".tbin".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
				{
					monitor.Log($".tbin files are not supported in content packs, only .tmx! Could not load '{file}' from '{id}'.", LogLevel.Warn);
				}
			}
		}
		private static void OnWorldLoaded(object sender, SaveLoadedEventArgs ev)
		{
			if (!Game1.IsMasterGame)
				return;

			if (Manager.TryGetSavedVariant(out var variant))
				if (Manager.TryGetVariant(out var map, variant))
					Manager.SetVariant(variant, "Maps/" + map);
				else
					monitor.Log($"Could not find map variant '{variant}'!", LogLevel.Warn);
			else
				monitor.Log($"No map variant selected.");
		}
	}
}
