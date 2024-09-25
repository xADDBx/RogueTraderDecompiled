using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e8c62d9f107f952409c42b08fb8633b7")]
public class AbilityWithTagUsedTrigger : UnitFactComponentDelegate, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[Serializable]
	private class AllowedCastersData
	{
		public bool Everyone = true;

		[HideIf("Everyone")]
		public bool Owner;

		[HideIf("Everyone")]
		public bool Allies;

		[HideIf("Everyone")]
		public bool Enemies;
	}

	[SerializeField]
	private AbilityTag AbilityTag;

	[SerializeField]
	private AllowedCastersData AllowedCasters;

	[SerializeField]
	private ActionList Actions;

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (context.Ability.Blueprint.AbilityTag == AbilityTag && CheckCaster(context.Ability.Caster))
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	private bool CheckCaster(MechanicEntity caster)
	{
		if (!AllowedCasters.Everyone && (!AllowedCasters.Owner || caster != base.Owner) && (!AllowedCasters.Allies || base.Owner == caster || !base.Owner.IsAlly(caster)))
		{
			if (AllowedCasters.Enemies)
			{
				return base.Owner.IsEnemy(caster);
			}
			return false;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
