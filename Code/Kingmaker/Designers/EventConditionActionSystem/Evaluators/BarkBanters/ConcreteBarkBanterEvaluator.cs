using System;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;

[Serializable]
[TypeId("ee5c2e43efcc4b179ebdd0fa5c174c28")]
public class ConcreteBarkBanterEvaluator : BarkBanterEvaluator
{
	[SerializeField]
	private BlueprintBarkBanterReference m_BarkBanterReference;

	public override string GetCaption()
	{
		return "Get concrete bark banter";
	}

	protected override BlueprintBarkBanter GetValueInternal()
	{
		return m_BarkBanterReference.Get();
	}
}
