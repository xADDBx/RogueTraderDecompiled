using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class WarhammerUnitPartStrangulateController : BaseUnitPart, IAreaHandler, ISubscriber, IInitiatorRulebookHandler<RuleCalculateActionPoints>, IRulebookHandler<RuleCalculateActionPoints>, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateMovementPoints>, IRulebookHandler<RuleCalculateMovementPoints>, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitCombatHandler, EntitySubscriber>, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IUnitBuffHandler, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, IHashable
{
	[JsonProperty]
	public List<Buff> Buffs = new List<Buff>();

	public int StrangulateActionCost = 3;

	public int StrangulateMoveCost = 3;

	public void NewBuff(Buff newBuff)
	{
		Buffs.Add(newBuff);
	}

	public void RemoveBuff(Buff buff)
	{
		Buff buff2 = Buffs.Find((Buff p) => p.Blueprint == buff.Blueprint);
		if (buff2 != null)
		{
			Buffs.Remove(buff2);
		}
	}

	public void RemoveAllBuffs()
	{
		Buff[] array = Buffs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Remove();
		}
	}

	public bool IsStrangulating()
	{
		return Buffs.Any();
	}

	public void OnAreaBeginUnloading()
	{
		RemoveAllBuffs();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateActionPoints evt)
	{
		evt.MaxPointsBonus -= Buffs.Count * StrangulateActionCost;
	}

	public void OnEventDidTrigger(RuleCalculateActionPoints evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateMovementPoints evt)
	{
		evt.Bonus -= Buffs.Count * StrangulateMoveCost;
	}

	public void OnEventDidTrigger(RuleCalculateMovementPoints evt)
	{
	}

	public void HandleUnitJoinCombat()
	{
	}

	public void HandleUnitLeaveCombat()
	{
		RemoveAllBuffs();
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit == base.Owner)
		{
			RemoveAllBuffs();
		}
		Buffs.RemoveAll((Buff p) => p.Owner == unit);
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit == base.Owner)
		{
			RemoveAllBuffs();
		}
		Buffs.RemoveAll((Buff p) => p.Owner == unit);
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		if (base.Owner.State.IsHelpless || !base.Owner.State.IsAble || !base.Owner.State.CanAct)
		{
			RemoveAllBuffs();
		}
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		if (base.Owner.State.IsHelpless || !base.Owner.State.IsAble || !base.Owner.State.CanAct)
		{
			RemoveAllBuffs();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Buff> buffs = Buffs;
		if (buffs != null)
		{
			for (int i = 0; i < buffs.Count; i++)
			{
				Hash128 val2 = ClassHasher<Buff>.GetHash128(buffs[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
