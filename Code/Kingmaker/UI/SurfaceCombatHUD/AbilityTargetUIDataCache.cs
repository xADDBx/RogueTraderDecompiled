using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public class AbilityTargetUIDataCache : MonoBehaviour, IAbilityTargetSelectionUIHandler, ISubscriber, IVirtualPositionUIHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler
{
	private readonly Dictionary<(AbilityData ability, MechanicEntity target, Vector3 casterPosition), AbilityTargetUIData> m_UIDataCache = new Dictionary<(AbilityData, MechanicEntity, Vector3), AbilityTargetUIData>();

	public static AbilityTargetUIDataCache Instance { get; private set; }

	private void OnEnable()
	{
		Instance = this;
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		Instance = null;
		Clear();
		EventBus.Unsubscribe(this);
	}

	private void Clear()
	{
		m_UIDataCache.Clear();
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		Clear();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}

	public void HandleVirtualPositionChanged(Vector3? position)
	{
		Clear();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		Clear();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		Clear();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		Clear();
	}

	public AbilityTargetUIData GetOrCreate(AbilityData ability, MechanicEntity target, Vector3 casterPosition)
	{
		bool flag = HasDynamicDamageBuff(ability.Caster);
		if (!m_UIDataCache.TryGetValue((ability, target, casterPosition), out var value))
		{
			flag = false;
			value = new AbilityTargetUIData(ability, target, casterPosition);
			m_UIDataCache.Add((ability, target, casterPosition), value);
		}
		if (flag)
		{
			value.UpdateDamage();
		}
		return value;
	}

	public void AddOrReplace(AbilityTargetUIData uiData)
	{
		m_UIDataCache[(uiData.Ability, uiData.Target, uiData.CasterPosition)] = uiData;
	}

	private static bool HasDynamicDamageBuff(MechanicEntity caster)
	{
		foreach (Buff buff in caster.Buffs)
		{
			if (buff.Blueprint.DynamicDamage)
			{
				return true;
			}
		}
		return false;
	}
}
