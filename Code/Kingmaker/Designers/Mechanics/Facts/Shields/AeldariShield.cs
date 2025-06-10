using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Shields;

[TypeId("94808ee246db46a8b1b3d6e6f32d037d")]
public class AeldariShield : UnitFactComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, ITurnBasedModeHandler, IHashable
{
	[SerializeField]
	[Tooltip("Базовый шанс блока")]
	private int m_BlockChance;

	[SerializeField]
	[Tooltip("Количество зарядов")]
	private int m_NumberOfCharges = 1;

	[SerializeField]
	[Tooltip("Кулдаун перезарядки зарядов в раундах")]
	private int m_ResetChargeCooldownInRaunds = 1;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<UnitPartAeldariShields>()?.Add(base.Fact, m_NumberOfCharges, m_BlockChance, m_ResetChargeCooldownInRaunds);
		base.OnActivate();
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartAeldariShields>()?.Add(base.Fact, m_NumberOfCharges, m_BlockChance, m_ResetChargeCooldownInRaunds);
		base.OnActivateOrPostLoad();
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartAeldariShields>()?.Remove(base.Fact);
		base.OnDeactivate();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.BaseUnitEntity == base.Owner && base.Owner.IsInCombat && !base.Owner.IsDeadOrUnconscious)
		{
			base.Owner.GetOptional<UnitPartAeldariShields>()?.Get(base.Fact)?.ResetNumberOfCharges(Game.Instance.TurnController.CombatRound);
		}
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			base.Owner.GetOptional<UnitPartAeldariShields>()?.Get(base.Fact)?.ResetNumberOfCharges(int.MaxValue);
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
