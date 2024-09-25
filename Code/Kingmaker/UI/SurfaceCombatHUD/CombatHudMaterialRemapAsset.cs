using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

[CreateAssetMenu(menuName = "ScriptableObjects/CombatHudMaterialRemapAsset")]
public sealed class CombatHudMaterialRemapAsset : ScriptableObject
{
	[Serializable]
	public struct RemapRecord
	{
		[CombatHudMaterialRemapTag]
		public int Tag;

		public Material Material;
	}

	public RemapRecord[] RemapRecords;

	public bool RemapMaterial(int tag, out Material result)
	{
		if (tag > 0 && RemapRecords != null)
		{
			RemapRecord[] remapRecords = RemapRecords;
			for (int i = 0; i < remapRecords.Length; i++)
			{
				RemapRecord remapRecord = remapRecords[i];
				if (remapRecord.Tag == tag)
				{
					result = remapRecord.Material;
					return true;
				}
			}
		}
		result = null;
		return false;
	}

	public bool RemapMaterials(int tag, List<Material> results)
	{
		bool result = false;
		if (tag > 0 && RemapRecords != null)
		{
			RemapRecord[] remapRecords = RemapRecords;
			for (int i = 0; i < remapRecords.Length; i++)
			{
				RemapRecord remapRecord = remapRecords[i];
				if (remapRecord.Tag == tag)
				{
					result = true;
					if (remapRecord.Material != null)
					{
						results.Add(remapRecord.Material);
					}
				}
			}
		}
		return result;
	}
}
