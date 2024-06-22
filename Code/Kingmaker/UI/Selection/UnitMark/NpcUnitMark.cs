using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class NpcUnitMark : BaseSurfaceUnitMark, INetPingEntity, ISubscriber
{
	[SerializeField]
	private UnitMarkDecal m_DialogCurrentSpeakerDecal;

	[Header("Combat")]
	[SerializeField]
	private UnitMarkDecal m_CombatDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatIsInAoeDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatAbilityTargetDecal;

	[Header("Coop")]
	[SerializeField]
	private UnitMarkDecal m_PingTarget;

	private Tween m_PingTween;

	protected override List<UnitMarkDecal> GetAllDecals()
	{
		return new List<UnitMarkDecal> { m_DialogCurrentSpeakerDecal, m_CombatDecal, m_CombatSelectedDecal, m_CombatIsInAoeDecal };
	}

	protected override UnitMarkDecal GetAbilityTargetDecal()
	{
		return m_CombatAbilityTargetDecal;
	}

	public override void Initialize(AbstractUnitEntity unit)
	{
		base.Initialize(unit);
		SetUnitSize(unit.SizeRect.Width > 1);
		m_PingTarget?.SetActive(state: false);
	}

	public override void HandleStateChanged()
	{
		if (base.Unit != null)
		{
			bool flag = base.State.HasFlag(UnitMarkState.IsInCombat);
			bool flag2 = base.State.HasFlag(UnitMarkState.CurrentTurn);
			m_DialogCurrentSpeakerDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && base.State.HasFlag(UnitMarkState.DialogCurrentSpeaker));
			m_CombatDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && !flag2);
			m_CombatSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag2);
			m_CombatIsInAoeDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && base.State.HasFlag(UnitMarkState.IsInAoEPattern));
		}
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity != base.Unit)
		{
			return;
		}
		m_PingTween?.Kill();
		int index = player.Index - 1;
		m_PingTarget.SetMaterial(BlueprintRoot.Instance.UIConfig.CoopPlayersPingsMaterials[index]);
		m_PingTarget?.SetActive(state: true);
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingEntityMarker(entity);
		});
		m_PingTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_PingTarget?.SetActive(state: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingEntityMarker(entity);
			});
			m_PingTween = null;
		})
			.OnKill(delegate
			{
				m_PingTarget?.SetActive(state: false);
				EventBus.RaiseEvent(delegate(INetAddPingMarker h)
				{
					h.HandleRemovePingEntityMarker(entity);
				});
				m_PingTween = null;
			});
	}
}
