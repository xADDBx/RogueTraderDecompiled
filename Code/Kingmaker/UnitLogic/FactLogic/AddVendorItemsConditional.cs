using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("69df051a199946ffba848a012609c2b8")]
public class AddVendorItemsConditional : MechanicEntityFactComponentDelegate, IVendorLogicStateChanged, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public bool AlreadyTriggered;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref AlreadyTriggered);
			return result;
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private ConditionsChecker m_Conditions;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitLootReference m_Loot;

	private void TryTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (!savableData.AlreadyTriggered && m_Conditions.Check())
		{
			BlueprintUnitLoot blueprintUnitLoot = m_Loot.Get();
			PartVendor required = base.Owner.GetRequired<PartVendor>();
			if (blueprintUnitLoot != null)
			{
				required.AddLoot(blueprintUnitLoot);
				savableData.AlreadyTriggered = true;
			}
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		base.OnActivateOrPostLoad();
		TryTrigger();
	}

	void IVendorLogicStateChanged.HandleBeginTrading()
	{
	}

	void IVendorLogicStateChanged.HandleEndTrading()
	{
	}

	void IVendorLogicStateChanged.HandleVendorAboutToTrading()
	{
		if (base.Owner == EventInvokerExtensions.MechanicEntity)
		{
			TryTrigger();
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
