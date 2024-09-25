using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("f2efec85de75f5e4894158b16f8441ec")]
public class IncreaseResourceAmount : UnitFactComponentDelegate, IResourceAmountBonusHandler<EntitySubscriber>, IResourceAmountBonusHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IResourceAmountBonusHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public int Value = 1;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
	{
		if (base.Fact.Active && resource == Resource)
		{
			bonus += Value * base.Fact.GetRank();
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
