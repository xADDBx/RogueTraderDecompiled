using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[TypeId("82ac3c07409a41378e547140570ff3a9")]
public class StartEndGameTitles : GameAction
{
	[SerializeField]
	private bool m_LoadToMainMenu = true;

	public override void RunAction()
	{
		EventBus.RaiseEvent(delegate(IEndGameTitlesUIHandler h)
		{
			h.HandleShowEndGameTitles(m_LoadToMainMenu);
		});
	}

	public override string GetCaption()
	{
		return "Start end game titles";
	}
}
