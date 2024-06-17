using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Controllers;

public class HealthController : IControllerTick, IController, IControllerStop, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntityJoinTBCombat
{
	private bool m_HealPartyAfterCombat;

	public void OnStop()
	{
		m_HealPartyAfterCombat = false;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		TryHealPartyAfterCombat();
	}

	private void TryHealPartyAfterCombat()
	{
		if (!m_HealPartyAfterCombat)
		{
			return;
		}
		m_HealPartyAfterCombat = false;
		BaseUnitEntity unitWithBestMedicae = GetUnitWithBestMedicae(onlyConscious: false);
		BaseUnitEntity baseUnitEntity = GetUnitWithBestMedicae(onlyConscious: true);
		if (unitWithBestMedicae == null || baseUnitEntity == null)
		{
			return;
		}
		if (unitWithBestMedicae != baseUnitEntity && (int)baseUnitEntity.Skills.SkillMedicae <= (int)unitWithBestMedicae.Skills.SkillMedicae)
		{
			Rulebook.Trigger(new RulePerformMedicaeHeal(baseUnitEntity, unitWithBestMedicae, 0));
			if (unitWithBestMedicae.LifeState.IsConscious)
			{
				baseUnitEntity = unitWithBestMedicae;
			}
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			int num = 0;
			while (!(item is StarshipEntity) && !item.IsDead && num < item.Health.Damage)
			{
				num = item.Health.Damage;
				Rulebook.Trigger(new RulePerformMedicaeHeal(baseUnitEntity, item, 0));
			}
		}
	}

	[CanBeNull]
	private static BaseUnitEntity GetUnitWithBestMedicae(bool onlyConscious)
	{
		BaseUnitEntity baseUnitEntity = null;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (!item.IsStarship() && !(!item.LifeState.IsConscious && onlyConscious) && (baseUnitEntity == null || (int)item.Skills.SkillMedicae > (int)baseUnitEntity.Skills.SkillMedicae))
			{
				baseUnitEntity = item;
			}
		}
		return baseUnitEntity;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (Game.Instance.IsSpaceCombat)
		{
			return;
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		PartHealth partHealth = mechanicEntity?.GetHealthOptional();
		if (partHealth != null)
		{
			partHealth.UpdateWoundsAndTraumasOnNewTurn(isTurnBased);
			if (!TurnController.IsInTurnBasedCombat() && mechanicEntity.IsInPlayerParty && !mechanicEntity.Features.DoNotReviveOutOfCombat)
			{
				partHealth.HealDamageAll();
			}
		}
	}

	public void HandleEntityJoinTBCombat()
	{
		if (!Game.Instance.IsSpaceCombat)
		{
			(EventInvokerExtensions.MechanicEntity?.GetHealthOptional())?.UpdateWoundsAndTraumasOnNewTurn(isTurnBased: true);
		}
	}
}
