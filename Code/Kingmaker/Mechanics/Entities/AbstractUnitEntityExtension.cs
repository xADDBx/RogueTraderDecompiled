using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
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

	public static bool IsMovementLockedByGameModeOrCombat([NotNull] this AbstractUnitEntity entity)
	{
		GameModeType currentMode = Game.Instance.CurrentMode;
		if (currentMode == GameModeType.Cutscene)
		{
			return true;
		}
		if (currentMode == GameModeType.CutsceneGlobalMap)
		{
			return true;
		}
		if (currentMode == GameModeType.Dialog)
		{
			return true;
		}
		if (currentMode == GameModeType.GameOver)
		{
			return true;
		}
		if (entity.IsInCombat)
		{
			return true;
		}
		return false;
	}
}
