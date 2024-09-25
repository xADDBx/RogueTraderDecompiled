using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints;

public static class ElementExtendAsObject
{
	public static string NameSafe(this Element e)
	{
		return e?.name ?? "null";
	}

	public static T Or<T>(this T e, T replace) where T : Element
	{
		return e ?? replace;
	}

	public static IEnumerable<T> Valid<T>([CanBeNull] this IEnumerable<T> list) where T : Element
	{
		return from i in list.EmptyIfNull()
			where i
			select i;
	}

	public static T ReloadFromInstanceID<T>(this T obj) where T : Element
	{
		return obj;
	}
}
