using System;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartFadeOutAndDestroy : EntityPart, IGameTimeChangedHandler, ISubscriber, IHashable
{
	private static LogChannel Logger => EntityDestructionController.Logger;

	[JsonProperty]
	public TimeSpan DestroyTime { get; private set; }

	public void Setup(float destroyDelay)
	{
		base.ConcreteOwner.WillBeDestroyed = true;
		DestroyTime = Game.Instance.TimeController.GameTime + destroyDelay.Seconds();
		EventBus.RaiseEvent(base.Owner, delegate(IFadeOutAndDestroyHandler h)
		{
			h.HandleFadeOutAndDestroy();
		});
		Logger.Log($"Fade out and destroy {base.ConcreteOwner}. Destroy time {DestroyTime}");
	}

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		CheckReadyToDestroy();
	}

	private void CheckReadyToDestroy()
	{
		if (!(Game.Instance.Player.GameTime < DestroyTime))
		{
			Game.Instance.EntityDestroyer.Destroy(base.ConcreteOwner);
		}
	}

	public void HandleNonGameTimeChanged()
	{
		DestroyTime -= Game.Instance.TimeController.DeltaTimeSpan;
		CheckReadyToDestroy();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		TimeSpan val2 = DestroyTime;
		result.Append(ref val2);
		return result;
	}
}
