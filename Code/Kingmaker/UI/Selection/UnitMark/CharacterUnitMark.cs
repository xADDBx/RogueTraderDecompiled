using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class CharacterUnitMark : BaseSurfaceUnitMark, INetRoleSetHandler, ISubscriber, INetStopPlayingHandler, INetPingEntity
{
	[Header("Exploration")]
	[SerializeField]
	private UnitMarkDecal m_ExplorationSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_ExplorationOtherPlayerDecal;

	[SerializeField]
	private UnitMarkDecal m_ExplorationDialogCurrentSpeakerDecal;

	[Header("Combat")]
	[Header("Companions")]
	[SerializeField]
	private UnitMarkDecal m_CombatDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatCurrentTurnDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatAbilityTargetDecal;

	[Header("Other Player")]
	[SerializeField]
	private UnitMarkDecal m_CombatOtherPlayerDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedOtherPlayerDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatCurrentTurnOtherPlayerDecal;

	[Header("GamePad")]
	[SerializeField]
	private UnitMarkDecal m_GamepadSelectedDecal;

	[Header("Coop")]
	[SerializeField]
	private UnitMarkDecal m_PingTarget;

	private Tween m_PingTween;

	protected override List<UnitMarkDecal> GetAllDecals()
	{
		return new List<UnitMarkDecal>
		{
			m_ExplorationSelectedDecal, m_ExplorationOtherPlayerDecal, m_ExplorationDialogCurrentSpeakerDecal, m_ExplorationDialogCurrentSpeakerDecal, m_CombatDecal, m_CombatSelectedDecal, m_CombatCurrentTurnDecal, m_CombatOtherPlayerDecal, m_CombatSelectedOtherPlayerDecal, m_CombatCurrentTurnOtherPlayerDecal,
			m_GamepadSelectedDecal
		};
	}

	protected override UnitMarkDecal GetAbilityTargetDecal()
	{
		return m_CombatAbilityTargetDecal;
	}

	public override void Initialize(AbstractUnitEntity unit)
	{
		base.Initialize(unit);
		SetUnitSize(unit.SizeRect.Width > 1);
		bool isSelected = Game.Instance.SelectionCharacter.SelectedUnits.HasItem((BaseUnitEntity i) => i == unit);
		Selected(isSelected);
		m_PingTarget?.SetActive(state: false);
	}

	public override void HandleStateChanged()
	{
		if (base.Unit != null)
		{
			bool flag = base.State.HasFlag(UnitMarkState.CurrentTurn);
			bool flag2 = base.State.HasFlag(UnitMarkState.DialogCurrentSpeaker);
			bool flag3 = base.State.HasFlag(UnitMarkState.Selected);
			bool flag4 = base.State.HasFlag(UnitMarkState.IsInCombat);
			bool isDirectlyControllable = base.Unit.IsDirectlyControllable;
			bool flag5 = base.State.HasFlag(UnitMarkState.GamepadSelected);
			bool flag6 = base.Unit.IsMyNetRole();
			bool inLobbyAndPlaying = UINetUtility.InLobbyAndPlaying;
			m_ExplorationOtherPlayerDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && !flag2 && !flag4 && !flag6 && inLobbyAndPlaying);
			m_ExplorationSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && !flag && !flag2 && flag3 && !flag4 && isDirectlyControllable);
			m_ExplorationDialogCurrentSpeakerDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag2);
			m_CombatDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag4 && !flag && flag6);
			m_CombatSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag4 && flag3 && flag && flag6);
			m_CombatCurrentTurnDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && flag6);
			m_CombatOtherPlayerDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag4 && !flag && !flag6 && inLobbyAndPlaying);
			m_CombatSelectedOtherPlayerDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag4 && flag && !flag6 && Game.Instance.TurnController?.CurrentUnit == base.Unit && inLobbyAndPlaying);
			m_CombatCurrentTurnOtherPlayerDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && !flag6 && inLobbyAndPlaying);
			m_GamepadSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag5);
		}
	}

	public override void SetGamepadSelected(bool selected)
	{
		SetState(UnitMarkState.GamepadSelected, selected);
	}

	public void HandleRoleSet(string entityId)
	{
		if (base.Unit != null && base.Unit.UniqueId == entityId)
		{
			HandleStateChanged();
		}
	}

	void INetStopPlayingHandler.HandleStopPlaying()
	{
		HandleStateChanged();
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
