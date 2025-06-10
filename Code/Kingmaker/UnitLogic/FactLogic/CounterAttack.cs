using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("21dbbb0b719a4ce38c2152bf9ea6bbce")]
public class CounterAttack : UnitFactComponentDelegate, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int? UsageLimit;

		[JsonProperty]
		public int UsageCount;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			if (UsageLimit.HasValue)
			{
				int val2 = UsageLimit.Value;
				result.Append(ref val2);
			}
			result.Append(ref UsageCount);
			return result;
		}
	}

	public enum TriggerType
	{
		BeforeAttack = -1,
		AfterParryAttack = 0,
		AfterDodgeAttack = 2,
		AfterBlockAttack = 3,
		AfterAnyAttack = 1
	}

	public TriggerType Trigger;

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public bool GuardAllies;

	[ShowIf("GuardAllies")]
	public bool GuardAlliesCanMove = true;

	[ShowIf("GuardAllies")]
	public ContextValue GuardAlliesRange;

	[ShowIf("GuardAllies")]
	public RestrictionCalculator GuardAlliesRestriction = new RestrictionCalculator();

	public bool Limited;

	[ShowIf("Limited")]
	public ContextValue UsageLimit;

	public bool CanUseInRange;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartCounterAttack>().Add(base.Fact, this, Limited ? UsageLimit.Calculate(base.Context) : (-1));
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartCounterAttack>()?.Remove(base.Fact, this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
