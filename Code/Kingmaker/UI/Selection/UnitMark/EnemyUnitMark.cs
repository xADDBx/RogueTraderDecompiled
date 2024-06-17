using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class EnemyUnitMark : BaseSurfaceUnitMark, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCommandEndHandler, ICellAbilityHandler, IAbilityTargetSelectionUIHandler, INetPingEntity
{
	[Header("Exploration")]
	[SerializeField]
	private UnitMarkDecal m_ExplorationSelectedDecal;

	[Header("Combat")]
	[SerializeField]
	private UnitMarkDecal m_CombatDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatIsInAoeDecal;

	[Header("Coop")]
	[SerializeField]
	private UnitMarkDecal m_PingTarget;

	private Tween m_PingTween;

	public override void Initialize(AbstractUnitEntity unit)
	{
		base.Initialize(unit);
		SetUnitSize(unit.SizeRect.Width > 1);
		m_PingTarget?.SetActive(state: false);
	}

	protected override List<UnitMarkDecal> GetAllDecals()
	{
		return new List<UnitMarkDecal> { m_ExplorationSelectedDecal, m_CombatDecal, m_CombatIsInAoeDecal, m_CombatSelectedDecal };
	}

	public override void HandleStateChanged()
	{
		if (base.Unit != null)
		{
			bool flag = base.State.HasFlag(UnitMarkState.IsInCombat);
			bool flag2 = base.State.HasFlag(UnitMarkState.CurrentTurn);
			m_ExplorationSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && base.State.HasFlag(UnitMarkState.CastingSpell));
			m_CombatDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && !flag2);
			m_CombatSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag2);
			m_CombatIsInAoeDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && base.State.HasFlag(UnitMarkState.IsInAoEPattern));
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		SetUnitCastingSpellState(command, active: true);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		SetUnitCastingSpellState(command, active: false);
	}

	private void SetUnitCastingSpellState(AbstractUnitCommand command, bool active)
	{
		if (command is UnitUseAbility unitUseAbility && unitUseAbility.TargetUnit == base.Unit)
		{
			SetState(UnitMarkState.CastingSpell, active);
		}
	}

	public void HandleCellAbility(List<AbilityTargetUIData> abilityTargets)
	{
		bool active = abilityTargets.Count != 0 && abilityTargets.Any((AbilityTargetUIData n) => n.Target == base.Unit);
		SetState(UnitMarkState.IsInAoEPattern, active);
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		SetState(UnitMarkState.IsInAoEPattern, active: false);
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		SetState(UnitMarkState.IsInAoEPattern, active: false);
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity == base.Unit)
		{
			m_PingTween?.Kill();
			m_PingTarget?.SetActive(state: true);
			m_PingTween = DOTween.To(() => 1f, delegate
			{
			}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				m_PingTarget?.SetActive(state: false);
				m_PingTween = null;
			});
		}
	}
}
