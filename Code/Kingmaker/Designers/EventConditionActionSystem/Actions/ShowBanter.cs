using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[ComponentName("Actions/ShowBanter")]
[AllowMultipleComponents]
[TypeId("0b9dc26e13a64efe891a0388ebe5d0cd")]
public class ShowBanter : GameAction
{
	[SerializeReference]
	public BarkBanterEvaluator m_BarkBanterEvaluator;

	public override string GetCaption()
	{
		return "Show Banter";
	}

	protected override void RunAction()
	{
		if (m_BarkBanterEvaluator.TryGetValue(out var barkBanter))
		{
			EventBus.RaiseEvent(delegate(IBarkBanterPlayedHandler e)
			{
				e.HandleBarkBanter(barkBanter);
			});
		}
	}
}
