using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Events/LastAbilityWeaponChangedTrigger")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFact))]
[TypeId("0592e6edc97d472cbbc6b58c0127cf7c")]
public class LastAbilityWeaponChangedTrigger : EntityFactComponentDelegate, ILastAbilityWeaponChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private ActionList m_ActionList;

	public void HandleLastAbilityWeaponChange(ItemEntityWeapon oldWeapon, ItemEntityWeapon newWeapon)
	{
		if (!(base.Fact.Owner is BaseUnitEntity baseUnitEntity) || EventInvokerExtensions.BaseUnitEntity != baseUnitEntity || oldWeapon == null || newWeapon == null)
		{
			return;
		}
		using (baseUnitEntity.Context.GetDataScope())
		{
			m_ActionList?.Run();
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
