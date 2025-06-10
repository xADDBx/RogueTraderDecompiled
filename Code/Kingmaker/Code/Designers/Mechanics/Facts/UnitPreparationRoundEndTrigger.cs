using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("a56ab30bce234409b921a38d80d6650d")]
public class UnitPreparationRoundEndTrigger : UnitFactComponentDelegate, IPreparationTurnEndHandler, ISubscriber, IHashable
{
	public ActionList Actions;

	public void RunActions()
	{
		using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		}
	}

	public void HandleEndPreparationTurn()
	{
		if (base.Owner.IsInCombat)
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
