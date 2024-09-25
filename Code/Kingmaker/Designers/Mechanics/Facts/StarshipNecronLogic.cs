using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintStarship))]
[TypeId("73cdb42c2d277a446be7489e2676e7ad")]
public class StarshipNecronLogic : UnitFactComponentDelegate, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_LordBuff;

	[SerializeField]
	private BlueprintBuffReference m_MarkBuff;

	[SerializeField]
	private int m_LordSpawnPrefferedDistance;

	[SerializeField]
	private int m_LordEscapeMaxDistance;

	[SerializeField]
	private int m_RegenPerTurnPct;

	[SerializeField]
	private BlueprintBuffReference m_UndeathBuff;

	[SerializeField]
	private int m_UndeathDamagePerTurnPct;

	[SerializeField]
	private ActionList ActionsOnUndeathEvent;

	public BlueprintBuff LordBuff => m_LordBuff?.Get();

	public BlueprintBuff MarkBuff => m_MarkBuff?.Get();

	public int LordEscapeMaxDistance => m_LordEscapeMaxDistance;

	public BlueprintBuff UndeathBuff => m_UndeathBuff?.Get();

	public int UndeathDamagePerTurnPct => m_UndeathDamagePerTurnPct;

	public void HandleUnitJoinCombat()
	{
		if (EventInvokerExtensions.Entity != base.Owner)
		{
			return;
		}
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		StarshipEntity starshipEntity = playerShip.Buffs.Get(MarkBuff)?.MaybeContext?.MaybeCaster as StarshipEntity;
		if (starshipEntity == null || MyShipIsBetter(starshipEntity))
		{
			if (starshipEntity != null)
			{
				starshipEntity.Buffs.Remove(LordBuff);
				playerShip.Buffs.Remove(MarkBuff);
			}
			base.Owner.Buffs.Add(LordBuff, new BuffDuration(null, BuffEndCondition.CombatEnd));
			playerShip.Buffs.Add(MarkBuff, base.Owner, new BuffDuration(null, BuffEndCondition.TurnStartOrCombatEnd));
		}
		bool MyShipIsBetter(StarshipEntity lordShip)
		{
			if (base.Owner.Blueprint.Size > lordShip.Blueprint.Size)
			{
				return true;
			}
			if (base.Owner.Blueprint.Size < lordShip.Blueprint.Size)
			{
				return false;
			}
			int num = Math.Abs(WarhammerGeometryUtils.DistanceToInCells(base.Owner.Position, default(IntRect), playerShip.Position, default(IntRect)) - m_LordSpawnPrefferedDistance);
			int num2 = Math.Abs(WarhammerGeometryUtils.DistanceToInCells(lordShip.Position, default(IntRect), playerShip.Position, default(IntRect)) - m_LordSpawnPrefferedDistance);
			return num < num2;
		}
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.HPBeforeDamage > 0 && base.Owner.Health.Damage >= (int)base.Owner.Health.HitPoints && base.Owner.Buffs.GetBuff(UndeathBuff) == null)
		{
			base.Owner.Health.SetDamage(0);
			base.Owner.Buffs.Add(UndeathBuff, base.Owner, new BuffDuration(null, BuffEndCondition.CombatEnd));
			using (base.Context.GetDataScope(base.Owner.ToITargetWrapper()))
			{
				ActionsOnUndeathEvent.Run();
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (!isTurnBased || EventInvokerExtensions.MechanicEntity != base.Owner)
		{
			return;
		}
		Buff buff = base.Owner.Buffs.GetBuff(UndeathBuff);
		if (buff != null)
		{
			int num = base.Owner.Health.MaxHitPoints * m_UndeathDamagePerTurnPct / 100;
			Rulebook.Trigger(new RuleDealDamage(buff.MaybeContext?.MaybeCaster, base.Owner, new DamageData(DamageType.Direct, num, num)));
			return;
		}
		int num2 = base.Owner.Health.Damage * m_RegenPerTurnPct / 100;
		if (num2 > 0)
		{
			Rulebook.Trigger(new RuleHealDamage(base.Owner, base.Owner, num2));
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
