using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StarshipSlotDescription
{
	public int Priority;

	public GameObject Prefab;

	public List<RequiredSlotVariant> RequiredSlots = new List<RequiredSlotVariant>();
}
