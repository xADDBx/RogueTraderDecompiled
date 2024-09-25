using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("1de688dd70524edc9f5a9e250a2b6a9a")]
public class TutorialTriggerLevelUpByFeatureGroup : TutorialTrigger, IStarshipLevelUpHandler, ISubscriber<IStarshipEntity>, ISubscriber, IHashable
{
	[Flags]
	public enum StarshipFeatureGroup
	{
		UltimateAbility = 2,
		ActiveAbility = 4,
		AdvancedAbility = 8,
		ShipUpgrade = 0x10
	}

	[SerializeField]
	public StarshipFeatureGroup StarshipFeatureGroups;

	public void HandleStarshipLevelUp(int newLevel, LevelUpManager manager)
	{
		if (StarshipFeatureGroups != 0 && !(from f in manager.Selections.OfType<SelectionStateFeature>()
			where (f.GetSelectionItemFeatureGroup().ToStarshipFeatureGroup() & StarshipFeatureGroups) != 0
			select f).Empty())
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = manager.TargetUnit;
			});
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
