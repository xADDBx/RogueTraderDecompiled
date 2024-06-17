using System.Collections.Generic;
using Kingmaker.Items;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.CharacterSystem;

namespace Kingmaker.View.Equipment;

public class UnitViewMechadendritesEquipment
{
	private readonly List<UnitMechadendriteEquipmentData> m_MechadendritesDatas = new List<UnitMechadendriteEquipmentData>();

	public List<UnitMechadendriteEquipmentData> MechadendritesDatas => m_MechadendritesDatas;

	public UnitViewMechadendritesEquipment(AbstractUnitEntityView owner, Character character)
	{
		List<ItemEntity> mechadendrites = owner.Mechadendrites;
		if (mechadendrites == null)
		{
			return;
		}
		foreach (ItemEntity item in mechadendrites)
		{
			m_MechadendritesDatas.Add(new UnitMechadendriteEquipmentData(owner, character, item));
		}
	}

	public void UpdateAll()
	{
		foreach (UnitMechadendriteEquipmentData mechadendritesData in m_MechadendritesDatas)
		{
			mechadendritesData.RecreateModel();
			mechadendritesData.AttachModel();
		}
	}
}
