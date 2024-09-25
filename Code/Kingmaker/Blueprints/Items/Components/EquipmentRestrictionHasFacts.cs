using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[TypeId("d7b23547716f4a949471625ff6c66fb2")]
public class EquipmentRestrictionHasFacts : EquipmentRestriction
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
		bool flag = (!All && Facts.Any((BlueprintUnitFact p) => unit.Facts.Contains(p))) || (All && Facts.All((BlueprintUnitFact p) => unit.Facts.Contains(p)));
		if (!m_Inverted)
		{
			return flag;
		}
		return !flag;
	}

	public bool CheckFactRestriction(BaseUnitEntity unit, BlueprintUnitFact fact)
	{
		if (!m_Inverted)
		{
			return unit.Facts.Contains(fact);
		}
		return !unit.Facts.Contains(fact);
	}
}
