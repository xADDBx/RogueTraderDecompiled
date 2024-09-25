using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("0f25735707f54eaf98dcbfd49fd4fff3")]
public class PlayerNotMovingTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, IHashable
{
	private TimeSpan m_TimeSinceNotMoving = TimeSpan.Zero;

	private static BaseUnitEntity Player => GameHelper.GetPlayerCharacter();

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentMode != GameModeType.Default))
		{
			if (!Player.MovementAgent.IsReallyMoving)
			{
				m_TimeSinceNotMoving += delta;
			}
			if (m_TimeSinceNotMoving.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (unit.IsInPlayerParty)
		{
			IsDone = true;
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
