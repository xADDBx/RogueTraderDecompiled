using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem.ContextData;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add loot")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("aea59f4a6ffae1e45a67d731f3f7908f")]
public class AddLoot : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	public BlueprintUnitLootReference LootReference => m_Loot;

	protected override void OnFactAttached()
	{
		if (!ContextData<UnitHelper.DoNotCreateItems>.Current && !m_Loot.IsEmpty())
		{
			m_Loot.Get().GenerateItems().ForEach(delegate(LootEntry i)
			{
				base.Owner.Inventory.Add(i.Item, i.Count);
			});
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
