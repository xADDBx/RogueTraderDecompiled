using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints;

public static class BlueprintComponentExtendAsObject
{
	public static string NameSafe(this BlueprintComponent c)
	{
		return c?.name ?? "null";
	}

	public static T Or<T>(this T c, T replace) where T : BlueprintComponent
	{
		return c ?? replace;
	}

	public static IEnumerable<T> Valid<T>([CanBeNull] this IEnumerable<T> list) where T : BlueprintComponent
	{
		return from i in list.EmptyIfNull()
			where i
			select i;
	}

	public static T ReloadFromInstanceID<T>(this T obj) where T : BlueprintComponent
	{
		return obj;
	}
}
