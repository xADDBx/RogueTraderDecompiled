using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("afdb9910a3164c2ab7a573983b2bd959")]
public class UnitUpgraderComponent : BlueprintComponent
{
	[SerializeField]
	private BlueprintUnitUpgrader.Reference[] m_Upgraders = new BlueprintUnitUpgrader.Reference[0];

	public ReferenceArrayProxy<BlueprintUnitUpgrader> Upgraders
	{
		get
		{
			BlueprintReference<BlueprintUnitUpgrader>[] upgraders = m_Upgraders;
			return upgraders;
		}
	}

	public void AddUpgrader(BlueprintUnitUpgrader.Reference upgrader)
	{
		int num = m_Upgraders.Length;
		Array.Resize(ref m_Upgraders, m_Upgraders.Length + 1);
		m_Upgraders[num] = upgrader;
	}

	public IEnumerable<BlueprintUnitUpgrader> EnumerateUpgraders(bool fromPlaceholders)
	{
		return Upgraders.Where((BlueprintUnitUpgrader u) => u.ApplyFromPlaceholder == fromPlaceholders);
	}
}
