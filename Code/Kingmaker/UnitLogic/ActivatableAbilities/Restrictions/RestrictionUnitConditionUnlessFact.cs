using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[AllowMultipleComponents]
[ComponentName("AA restriction unit condition")]
[TypeId("27de986d733ccd9498bca34f20529fe7")]
public class RestrictionUnitConditionUnlessFact : ActivatableAbilityRestriction, IHashable
{
	public UnitCondition Condition;

	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintUnitFactReference m_CheckedFact;

	public BlueprintUnitFact CheckedFact => m_CheckedFact?.Get();

	protected override bool IsAvailable()
	{
		if (base.Owner.State.HasCondition(Condition))
		{
			return base.Owner.Facts.Contains(CheckedFact);
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
