using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c920786099320fb4bb9c3947accc0a64")]
public class ShowMultiEntrance : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintMultiEntranceReference m_Map;

	public BlueprintMultiEntrance Entrance => m_Map?.Get();

	public override string GetCaption()
	{
		return "Show entrance map " + Entrance;
	}

	public override void RunAction()
	{
		if ((bool)Entrance)
		{
			EventBus.RaiseEvent(delegate(IMultiEntranceHandler h)
			{
				h.HandleMultiEntrance(Entrance);
			});
		}
	}
}
