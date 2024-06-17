using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("5a95724975c24882a5efb0c5aed8c2ab")]
public class GlobalCombatStateTrigger : UnitFactComponentDelegate, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public ActionList ActionsOnEnter;

	public ActionList ActionsOnLeave;

	public void HandleUnitJoinCombat()
	{
		if (base.Owner.IsPreviewUnit)
		{
			PFLog.Default.Log("[HandleUnitJoinCombat] Called on preview unit -- ignoring!");
			return;
		}
		CompanionState? companionState = base.Owner.GetCompanionOptional()?.State;
		if ((companionState.HasValue && companionState.GetValueOrDefault() != CompanionState.InParty) || !(ContextData<EventInvoker>.Current?.InvokerEntity is UnitEntity entity))
		{
			return;
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(ActionsOnEnter, entity.ToITargetWrapper());
		}
	}

	public void HandleUnitLeaveCombat()
	{
		CompanionState? companionState = base.Owner.GetCompanionOptional()?.State;
		if ((companionState.HasValue && companionState.GetValueOrDefault() != CompanionState.InParty) || !(ContextData<EventInvoker>.Current?.InvokerEntity is UnitEntity entity))
		{
			return;
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(ActionsOnLeave, entity.ToITargetWrapper());
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
