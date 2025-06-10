using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InvisibleKittenHolderPart : ViewBasedPart, IDamageHandler, ISubscriber, IHashable
{
	[JsonProperty]
	private UnitReference m_Kitten;

	[NotNull]
	public BaseUnitEntity GetKitten()
	{
		if (m_Kitten == null && base.View != null)
		{
			EntityViewBase entityViewBase = (EntityViewBase)base.View;
			BlueprintUnit invisibleKittenUnit = BlueprintRootReferenceHelper.GetRoot().InvisibleKittenUnit;
			BaseUnitEntity baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(invisibleKittenUnit, entityViewBase.ViewTransform.position, entityViewBase.ViewTransform.rotation, base.ConcreteOwner?.HoldingState);
			baseUnitEntity.IsInGame = false;
			m_Kitten = baseUnitEntity.FromBaseUnitEntity();
			return baseUnitEntity;
		}
		return m_Kitten.ToBaseUnitEntity();
	}

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		UnitReference obj = m_Kitten;
		Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
