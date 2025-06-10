using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Code.UnitLogic.FactLogic;

[TypeId("b10907b3b5ee476294689757e3563cf9")]
public class MovedInCombatByAbilityTrigger : UnitFactComponentDelegate, IUnitMovedByAbilityHandler, ISubscriber, IHashable
{
	[InfoBox("Current ForceMoved Distance is written into ContextValue1")]
	[SerializeField]
	private ActionList m_Actions;

	public void HandleUnitMovedByAbility(AbilityExecutionContext abilityExecutionContext, int distanceInCells)
	{
		if (distanceInCells < 1)
		{
			return;
		}
		if (base.Context == null)
		{
			PFLog.Default.Error("MovedInCombatByAbilityTrigger. DistanceInCells won't be written into Context: can`t find one");
		}
		else
		{
			if (abilityExecutionContext.Caster != base.Owner)
			{
				return;
			}
			base.Context[ContextPropertyName.Value1] = distanceInCells;
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
		{
			base.Fact.RunActionInContext(m_Actions, base.OwnerTargetWrapper);
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
