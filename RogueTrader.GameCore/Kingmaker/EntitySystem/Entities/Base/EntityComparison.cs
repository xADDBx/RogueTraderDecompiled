using System;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.EntitySystem.Entities.Base;

public static class EntityComparison<TEntity> where TEntity : IEntity
{
	public static Comparison<TEntity> Instance = Compare;

	private static int Compare(TEntity a, TEntity b)
	{
		return string.Compare(a?.UniqueId, b?.UniqueId, StringComparison.Ordinal);
	}
}
