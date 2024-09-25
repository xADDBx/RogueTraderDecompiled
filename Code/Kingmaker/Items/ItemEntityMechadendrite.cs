using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechadendrites;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntityMechadendrite : ItemEntity<BlueprintItemMechadendrite>, IHashable
{
	public ItemEntityMechadendrite(BlueprintItemMechadendrite bpItem)
		: base(bpItem)
	{
	}

	public ItemEntityMechadendrite(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void OnDidEquipped(MechanicEntity wielder)
	{
		base.OnDidEquipped(wielder);
		if (!(base.Owner is UnitEntity unitEntity) || !(unitEntity.View != null))
		{
			return;
		}
		unitEntity.View.MechadendritesEquipment?.MechadendritesDatas.Add(new UnitMechadendriteEquipmentData(unitEntity.View, unitEntity.View.CharacterAvatar, this));
		unitEntity.View.MechadendritesEquipment?.UpdateAll();
		UnitPartMechadendrites orCreate = base.Owner.GetOrCreate<UnitPartMechadendrites>();
		if (orCreate != null && !(base.Owner.View == null))
		{
			MechadendriteSettings[] componentsInChildren = base.Owner.View.GetComponentsInChildren<MechadendriteSettings>();
			foreach (MechadendriteSettings settings in componentsInChildren)
			{
				orCreate.RegisterMechadendrite(settings);
			}
		}
	}

	public override void OnWillUnequip()
	{
		base.OnWillUnequip();
		if (!(base.Owner is UnitEntity unitEntity) || !(unitEntity.View != null))
		{
			return;
		}
		UnitMechadendriteEquipmentData unitMechadendriteEquipmentData = unitEntity.View.MechadendritesEquipment?.MechadendritesDatas.Find((UnitMechadendriteEquipmentData x) => x.VisibleItem.Blueprint == base.Blueprint);
		if (unitMechadendriteEquipmentData == null)
		{
			return;
		}
		unitEntity.View.MechadendritesEquipment?.MechadendritesDatas.Remove(unitMechadendriteEquipmentData);
		unitMechadendriteEquipmentData.DestroyModel();
		UnitPartMechadendrites orCreate = base.Owner.GetOrCreate<UnitPartMechadendrites>();
		if (orCreate != null && !(base.Owner.View == null))
		{
			MechadendriteSettings[] componentsInChildren = base.Owner.View.GetComponentsInChildren<MechadendriteSettings>();
			foreach (MechadendriteSettings settings in componentsInChildren)
			{
				orCreate.RegisterMechadendrite(settings);
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
