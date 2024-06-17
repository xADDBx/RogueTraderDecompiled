using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;

namespace Code.GameCore.EntitySystem.Entities.Base;

public class EntityViewBaseCache
{
	public static readonly HashSet<IEntityViewBase> All;

	static EntityViewBaseCache()
	{
		All = new HashSet<IEntityViewBase>();
	}
}
