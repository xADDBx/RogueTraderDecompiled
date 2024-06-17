using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("92d47e384bca4f328c246dd05139d6d8")]
public class UIEventTrigger : EntityFactComponentDelegate, IUIEventHandler, ISubscriber, IHashable
{
	public UIEventType EventType;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	public void HandleUIEvent(UIEventType type)
	{
		if (type == EventType)
		{
			Game.Instance.GameCommandQueue.UIEventTrigger(this);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
