using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Timer;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("10a31fac3d01467681d98ff8aa7608b9")]
public class ContextActionStartPlayerTimer : ContextAction
{
	[SerializeField]
	private ContextValue m_DurationSeconds;

	[SerializeField]
	private PlayerTimer.ScopeType m_Scope;

	[SerializeField]
	[ValidateNotNull]
	private ActionsHolderReference m_ActionsRef;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPlayerTimer.Reference m_Blueprint;

	public override string GetCaption()
	{
		return "Start player timer " + m_Blueprint.NameSafe();
	}

	protected override void RunAction()
	{
		if (m_ActionsRef == null || m_ActionsRef.IsEmpty())
		{
			Element.LogError(this, "No timer actions set!");
			return;
		}
		if (m_Blueprint == null || m_Blueprint.IsEmpty())
		{
			Element.LogError(this, "No timer blueprint is set!");
			return;
		}
		PlayerTimer timer = new PlayerTimer(m_ActionsRef.Get(), m_DurationSeconds.Calculate(base.Context), m_Scope, m_Blueprint);
		Game.Instance.Player.Timers.Start(timer);
	}
}
