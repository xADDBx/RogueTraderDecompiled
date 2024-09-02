using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class UIUtilityGetDialogListenerEntity
{
	private static BlueprintUnit s_ListenerBlueprint;

	public static BaseUnitEntity GetListenerEntity(BlueprintUnit listenerBlueprint, [CanBeNull] BlueprintCueBase cue = null)
	{
		if (listenerBlueprint == null)
		{
			return null;
		}
		s_ListenerBlueprint = listenerBlueprint;
		Vector3 dialogPosition = Game.Instance.DialogController.DialogPosition;
		IEnumerable<BaseUnitEntity> second = Game.Instance.EntitySpawner.CreationQueue.Select((EntitySpawnController.SpawnEntry ce) => ce.Entity).OfType<BaseUnitEntity>();
		MakeEssentialCharactersConscious();
		return (from u in Game.Instance.State.AllBaseUnits.Concat(Game.Instance.Player.Party)
			where u.IsInGame && !u.Suppressed
			select u).Concat(second).Select(SelectMatchingUnit).NotNull()
			.Distinct()
			.Nearest(dialogPosition);
	}

	[CanBeNull]
	private static BaseUnitEntity SelectMatchingUnit(BaseUnitEntity unit)
	{
		BaseUnitEntity baseUnitEntity = null;
		if (unit.Blueprint == s_ListenerBlueprint)
		{
			baseUnitEntity = unit;
		}
		if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsConscious && !baseUnitEntity.LifeState.IsFinallyDead)
		{
			UnitReturnToConsciousController.MakeUnitConscious(baseUnitEntity);
		}
		if (baseUnitEntity != null && !baseUnitEntity.LifeState.IsDead)
		{
			return baseUnitEntity;
		}
		return null;
	}

	private static void MakeEssentialCharactersConscious()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.Blueprint == s_ListenerBlueprint && item.IsEssentialForGame && !item.LifeState.IsConscious)
			{
				UnitReturnToConsciousController.MakeUnitConscious(item);
			}
		}
	}
}
