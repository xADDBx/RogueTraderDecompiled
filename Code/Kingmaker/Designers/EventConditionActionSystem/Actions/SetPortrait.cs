using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("bfefd4b0c2e912a4a8d9e0bcde22a95c")]
[PlayerUpgraderAllowed(true)]
public class SetPortrait : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	[FormerlySerializedAs("Portrait")]
	private BlueprintPortraitReference m_Portrait;

	public BlueprintPortrait Portrait => m_Portrait?.Get();

	public override string GetCaption()
	{
		return $"Set Portrait ({Portrait} to {Unit})";
	}

	protected override void RunAction()
	{
		if (!(Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			baseUnitEntity.UISettings.SetPortrait(Portrait);
		}
	}
}
