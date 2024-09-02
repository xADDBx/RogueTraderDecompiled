using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("3fe34667d45f4ff2a1614dd885d8c2d3")]
public class SoulMarkRank : IntEvaluator
{
	public SoulMarkDirection SoulMark;

	public override string GetCaption()
	{
		return $"Rank of soul mark {SoulMark}";
	}

	protected override int GetValueInternal()
	{
		return SoulMarkShiftExtension.GetSoulMark(SoulMark)?.Rank ?? 0;
	}
}
