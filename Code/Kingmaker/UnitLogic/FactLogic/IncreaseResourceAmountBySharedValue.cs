using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("6569de6749ae3b749883f9fedbd76ac7")]
public class IncreaseResourceAmountBySharedValue : UnitFactComponentDelegate, IResourceAmountBonusHandler<EntitySubscriber>, IResourceAmountBonusHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IResourceAmountBonusHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public ContextValue Value;

	public bool Decrease;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
	{
		if (base.Fact.Active && resource == Resource)
		{
			if (!Decrease)
			{
				bonus += Value.Calculate(base.Context);
			}
			else
			{
				bonus -= Value.Calculate(base.Context);
			}
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
