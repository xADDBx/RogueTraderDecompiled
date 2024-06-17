using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[NonOverridable]
[TypeId("856d2f6e26e25c04fad05a1429d1b579")]
public class KingmakerEquipmentEntity : BlueprintScriptableObject
{
	[Serializable]
	public class TwoLists
	{
		public EquipmentEntityLink[] MaleArray;

		public EquipmentEntityLink[] FemaleArray;
	}

	[SerializeField]
	public EquipmentEntityLink[] m_MaleArray;

	[SerializeField]
	public EquipmentEntityLink[] m_FemaleArray;

	[SerializeField]
	public bool m_RaceDependent;

	[SerializeField]
	[ShowIf("m_RaceDependent")]
	public TwoLists[] m_RaceDependentArrays = new TwoLists[8];

	public void Preload(Gender gender, Race race)
	{
		EquipmentEntityLink[] links = GetLinks(gender, race);
		for (int i = 0; i < links.Length; i++)
		{
			links[i].Preload();
		}
	}

	[NotNull]
	public IEnumerable<EquipmentEntity> Load(Gender gender, Race race)
	{
		return from eel in GetLinks(gender, race)
			select eel.Load() into ee
			where ee != null
			select ee;
	}

	[NotNull]
	public EquipmentEntityLink[] GetLinks(Gender gender, Race race)
	{
		if (m_RaceDependent)
		{
			TwoLists twoLists = m_RaceDependentArrays.Get((int)race);
			if (twoLists != null && (twoLists.MaleArray.Length != 0 || twoLists.FemaleArray.Length != 0))
			{
				if (gender != 0)
				{
					return twoLists.FemaleArray;
				}
				return twoLists.MaleArray;
			}
		}
		if (gender != 0)
		{
			return m_FemaleArray;
		}
		return m_MaleArray;
	}
}
