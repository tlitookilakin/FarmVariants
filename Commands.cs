using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text;

namespace FarmVariants
{
	internal class Commands
	{
		private static readonly Dictionary<string, Func<string[], string>> subs = new(StringComparer.OrdinalIgnoreCase)
		{
			{"set", SetVariant},
			{"help", ShowHelp},
			{"get", GetVariant},
			{"list", ListVariants},
			{"Reload", Reload}
		};
		private static readonly Dictionary<string, string> help = new(StringComparer.OrdinalIgnoreCase)
		{
			{"set", "Use '?' to select a random variant, 'Default' to reset to the default map, or use the name of a variant to set it to that variant."},
			{"help", "Type the name of a subcommand to get help using it."},
			{"get", "'Default' means no variant is active and the default map is being used."},
			{"list", ""},
			{"reload", "Has no effect if no variant is being used. Useful for hot-reloading content packs."}
		};
		private static readonly Dictionary<string, string> desc = new(StringComparer.OrdinalIgnoreCase)
		{
			{"set", "Sets the variant of the current farm map."},
			{"help", "Shows information about this command and subcommands."},
			{"get", "Gets the current variant."},
			{"list", "Lists all available variants."},
			{"reload", "Reloads the farm map if it is a variant."}
		};
		internal static void ProcessCommand(string cmd, string[] args)
		{
			var which = args.Length > 0 ? args[0] : "help";
			if (subs.TryGetValue(which, out var f))
				ModEntry.monitor.Log(f(args.Length > 1 ? args[1..] : Array.Empty<string>()), LogLevel.Info);
			else
				ModEntry.monitor.Log($"Subcommand '{which}' not recognized. Type 'farm_variant help' to see a list of valid subcommands.", LogLevel.Info);
		}
		private static string ShowHelp(string[] args)
		{
			if (args.Length > 0 && help.TryGetValue(args[0], out var txt))
				return desc[args[0]] + " " + txt;

			var sb = new StringBuilder();
			sb.AppendLine("The following subcommands are available for farm variants:");
			foreach((var cmd, var descr) in desc)
				sb.Append('\t').Append(cmd).Append(":\t").AppendLine(descr);
			return sb.ToString();
		}
		private static string SetVariant(string[] args)
		{
			if (args.Length == 0)
				return ShowHelp(new[]{"set"});

			var name = args.Join(" ");

			if (!Context.IsWorldReady)
				return "Could not set variant, world is not loaded.";

			string path;
			if (name == "?")
				path = Manager.TryGetRandomVariant(out var map, out name) ? map : Manager.GetDefaultMap();
			else if (!Manager.TryGetVariant(out path, name))
				return $"Variant {name} not found for current farm type.";

			Manager.SetVariant(name, "Maps/" + path);
			return $"Successfully switched to variant {name}! (path: {path}).";
		}
		private static string GetVariant(string[] args)
		{
			if (!Context.IsWorldReady)
				return "Could not get variant, world is not loaded.";
			return Manager.TryGetSavedVariant(out var id) ? id : "Default";
		}
		private static string ListVariants(string[] args)
		{
			Manager.registeredVariants ??= ModEntry.helper.GameContent.Load<Dictionary<string, Dictionary<string, string>>>(Manager.DATAPATH);
			var sb = new StringBuilder();
			foreach((var key, var val) in Manager.registeredVariants)
			{
				if (val.Count > 0)
				{
					sb.Append('\t').Append(key).Append(':').AppendLine().Append("\t\t");
					foreach (var variant in val.Keys)
						sb.Append(variant).Append(", ");
					sb.AppendLine();
				}
			}
			return sb.ToString();
		}
		private static string Reload(string[] args)
		{
			if (!Context.IsWorldReady)
				return "No maps to reload! World is not loaded.";

			if (Manager.CurrentID != "Default")
			{
				var farm = Game1.getFarm();
				if (farm is not null) {
					ModEntry.helper.GameContent.InvalidateCache(farm.mapPath.Value);
					farm.reloadMap();
				}
			}

			return "Content pack reloaded";
		}
	}
}
