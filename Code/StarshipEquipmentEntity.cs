using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Space/StarshipEE")]
public class StarshipEquipmentEntity : ScriptableObject
{
	public List<StarshipSlotDescription> EEArtSlotsDescription = new List<StarshipSlotDescription>();
}
