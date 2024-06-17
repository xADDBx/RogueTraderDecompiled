using System;
using Kingmaker.Controllers;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartDestroyAfterDelay : AbstractUnitPart, IGameTimeChangedHandler, ISubscriber, IHashable
{
	[JsonProperty]
	public float Delay { get; private set; }

	public void Setup(float delay)
	{
		Delay = delay;
	}

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		Delay -= (float)delta.TotalSeconds;
		if (!(Delay > 0f))
		{
			if ((bool)base.Owner.GetOptional<UnitPartSummonedMonster>())
			{
				Game.Instance.EntityDestroyer.Destroy(base.Owner);
			}
			else if (!base.Owner.IsPlayerFaction)
			{
				base.Owner.IsInGame = false;
			}
			RemoveSelf();
		}
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = Delay;
		result.Append(ref val2);
		return result;
	}
}
