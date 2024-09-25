using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("153c40c97b454de0927e15f66fb9289a")]
public class UnitJoinedCombatTrigger : UnitFactComponentDelegate, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, ITurnBasedModeHandler, IHashable
{
	public ActionList Actions;

	public void RunActions()
	{
		using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		}
	}

	public void HandleUnitJoinCombat()
	{
		RunActions();
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			RunActions();
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
