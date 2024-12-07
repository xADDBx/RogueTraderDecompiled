using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("4e725fc35ebe4a38b0e1fc37c60232b3")]
public class StrategistKeystoneRearAbilityFlipPattern : UnitFactComponentDelegate, IClickMechanicActionBarSlotHandler, ISubscriber, IFlipZoneAbilityHandler, IHashable
{
	protected override void OnActivate()
	{
		base.OnActivate();
		base.Owner.GetOrCreate<UnitPartStrategistKeystoneRearAbilityFlipPattern>();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitPartStrategistKeystoneRearAbilityFlipPattern>();
		base.OnDeactivate();
	}

	void IClickMechanicActionBarSlotHandler.HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		if (ability is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.Ability.Caster == base.Owner && mechanicActionBarSlotAbility.Ability.Blueprint.HasLogic<IsFlipZoneAbility>())
		{
			base.Owner.GetOptional<UnitPartStrategistKeystoneRearAbilityFlipPattern>()?.Reset();
		}
	}

	void IFlipZoneAbilityHandler.HandleFlipZoneAbility()
	{
		base.Owner.GetOptional<UnitPartStrategistKeystoneRearAbilityFlipPattern>()?.Flip();
		EventBus.RaiseEvent(delegate(IFlippedZoneAbilityHandler h)
		{
			h.HandleFlippedZoneAbility();
		});
		PFLog.Default.Log("IFlipZoneAbilityHandler.HandleFlipZoneAbility");
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
