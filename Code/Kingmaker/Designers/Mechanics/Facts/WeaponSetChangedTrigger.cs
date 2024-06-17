using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Events/WeaponSetChangedTrigger")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFact))]
[TypeId("5b683ca29d3843829eb81362767ae7be")]
public class WeaponSetChangedTrigger : EntityFactComponentDelegate, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private ActionList m_ActionList;

	public void HandleUnitChangeActiveEquipmentSet()
	{
		if (!(base.Fact.Owner is BaseUnitEntity baseUnitEntity) || EventInvokerExtensions.BaseUnitEntity != baseUnitEntity)
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
