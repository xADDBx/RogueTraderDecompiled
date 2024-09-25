using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.View;
using Kingmaker.View.MapObjects;

namespace Kingmaker.EntitySystem.Entities;

public static class MechanicEntityHelper
{
	public static readonly Comparison<Entity> ByIdComparison = (Entity a, Entity b) => string.Compare(a.UniqueId, b.UniqueId, StringComparison.Ordinal);

	[CanBeNull]
	public static UnitVisualSettings.MusicCombatState? GetCombatMusic(this MechanicEntity entity)
	{
		return (entity.View as UnitEntityView)?.CombatMusic;
	}

	[CanBeNull]
	public static ILootable GetLoot(this MechanicEntity entity)
	{
		if (!entity.GetOptional<InteractionLootPart>())
		{
			return null;
		}
		return entity as ILootable;
	}
}
