using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("fb77900c7ec1461dadb27600db6e67cb")]
public class FactListGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintUnitFactReference[] m_Facts = new BlueprintUnitFactReference[0];

	public bool OnlyFromCaster;

	[ShowIf("OnlyFromCaster")]
	public PropertyTargetType Caster;

	public bool MultiplyByRanks;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.Facts.List.Sum(delegate(EntityFact p)
		{
			if (!Facts.Contains(p.Blueprint) || (OnlyFromCaster && p.MaybeContext?.MaybeCaster != this.GetTargetByType(Caster)))
			{
				return 0;
			}
			return (!MultiplyByRanks) ? 1 : p.GetRank();
		});
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = string.Join(',', m_Facts.Select((BlueprintUnitFactReference f) => f.NameSafe()));
		if (text.Length > 80)
		{
			text = text.Substring(0, 80) + "...";
		}
		return "Count all facts from the list [" + text + "]";
	}
}
