using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("6e7c340ab284b5d4abb5b75a0a5da91a")]
public class RemoveFact : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[NotNull]
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Fact")]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	public override string GetCaption()
	{
		return $"Remove Fact ({Fact} from {Unit})";
	}

	public override void RunAction()
	{
		Unit.GetValue().Facts.Remove(Fact);
	}
}
