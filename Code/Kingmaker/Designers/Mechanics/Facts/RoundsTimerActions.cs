using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowMultipleComponents]
[TypeId("904cdbeb5bf84eaf89247b99658a8b3b")]
public class RoundsTimerActions : EntityFactComponentDelegate, IRoundStartHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int RoundsPassed;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref RoundsPassed);
			return result;
		}
	}

	private enum TickPolicy
	{
		[UsedImplicitly]
		Default,
		TurnBasedOnly,
		RealtimeOnly
	}

	[SerializeField]
	private TickPolicy m_TickPolicy;

	[InfoBox("Wait Delay rounds before run Actions (0 means trigger on first New Round event)")]
	public int Delay;

	[InfoBox("Run Actions every Loop rounds after Delay rounds (0 means never repeat)")]
	public int Loop;

	public ActionList Actions;

	public bool CanTickInTurnBased => m_TickPolicy != TickPolicy.RealtimeOnly;

	public bool CanTickRealtime => m_TickPolicy != TickPolicy.TurnBasedOnly;

	private bool CanTick(bool isTurnBased)
	{
		if (!isTurnBased || !CanTickInTurnBased)
		{
			if (!isTurnBased)
			{
				return CanTickRealtime;
			}
			return false;
		}
		return true;
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		if (CanTick(isTurnBased) && (!(base.Owner is MechanicEntity mechanicEntity) || mechanicEntity.Initiative.Empty))
		{
			NextRound();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (CanTick(isTurnBased) && base.Owner is MechanicEntity mechanicEntity && !mechanicEntity.Initiative.Empty && base.Owner == EventInvokerExtensions.MechanicEntity)
		{
			NextRound();
		}
	}

	private void NextRound()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		savableData.RoundsPassed++;
		if (savableData.RoundsPassed == Delay + 1 || (savableData.RoundsPassed > Delay && Loop > 0 && (savableData.RoundsPassed - Delay - 1) % Loop == 0))
		{
			using (base.Fact.MaybeContext?.GetDataScope())
			{
				Actions.Run();
			}
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
