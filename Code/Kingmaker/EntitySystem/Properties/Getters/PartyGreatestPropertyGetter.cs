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
		int num = int.MinValue;
		List<UnitReference> list = Game.Instance.Player.PartyCharacters;
		if (m_Fact != null)
		{
			list = list.FindAll((UnitReference p) => p.Entity.ToBaseUnitEntity().Facts.Contains((BlueprintUnitFact)m_Fact));
		}
		if (list != null)
		{
			foreach (UnitReference item in list)
			{
				num = ((num < Property.GetValue(item.ToBaseUnitEntity())) ? Property.GetValue(item.ToBaseUnitEntity()) : num);
			}
			return num;
		}
		return 0;
	}
}
