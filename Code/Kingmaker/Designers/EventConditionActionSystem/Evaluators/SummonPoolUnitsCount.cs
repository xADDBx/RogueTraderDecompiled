using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/SummonPoolUnitsCount")]
[AllowMultipleComponents]
[TypeId("0a7ef55390dc10c428693321c0e1f6d7")]
public class SummonPoolUnitsCount : IntEvaluator
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSummonPoolReference m_SummonPool;

	public BlueprintSummonPool SummonPool => m_SummonPool?.Get();

	protected override int GetValueInternal()
	{
		return Game.Instance.SummonPools.Get(SummonPool)?.Count ?? 0;
	}

	public override string GetCaption()
	{
		return $"Units in {SummonPool} summonpool";
	}
}
