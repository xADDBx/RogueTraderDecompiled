using System;
using Kingmaker.Blueprints;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Customization;

[Serializable]
public class ClothesSelection
{
	[SerializeField]
	[FormerlySerializedAs("Variations")]
	private KingmakerEquipmentEntityReference[] m_Variations = new KingmakerEquipmentEntityReference[0];

	public ReferenceArrayProxy<KingmakerEquipmentEntity> Variations
	{
		get
		{
			BlueprintReference<KingmakerEquipmentEntity>[] variations = m_Variations;
			return variations;
		}
	}
}
