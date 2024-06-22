using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;

namespace Kingmaker.Controllers.Units;

public class MomentumController : IControllerEnable, IController, IControllerDisable, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IInterruptTurnStartHandler, ISubscriber<IMechanicEntity>, ITurnStartHandler, IUnitWoundHandler, IUnitTraumaHandler, IRoundStartHandler
{
	private HashSet<MechanicEntity> m_GainedMomentumThisTurn = new HashSet<MechanicEntity>();

	public TurnController TurnController { get; set; }

	public List<MomentumGroup> Groups => Game.Instance.Player.GetOrCreate<TurnDataPart>()?.MomentumGroups;

	public void OnEnable()
	{
		foreach (MomentumGroup group in Groups)
		{
			group.Units.RemoveAll((EntityRef<MechanicEntity> x) => x.Entity == null);
		}
	}

	public void OnDisable()
	{
	}

	[CanBeNull]
	public MomentumGroup GetGroup(MechanicEntity entity)
	{
		return Groups.FirstItem((MomentumGroup i) => i.Units.Contains(entity));
	}

	public void InitializeGroups()
	{
		BlueprintMomentumRoot settings = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		if (!Groups.Contains((MomentumGroup p) => p.Blueprint == settings.PartyGroup))
		{
			Groups.Add(new MomentumGroup(settings.PartyGroup));
		}
		if (!Groups.Contains((MomentumGroup p) => p.Blueprint == settings.DefaultEnemyGroup))
		{
			Groups.Add(new MomentumGroup(settings.DefaultEnemyGroup));
		}
		foreach (MomentumGroup group in Groups)
		{
			group.ResetMomentumToStartingValue();
		}
	}

	public void ClearGroups()
	{
		Groups.Clear();
	}

	void IUnitCombatHandler.HandleUnitJoinCombat()
	{
		HandleUnitJoinCombat(EventInvokerExtensions.BaseUnitEntity);
	}

	void IUnitCombatHandler.HandleUnitLeaveCombat()
	{
		HandleUnitLeaveCombat(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleUnitJoinCombat(BaseUnitEntity unit)
	{
		HandleUnitJoinCombatOrBecameConscious(unit);
	}

	public void HandleUnitLeaveCombat(BaseUnitEntity unit)
	{
		HandleUnitLeaveCombatOrBecameUnconscious(unit);
	}

	void IUnitWoundHandler.HandleWoundReceived()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity is UnitEntity && baseUnitEntity.IsInCombat)
		{
			Rulebook.Trigger(RulePerformMomentumChange.CreateWound(baseUnitEntity));
		}
	}

	public void HandleWoundAvoided()
	{
	}

	void IUnitTraumaHandler.HandleTraumaReceived()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity is UnitEntity && baseUnitEntity.IsInCombat)
		{
			Rulebook.Trigger(RulePerformMomentumChange.CreateTrauma(baseUnitEntity));
		}
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			if (prevLifeState != 0 && baseUnitEntity.LifeState.State == UnitLifeState.Conscious)
			{
				HandleUnitJoinCombatOrBecameConscious(baseUnitEntity);
			}
			if (prevLifeState == UnitLifeState.Conscious && baseUnitEntity.LifeState.State != 0)
			{
				HandleUnitLeaveCombatOrBecameUnconscious(baseUnitEntity);
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		BlueprintMomentumRoot momentumRoot = Root.WH.MomentumRoot;
		if (!isTurnBased || Game.Instance.CurrentMode == GameModeType.SpaceCombat || mechanicEntity.Facts.Contains(momentumRoot.HeroicActBuff) || m_GainedMomentumThisTurn.Contains(mechanicEntity))
		{
			return;
		}
		RulePerformMomentumChange rulePerformMomentumChange = Rulebook.Trigger(RulePerformMomentumChange.CreateStartTurn(mechanicEntity));
		if (rulePerformMomentumChange.ResultGroup != null && mechanicEntity.IsPlayerFaction)
		{
			if (rulePerformMomentumChange.ResultCurrentValue >= momentumRoot.HeroicActThreshold)
			{
				FxHelper.SpawnFxOnEntity(momentumRoot.HeroicActReachedFX.Load(), mechanicEntity.View);
			}
			if (rulePerformMomentumChange.ResultCurrentValue <= mechanicEntity.GetDesperateMeasureThreshold())
			{
				FxHelper.SpawnFxOnEntity(momentumRoot.DesperateMeasuresReachedFX.Load(), mechanicEntity.View);
			}
			m_GainedMomentumThisTurn.Add(mechanicEntity);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (interruptionData.AsExtraTurn)
		{
			HandleUnitStartTurn(isTurnBased: true);
		}
	}

	private void HandleUnitJoinCombatOrBecameConscious(BaseUnitEntity unit)
	{
		BlueprintMomentumRoot momentumRoot = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		SeparateMomentumGroup component = unit.Blueprint.GetComponent<SeparateMomentumGroup>();
		BlueprintMomentumGroup momentumGroupBlueprint = ((component != null) ? component.MomentumGroup : (unit.IsPlayerFaction ? momentumRoot.PartyGroup : momentumRoot.DefaultEnemyGroup));
		MomentumGroup momentumGroup = Groups.FirstItem((MomentumGroup p) => p.Blueprint == momentumGroupBlueprint);
		if (momentumGroup == null)
		{
			momentumGroup = new MomentumGroup(momentumGroupBlueprint);
			Groups.Add(momentumGroup);
		}
		if (!momentumGroup.Units.Contains(unit))
		{
			momentumGroup.Units.Add(unit);
		}
	}

	private void HandleUnitLeaveCombatOrBecameUnconscious(BaseUnitEntity unit)
	{
		MomentumGroup momentumGroup = Groups.FirstItem((MomentumGroup i) => i.Units.Contains(unit));
		if (momentumGroup == null)
		{
			return;
		}
		if (!unit.LifeState.IsConscious && !(unit is StarshipEntity))
		{
			Rulebook.Trigger(RulePerformMomentumChange.CreateBecameDeadOrUnconscious(unit));
		}
		momentumGroup.Units.Remove(unit);
		BlueprintMomentumRoot root = Game.Instance.BlueprintRoot.WarhammerRoot.MomentumRoot;
		MechanicEntity mechanicEntity = unit.GetHealthOptional()?.LastHandledDamage?.ConcreteInitiator;
		if (mechanicEntity == null || unit.LifeState.IsConscious || unit is StarshipEntity)
		{
			return;
		}
		Func<MomentumGroup, bool> predicate = (unit.IsPlayerFaction ? ((Func<MomentumGroup, bool>)((MomentumGroup p) => p.Blueprint != root.PartyGroup)) : ((Func<MomentumGroup, bool>)((MomentumGroup p) => p.Blueprint == root.PartyGroup)));
		foreach (MomentumGroup item in Groups.Where(predicate))
		{
			Rulebook.Trigger(RulePerformMomentumChange.CreateKillEnemy(mechanicEntity, unit, item));
		}
	}

	[Cheat(Name = "change_momentum_by", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ChangeMomentumBy(int delta = 100, BaseUnitEntity unit = null)
	{
		if (unit == null)
		{
			unit = Utilities.GetUnitUnderMouse() ?? Game.Instance.Player.MainCharacterEntity;
		}
		BaseUnitEntity entity = unit;
		RuleReason reason = default(RuleReason);
		Rulebook.Trigger(RulePerformMomentumChange.CreateCustom(entity, delta, in reason));
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		if (isTurnBased)
		{
			m_GainedMomentumThisTurn.Clear();
		}
	}
}
