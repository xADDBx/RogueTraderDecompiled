using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(menuName = "Character System/Equipment Entities List")]
public class EquipmentEntitiesList : ScriptableObject
{
	public List<EquipmentEntity> EquipmentEntities = new List<EquipmentEntity>();
}
