using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace FarmVariants
{
	internal static class Utils
	{
		internal static bool CustomFarmExists(string which)
		{
			if (Game1.whichModFarm is not null && Game1.whichModFarm.ID.Equals(which, StringComparison.Ordinal))
				return true;

			var farms = ModEntry.helper!.GameContent.Load<List<ModFarmType>>("Data/AdditionalFarms");
			foreach (var farm in farms)
				if (farm.ID.Equals(which, StringComparison.Ordinal))
					return true;

			return false;
		}
		internal static IDictionary<K, V> Concat<K, V>(this IDictionary<K, V> dest, IDictionary<K, V> source)
		{
			foreach ((var key, var val) in source)
				dest[key] = val;
			return dest;
		}
		internal static bool ContainsAsset(this IList<string> strings, IAssetName name)
		{
			foreach (var str in strings)
				if (name.IsEquivalentTo(str))
					return true;
			return false;
		}
		internal static bool Matches(this IList<IAssetName> names, string name)
		{
			foreach (var asset in names)
				if (asset.IsEquivalentTo(name))
					return true;
			return false;
		}
	}
}
