using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

[TypeId("5bb6087af8bc42deb4b2ead62d3f5bf5")]
public class StatGetter : PropertyGetter
{
	[SerializeField]
	private StatType m_Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"{m_Type} of {FormulaTargetScope.Current}";
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetStatOptional(m_Type)?.ModifiedValue ?? 0;
	}
}
