using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Owlcat.Runtime.Core.Utility;

public static class PoolTypesHolder
{
	[NotNull]
	public static readonly List<Type> Types = new List<Type>();

	public static void Add([NotNull] Type type)
	{
		Types.Add(type);
	}
}
