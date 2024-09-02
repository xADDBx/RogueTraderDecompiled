using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("947812f74be2492ba3f7dbc222302f0e")]
public class DoorNotOpeningTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, IInteractionHandler, ISubscriber<IBaseUnitEntity>, IHashable
{
	private TimeSpan m_TimeSinceNotOpening = TimeSpan.Zero;

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentMode != GameModeType.Default))
		{
			m_TimeSinceNotOpening += delta;
			if (m_TimeSinceNotOpening.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void OnInteract(InteractionPart interaction)
	{
		if (interaction is InteractionDoorPart)
		{
			IsDone = true;
		}
	}

	public void OnInteractionRestricted(InteractionPart interaction)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
