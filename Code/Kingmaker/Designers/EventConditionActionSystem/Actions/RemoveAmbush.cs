using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.QA;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c8939e3df36112a4e86bee974c7ddda6")]
public class RemoveAmbush : GameAction
{
	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private bool m_ExitStealth;

	public override string GetCaption()
	{
		return "Remove ambush from " + m_Unit?.ToString() + (m_ExitStealth ? " and exit stealth" : "");
	}

	public override void RunAction()
	{
		if (!(m_Unit.GetValue() is BaseUnitEntity baseUnitEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {m_Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		baseUnitEntity.Stealth.InAmbush = false;
		if (m_ExitStealth)
		{
			baseUnitEntity.Stealth.ShouldExitStealth = true;
			baseUnitEntity.Stealth.WantActivate = false;
			if (Game.Instance.CurrentMode != GameModeType.Default)
			{
				baseUnitEntity.Stealth.Active = false;
				baseUnitEntity.Stealth.Clear();
			}
		}
	}
}
