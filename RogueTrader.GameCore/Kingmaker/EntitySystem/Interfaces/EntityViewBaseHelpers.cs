using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public static class EntityViewBaseHelpers
{
	public static IEntityViewBase Or([CanBeNull] this IEntityViewBase we, IEntityViewBase defaultValue)
	{
		if (we == null)
		{
			return defaultValue;
		}
		if (!((we as UnityEngine.Object) ?? throw new InvalidOperationException($"Object {we} of unknown type {we.GetType()}")))
		{
			return defaultValue;
		}
		return we;
	}
}
