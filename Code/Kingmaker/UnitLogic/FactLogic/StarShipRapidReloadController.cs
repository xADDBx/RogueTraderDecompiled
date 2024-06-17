using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("8d6d441ffcdf3e24a88daf5a16f31586")]
public class StarShipRapidReloadController : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	[SerializeField]
	private StarshipWeaponType m_weaponType;

	[SerializeField]
	private StarshipWeaponType m_weaponPenaltedType;

	private StarShipUnitPartRapidReload RapidReloadStore => base.Owner.GetOptional<StarShipUnitPartRapidReload>();

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (evt.Initiator == base.Context?.MaybeOwner && ((evt.Initiator as StarshipEntity)?.Navigation?.IsAccelerationMovementPhase).GetValueOrDefault())
		{
			if (evt.Spell.StarshipWeapon?.Blueprint.WeaponType == m_weaponType)
			{
				RapidReloadStore?.AbilityActivationNotification(evt.Spell.StarshipWeapon, penalted: false);
			}
			if (evt.Spell.StarshipWeapon?.Blueprint.WeaponType == m_weaponPenaltedType)
			{
				RapidReloadStore?.AbilityActivationNotification(evt.Spell.StarshipWeapon, penalted: true);
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (base.Owner == EventInvokerExtensions.MechanicEntity)
		{
			RapidReloadStore?.ClearActivationInfo();
		}
	}

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<StarShipUnitPartRapidReload>();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<StarShipUnitPartRapidReload>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
