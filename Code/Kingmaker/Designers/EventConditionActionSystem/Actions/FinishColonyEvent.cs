using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("43fcc20ec8304c5dad04eb1bf5839c9c")]
public class FinishColonyEvent : GameAction
{
	[SerializeField]
	private BlueprintColonyEventResult.Reference m_EventResult;

	private BlueprintColonyEventResult EventResult => m_EventResult?.Get();

	public override string GetCaption()
	{
		return "Apply rewards from event";
	}

	public override void RunAction()
	{
		BlueprintColonyEvent blueprintColonyEvent = ContextData<ColonyContextData>.Current?.Event;
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		if ((bool)blueprintColonyEvent)
		{
			colony?.FinishEvent(blueprintColonyEvent, EventResult);
		}
	}
}
