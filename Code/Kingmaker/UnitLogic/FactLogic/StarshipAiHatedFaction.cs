using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f317fa10ff77b3f45b4ac4fa3b99d0e5")]
public class StarshipAiHatedFaction : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintFactionReference m_HatedFaction;

	public BlueprintFaction HatedFaction => m_HatedFaction?.Get();

	protected override void OnActivate()
	{
		PartUnitBrain brainOptional = base.Context.MaybeOwner.GetBrainOptional();
		if (brainOptional != null)
		{
			brainOptional.EnemyConditionsDirty = true;
		}
	}

	protected override void OnDeactivate()
	{
		PartUnitBrain brainOptional = base.Context.MaybeOwner.GetBrainOptional();
		if (brainOptional != null)
		{
			brainOptional.EnemyConditionsDirty = true;
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
