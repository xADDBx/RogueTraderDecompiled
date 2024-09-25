using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Kingmaker.Utility.GuidUtility;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("c436123ee6f44c75a69221bfb7d9024c")]
public class SplitUnitGroup : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override void RunAction()
	{
		if (!(Target.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Target} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			baseUnitEntity.CombatGroup.Id = Uuid.Instance.CreateString();
		}
	}

	public override string GetCaption()
	{
		return $"Split {Target} from group";
	}
}
