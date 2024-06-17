using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[TypeId("80549774478d4678b0dc46dfb04461f1")]
public class EquipmentRestrictionPlayerHasFacts : EquipmentRestriction
{
	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts = new BlueprintUnitFactReference[0];

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public override bool CanBeEquippedBy(MechanicEntity unit)
	{
		return Facts.Any((BlueprintUnitFact p) => GameHelper.GetPlayerCharacter().Facts.Contains(p));
	}
}
