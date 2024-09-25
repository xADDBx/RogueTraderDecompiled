using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View;

namespace Kingmaker.Visual.Sound;

public class HitAsksController : IUnitAsksController, IDisposable, IWarhammerAttackHandler, ISubscriber
{
	public HitAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void IWarhammerAttackHandler.HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (withWeaponAttackHit.ResultIsHit && withWeaponAttackHit.ResultDamageRule != null && withWeaponAttackHit.TargetUnit != null)
		{
			if (withWeaponAttackHit.TargetUnit.IsAlly(withWeaponAttackHit.Initiator))
			{
				withWeaponAttackHit.TargetUnit?.View.Asks?.FriendlyFire?.Schedule();
			}
			else if (withWeaponAttackHit.TargetUnit.IsEnemy(withWeaponAttackHit.Initiator) && (withWeaponAttackHit.ResultDamageRule.ResultIsCritical || withWeaponAttackHit.RollPerformAttackRule.ResultIsRighteousFury) && withWeaponAttackHit.ConcreteInitiator.View is UnitEntityView unitEntityView)
			{
				unitEntityView.Asks?.CriticalHit?.Schedule();
			}
		}
	}
}
