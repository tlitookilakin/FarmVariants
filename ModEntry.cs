using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace FarmVariants
{
	public class ModEntry : Mod
	{
		internal ITranslationHelper i18n => Helper.Translation;
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

			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
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
						AddMapDirectory(dirpath, dir, pack.Manifest.UniqueID);
				else
					monitor.Log($"No Maps folder in content pack '{pack.Manifest.Name}'! This pack will not be loaded!", LogLevel.Warn);
			}
		}
		private static void AddMapDirectory(string path, string name, string id)
		{
			var variants = Manager.packVariants.TryGetValue(name, out var edict) ? edict : Manager.packVariants[name] = new();
			var dirpath = Path.Join(path, name);
			foreach(var file in Directory.EnumerateFiles(dirpath))
			{
				if (".tmx".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
				{
					var vname = Path.GetFileNameWithoutExtension(file);
					var asset = $"FARMVARIANT_{id}|{name}|{vname}";
					Manager.validPackMaps.Add("Maps/" + asset);
					variants.Add(id + '/' + vname, asset);
				} else if(".tbin".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
				{
					monitor.Log($".tbin files are not supported in content packs, only .tmx! Could not load '{file}' from '{id}'.", LogLevel.Warn);
				}
			}
		}
	}
}
