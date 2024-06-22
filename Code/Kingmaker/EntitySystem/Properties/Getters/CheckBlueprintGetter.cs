using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("a4316c207b21c9b4fa4c5ec4c9300c9b")]
public class CheckBlueprintGetter : MechanicEntityPropertyGetter
{
	[SerializeField]
	private BlueprintMechanicEntityFact.Reference m_Blueprint;

	public BlueprintMechanicEntityFact Blueprint => m_Blueprint?.Get();

	protected override int GetBaseValue()
	{
		if (base.CurrentEntity.Blueprint != Blueprint)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return string.Format("{0} BP is {1}", FormulaTargetScope.Current, (Blueprint != null) ? ((object)Blueprint) : ((object)"<null>"));
	}
}
