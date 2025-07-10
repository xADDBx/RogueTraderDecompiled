using System;
using System.Linq;
using DG.Tweening;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Selection.UnitMark.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class StarshipUnitMark : BaseUnitMark, IUnitCommandActHandler<EntitySubscriber>, IUnitCommandActHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandActHandler, EntitySubscriber>, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, IEventTag<IUnitCommandEndHandler, EntitySubscriber>, IUnitDirectHoverUIHandler, INetPingEntity
{
	[SerializeField]
	private Transform m_Container;

	[SerializeField]
	private ShipDecalData[] m_Decals;

	[SerializeField]
	private ShipDecalConfig m_ShipDecalConfig;

	[Header("Coop")]
	[SerializeField]
	private ShipDecalData[] m_PingDecals;

	private Tween m_PingTween;

	protected ShipDecalData CurrentShipDecalData;

	protected ShipDecalData CurrentShipPingDecalData;

	private StarshipEntity m_StarshipEntity;

	protected bool IsDirect = true;

	private bool IsInSpaceCombat => Game.Instance.Player.PlayerShip.IsInCombat;

	public override void Initialize(AbstractUnitEntity unit)
	{
		base.Initialize(unit);
		if (unit is StarshipEntity starshipEntity)
		{
			m_StarshipEntity = starshipEntity;
			CurrentShipPingDecalData = GetShipDecalData(starshipEntity.GetOccupiedNodes().Count(), pingDecals: true);
			CurrentShipDecalData = GetShipDecalData(starshipEntity.GetOccupiedNodes().Count());
			CurrentShipDecalData.gameObject.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && IsInSpaceCombat);
			UpdateDirection();
			CurrentShipDecalData.SwitchShipDecal(IsDirect);
			CurrentShipPingDecalData.SwitchShipDecal(IsDirect);
			PartStarshipShields starshipShieldsOptional = unit.GetStarshipShieldsOptional();
			if (starshipShieldsOptional != null)
			{
				CurrentShipDecalData.InitializeShields(starshipShieldsOptional, m_ShipDecalConfig);
			}
			CheckStarshipShieldsVisibility();
			m_PingDecals?.ForEach(delegate(ShipDecalData d)
			{
				d.gameObject.SetActive(value: false);
			});
		}
	}

	public override void HandleStateChanged()
	{
		if (!(CurrentShipDecalData == null))
		{
			CurrentShipDecalData.gameObject.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && IsInSpaceCombat);
			bool flag = base.State.HasFlag(UnitMarkState.CurrentTurn);
			bool flag2 = base.State.HasFlag(UnitMarkState.Selected);
			bool isVisible = base.State.HasFlag(UnitMarkState.Highlighted);
			CurrentShipDecalData.SetShipDecalsSelected(flag2 && flag);
			CurrentShipDecalData.ShowShieldValues(isVisible);
			UpdateShieldsState();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		m_Container.gameObject.SetActive(value: false);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateDirection();
		UpdateCurrentDecal();
		m_Container.gameObject.SetActive(value: true);
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		if (unitEntityView.Data is StarshipEntity starshipEntity && starshipEntity == m_StarshipEntity)
		{
			CurrentShipDecalData.ShowShieldValues(isHover);
			StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
			if (m_StarshipEntity != playerShip)
			{
				CurrentShipDecalData.SetShieldsHitHighlightVisible(isHover);
			}
		}
	}

	public override void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		if (m_StarshipEntity != playerShip)
		{
			RuleStarshipCalculateHitLocation ruleStarshipCalculateHitLocation = new RuleStarshipCalculateHitLocation(playerShip, m_StarshipEntity);
			Rulebook.Trigger(ruleStarshipCalculateHitLocation);
			StarshipSectorShields shields = m_StarshipEntity.Shields.GetShields(ruleStarshipCalculateHitLocation.ResultHitLocation);
			CurrentShipDecalData.MarkHighlightShieldsHit(shields);
		}
	}

	public override void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		CurrentShipDecalData.ClearHighlightShieldsHit();
	}

	protected override void HandleUnitSizeChangedImpl()
	{
		base.transform.rotation = Quaternion.identity;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		PartStarshipShields partStarshipShields = m_StarshipEntity?.GetStarshipShieldsOptional();
		if (partStarshipShields != null)
		{
			CurrentShipDecalData.InitializeShields(partStarshipShields, m_ShipDecalConfig);
		}
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		if (CurrentShipDecalData != null)
		{
			CurrentShipDecalData?.DisposeShields();
		}
	}

	private ShipDecalData GetShipDecalData(int occupiedNodes, bool pingDecals = false)
	{
		switch (occupiedNodes)
		{
		case 1:
			return pingDecals ? GetPingShipDecalDataBySize(ShipDecalSize.Single) : GetShipDecalDataBySize(ShipDecalSize.Single);
		case 2:
			return pingDecals ? GetPingShipDecalDataBySize(ShipDecalSize.Small) : GetShipDecalDataBySize(ShipDecalSize.Small);
		case 4:
			return pingDecals ? GetPingShipDecalDataBySize(ShipDecalSize.Square2X2) : GetShipDecalDataBySize(ShipDecalSize.Square2X2);
		case 8:
		case 10:
			return pingDecals ? GetPingShipDecalDataBySize(ShipDecalSize.Medium) : GetShipDecalDataBySize(ShipDecalSize.Medium);
		case 18:
		case 24:
			return pingDecals ? GetPingShipDecalDataBySize(ShipDecalSize.Large) : GetShipDecalDataBySize(ShipDecalSize.Large);
		default:
			return pingDecals ? GetPingShipDecalDataBySize(ShipDecalSize.Single) : GetShipDecalDataBySize(ShipDecalSize.Single);
		}
	}

	private ShipDecalData GetShipDecalDataBySize(ShipDecalSize size)
	{
		ShipDecalData[] decals = m_Decals;
		foreach (ShipDecalData shipDecalData in decals)
		{
			if (shipDecalData.Size == size)
			{
				return shipDecalData;
			}
		}
		throw new IndexOutOfRangeException("ShipDecalData with specified size was not found!");
	}

	private ShipDecalData GetPingShipDecalDataBySize(ShipDecalSize size)
	{
		ShipDecalData[] pingDecals = m_PingDecals;
		foreach (ShipDecalData shipDecalData in pingDecals)
		{
			if (shipDecalData.Size == size)
			{
				return shipDecalData;
			}
		}
		throw new IndexOutOfRangeException("PingShipDecalData with specified size was not found!");
	}

	private void UpdateDirection()
	{
		int num = CustomGraphHelper.GuessDirection(m_StarshipEntity.Forward);
		IsDirect = num < 4;
	}

	private void UpdateCurrentDecal()
	{
		CurrentShipDecalData.SwitchShipDecal(IsDirect);
		CurrentShipPingDecalData.SwitchShipDecal(IsDirect);
		UpdateShieldsState();
	}

	private void UpdateShieldsState()
	{
		PartStarshipShields starshipShieldsOptional = base.Unit.GetStarshipShieldsOptional();
		bool isVisible = !BaseUnitMark.IsHideAllUI && IsInSpaceCombat && starshipShieldsOptional != null && starshipShieldsOptional.ShieldsSum > 0;
		CurrentShipDecalData.SetShieldsVisible(isVisible, IsDirect);
	}

	private void LateUpdate()
	{
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		if (m_StarshipEntity?.View != null)
		{
			m_Container.localRotation = m_StarshipEntity.View.gameObject.transform.localRotation;
		}
	}

	protected virtual void CheckStarshipShieldsVisibility()
	{
		UpdateShieldsState();
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity != base.Unit)
		{
			return;
		}
		m_PingTween?.Kill();
		ShipDecalData decal = GetShipDecalData(m_StarshipEntity.GetOccupiedNodes().Count(), pingDecals: true);
		int coopColorPingMaterial = player.Index - 1;
		decal.Or(null)?.SetCoopColorPingMaterial(coopColorPingMaterial);
		decal.Or(null)?.gameObject.SetActive(value: true);
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingEntityMarker(entity);
		});
		m_PingTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			decal.Or(null)?.gameObject.SetActive(value: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingEntityMarker(entity);
			});
			m_PingTween = null;
		})
			.OnKill(delegate
			{
				decal.Or(null)?.gameObject.SetActive(value: false);
				EventBus.RaiseEvent(delegate(INetAddPingMarker h)
				{
					h.HandleRemovePingEntityMarker(entity);
				});
				m_PingTween = null;
			});
	}
}
