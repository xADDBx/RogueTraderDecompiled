using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Utility.UnityExtensions;

public static class GenericStaticTypesHolder
{
	[NotNull]
	public static readonly List<Type> Types = new List<Type>();

	public static void Add([NotNull] Type type)
	{
		Types.Add(type);
	}
}
