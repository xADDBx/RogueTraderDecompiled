using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Customization;

[Serializable]
public class UnitVariations
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Units")]
	private BlueprintUnitReference[] m_Units = new BlueprintUnitReference[0];

	[NotNull]
	public List<UnitCustomizationVariation> Variations = new List<UnitCustomizationVariation>();

	public ReferenceArrayProxy<BlueprintUnit> Units
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] units = m_Units;
			return units;
		}
	}

	public void SetUnits(IEnumerable<BlueprintUnit> units)
	{
		m_Units = units.Select((BlueprintUnit u) => u.ToReference<BlueprintUnitReference>()).ToArray();
	}
}
