using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("ff647c629c1143fab475eccdd0fee663")]
public abstract class TutorialTriggerTimer : EntityFactComponentDelegate, ITutorialTriggerTimerHandler, ISubscriber, IHashable
{
	[SerializeField]
	protected int TimerValue;

	[SerializeField]
	protected ActionList Actions = new ActionList();

	protected bool CanStart;

	protected bool IsDone;

	public virtual void HandleTimerStart()
	{
		CanStart = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
