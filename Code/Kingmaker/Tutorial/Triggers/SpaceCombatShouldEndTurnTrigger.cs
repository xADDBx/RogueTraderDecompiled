using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("6578ba4178eb4af8af86109b79cd9344")]
public class SpaceCombatShouldEndTurnTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, ITurnEndHandler, ISubscriber<IMechanicEntity>, IHashable
{
	private TimeSpan m_TimeSinceNoAction = TimeSpan.Zero;

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentMode != GameModeType.SpaceCombat))
		{
			m_TimeSinceNoAction += delta;
			if (m_TimeSinceNoAction.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		IsDone = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
