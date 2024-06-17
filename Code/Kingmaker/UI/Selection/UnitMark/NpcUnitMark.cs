using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
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

	[Header("Coop")]
	[SerializeField]
	private UnitMarkDecal m_PingTarget;

	private Tween m_PingTween;

	protected override List<UnitMarkDecal> GetAllDecals()
	{
		return new List<UnitMarkDecal> { m_DialogCurrentSpeakerDecal, m_CombatDecal, m_CombatSelectedDecal, m_CombatIsInAoeDecal };
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
