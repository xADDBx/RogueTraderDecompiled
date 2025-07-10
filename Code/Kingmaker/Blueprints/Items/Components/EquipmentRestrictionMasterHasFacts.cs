using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[TypeId("6ff7dc35e3e1440ba318292475716f0f")]
public class EquipmentRestrictionMasterHasFacts : EquipmentRestriction
{
	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts = new BlueprintUnitFactReference[0];

	[SerializeField]
	private bool m_Inverted;

	public bool All;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public bool Inverted => m_Inverted;

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		BaseUnitEntity master = ((BaseUnitEntity)unit).Master;
		if (master == null)
		{
			return m_Inverted;
		}
		bool flag = (!All && Facts.Any((BlueprintUnitFact p) => master.Facts.Contains(p))) || (All && Facts.All((BlueprintUnitFact p) => master.Facts.Contains(p)));
		if (!m_Inverted)
		{
			return flag;
		}
		return !flag;
	}

	public bool CheckFactRestriction(BaseUnitEntity unit, BlueprintUnitFact fact)
	{
		BaseUnitEntity master = unit.Master;
		if (master == null)
		{
			return m_Inverted;
		}
		if (!m_Inverted)
		{
			return master.Facts.Contains(fact);
		}
		return !master.Facts.Contains(fact);
	}
}
