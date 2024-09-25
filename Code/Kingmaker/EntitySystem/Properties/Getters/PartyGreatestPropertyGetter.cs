using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete("Use ListPropertyGetter instead")]
[TypeId("abafc391cb8e86c47b89b2a5501b7a02")]
public class PartyGreatestPropertyGetter : PropertyGetter
{
	public EntityProperty Property;

	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"${Property}";
	}

	protected override int GetBaseValue()
	{
		List<UnitReference> partyCharacters = Game.Instance.Player.PartyCharacters;
		if (partyCharacters == null || partyCharacters.Count == 0)
		{
			return 0;
		}
		int num = int.MinValue;
		foreach (UnitReference item in partyCharacters)
		{
			BaseUnitEntity baseUnitEntity = item.ToBaseUnitEntity();
			if (m_Fact == null || baseUnitEntity.Facts.Contains((BlueprintUnitFact)m_Fact))
			{
				int value = Property.GetValue(baseUnitEntity);
				num = Mathf.Max(num, value);
			}
		}
		return num;
	}
}
