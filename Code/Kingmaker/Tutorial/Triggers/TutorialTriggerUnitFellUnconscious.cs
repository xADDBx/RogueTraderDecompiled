using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("d76775862c1a4e94bc9867c46fd70fe7")]
public class TutorialTriggerUnitFellUnconscious : TutorialTrigger, IUnitDeathHandler, ISubscriber, IHashable
{
	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (!(Game.Instance.CurrentMode != GameModeType.SpaceCombat))
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = unitEntity as BaseUnitEntity;
		if (baseUnitEntity != null && unitEntity.IsPlayerFaction)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.TargetUnit = baseUnitEntity;
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
