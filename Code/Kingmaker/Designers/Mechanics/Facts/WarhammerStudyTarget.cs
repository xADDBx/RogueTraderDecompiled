using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("534eaad0a89741a08b759701680f1a79")]
public class WarhammerStudyTarget : UnitBuffComponentDelegate, ITurnBasedModeHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_StudiedBuff;

	public bool Permanent;

	public ContextDurationValue DurationValue;

	public BlueprintBuff StudiedBuff => m_StudiedBuff?.Get();

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			base.Buff.Remove();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = base.Context?.MaybeCaster;
		MechanicEntity mechanicEntity2 = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null && mechanicEntity2 == mechanicEntity && (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(mechanicEntity, base.Owner) != LosCalculations.CoverType.Invisible)
		{
			MechanicsContext context = base.Context;
			Rounds? rounds = (Permanent ? null : new Rounds?(DurationValue.Calculate(context)));
			base.Owner.Buffs.Add(StudiedBuff, context, rounds);
			base.Buff.Remove();
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
