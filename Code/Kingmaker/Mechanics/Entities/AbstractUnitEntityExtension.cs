using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Mechanics.Entities;

public static class AbstractUnitEntityExtension
{
	[NotNull]
	public static AbstractUnitEntity ToAbstractUnitEntity(this IAbstractUnitEntity entity)
	{
		return (AbstractUnitEntity)entity;
	}

	[NotNull]
	public static IBaseUnitEntity ToIBaseUnitEntity(this IAbstractUnitEntity entity)
	{
		return (IBaseUnitEntity)entity;
	}

	[NotNull]
	public static BaseUnitEntity ToBaseUnitEntity(this IAbstractUnitEntity entity)
	{
		return (BaseUnitEntity)entity;
	}
}
