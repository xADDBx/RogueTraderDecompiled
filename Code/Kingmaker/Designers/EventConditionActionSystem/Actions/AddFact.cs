using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("7e9b6e3ec852e264e8fcd8c4b5956eee")]
public class AddFact : GameAction
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

	public override string GetDescription()
	{
		return $"Добавляет юниту {Unit} факт {Fact}";
	}

	public override string GetCaption()
	{
		return $"Add Fact ({Fact} to {Unit})";
	}

	public override void RunAction()
	{
		Unit.GetValue().AddFact(Fact)?.TryAddSource(this);
	}
}
