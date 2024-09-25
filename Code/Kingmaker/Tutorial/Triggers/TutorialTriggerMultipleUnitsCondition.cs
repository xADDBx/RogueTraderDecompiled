using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("9e5508085f53bdc4283a7af159bbe14b")]
public class TutorialTriggerMultipleUnitsCondition : TutorialTrigger, IUnitConditionsChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IAreaHandler, IHashable
{
	public UnitCondition TriggerCondition;

	public int MinimumUnitsCount = 4;

	public bool AllowOnGlobalMap;

	public void HandleUnitConditionsChanged(UnitCondition condition)
	{
		if (!AllowOnGlobalMap)
		{
			BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea == null || !currentlyLoadedArea.IsPartyArea)
			{
				return;
			}
		}
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (condition == TriggerCondition && baseUnitEntity.Faction.IsPlayer && baseUnitEntity.State.HasCondition(condition))
		{
			TryToTrigger();
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (!AllowOnGlobalMap)
		{
			BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea == null || !currentlyLoadedArea.IsPartyArea)
			{
				TryToTrigger();
			}
		}
	}

	private void TryToTrigger()
	{
		int num = 0;
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.State.HasCondition(TriggerCondition))
			{
				num++;
			}
		}
		if (num > MinimumUnitsCount)
		{
			TryToTrigger(null);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
