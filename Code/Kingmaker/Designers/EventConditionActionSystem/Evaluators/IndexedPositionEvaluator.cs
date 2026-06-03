using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("7f4274ce7481411ea04bace4b1b27b0c")]
public class IndexedPositionEvaluator : PositionEvaluator
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnlockableFlagReference m_IndexFlag;

	[SerializeReference]
	public PositionEvaluator[] m_PositionEvaluators = new PositionEvaluator[0];

	private BlueprintUnlockableFlag IndexFlag => m_IndexFlag?.Get();

	public override string GetCaption()
	{
		return "Indexed Evaluator";
	}

	protected override Vector3 GetValueInternal()
	{
		if (m_PositionEvaluators.Length == 0)
		{
			throw new InvalidOperationException("No position evaluators have been set for " + base.Owner.AssetName);
		}
		if (IndexFlag.IsLocked)
		{
			IndexFlag.Unlock();
		}
		int num = Math.Max(IndexFlag.Value, 0) % m_PositionEvaluators.Length;
		return (m_PositionEvaluators[num] ?? throw new InvalidOperationException($"Evaluator at index {num} not found for {base.Owner.AssetName}")).GetValue();
	}
}
