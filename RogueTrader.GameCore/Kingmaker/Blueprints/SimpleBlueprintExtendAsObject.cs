using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints;

public static class SimpleBlueprintExtendAsObject
{
	public static string NameSafe(this SimpleBlueprint sb)
	{
		return sb?.name ?? "null";
	}

	public static T Or<T>(this T sb, T replace) where T : SimpleBlueprint
	{
		return sb ?? replace;
	}

	public static IEnumerable<T> Valid<T>([CanBeNull] this IEnumerable<T> list) where T : SimpleBlueprint
	{
		return from i in list.EmptyIfNull()
			where i
			select i;
	}

	public static T ReloadFromInstanceID<T>(this T obj) where T : SimpleBlueprint
	{
		return BlueprintsDatabase.LoadById<T>(obj?.AssetGuid);
	}
}
