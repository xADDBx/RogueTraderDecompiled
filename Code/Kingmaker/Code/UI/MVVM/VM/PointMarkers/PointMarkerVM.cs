using System;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.PointMarkers;

public class PointMarkerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<Sprite> Portrait = new ReactiveProperty<Sprite>();

	public readonly bool UsedSubtypeIcons;

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<UnitRelation> Relation = new ReactiveProperty<UnitRelation>();

	public readonly ReactiveProperty<EntityPointMarkObjectType> AnotherPointMarkObjectType = new ReactiveProperty<EntityPointMarkObjectType>();

	private Vector3 m_PositionInUI = Vector3.zero;

	public readonly BaseUnitEntity Unit;

	public readonly Entity AnotherEntity;

	public readonly GameObject PingPosition;

	public readonly ReactiveProperty<LineOfSightVM> LineOfSight = new ReactiveProperty<LineOfSightVM>();

	public Vector3 Position => Unit?.Position ?? AnotherEntity?.Position ?? PingPosition.transform.position;

	public PointMarkerVM(BaseUnitEntity unitEntity)
	{
		Unit = unitEntity;
		Portrait.Value = UIUtilityUnit.GetSurfaceCombatStandardPortrait(unitEntity, UIUtilityUnit.PortraitCombatSize.Icon);
		Relation.Value = GetUnitRelation(unitEntity);
		UsedSubtypeIcons = UIUtilityUnit.UsedSubtypeIcon(unitEntity);
	}

	public PointMarkerVM(Entity anotherEntity, bool isPing = false)
	{
		AnotherEntity = anotherEntity;
		AnotherPointMarkObjectType.Value = GetEntityPointMarkObjectType(anotherEntity, null, isPing);
		UsedSubtypeIcons = isPing;
	}

	public PointMarkerVM(GameObject pingPosition)
	{
		PingPosition = pingPosition;
		AnotherPointMarkObjectType.Value = GetEntityPointMarkObjectType(null, pingPosition, isPing: true);
		UsedSubtypeIcons = true;
	}

	protected override void DisposeImplementation()
	{
	}

	public void Update()
	{
		UpdateVisibility();
	}

	public void ScrollToUnit()
	{
		if (AnotherEntity == null && PingPosition == null)
		{
			Game.Instance.CameraController?.Follower?.Release();
		}
		CameraRig.Instance.ScrollTo(Position);
	}

	private void UpdateVisibility()
	{
		if ((Game.Instance.CurrentMode != GameModeType.Default && Game.Instance.CurrentMode != GameModeType.SpaceCombat && Game.Instance.CurrentMode != GameModeType.GlobalMap && Game.Instance.CurrentMode != GameModeType.Pause) || Game.GetCamera() == null)
		{
			IsVisible.Value = false;
			return;
		}
		m_PositionInUI = Game.GetCamera().WorldToScreenPoint(Position);
		IsVisible.Value = m_PositionInUI.x <= 0f || m_PositionInUI.x >= (float)Game.GetCamera().pixelWidth || m_PositionInUI.y <= 0f || m_PositionInUI.y >= (float)Game.GetCamera().pixelHeight;
	}

	private UnitRelation GetUnitRelation(BaseUnitEntity unitEntity)
	{
		if (unitEntity.Faction.IsPlayer)
		{
			return UnitRelation.Self;
		}
		if (unitEntity.Faction.Neutral)
		{
			return UnitRelation.Neutral;
		}
		if (unitEntity.Faction.IsPlayerEnemy)
		{
			return UnitRelation.Enemy;
		}
		return UnitRelation.Ally;
	}

	private EntityPointMarkObjectType GetEntityPointMarkObjectType(Entity entity, GameObject pingPosition, bool isPing)
	{
		if (isPing)
		{
			if (!(pingPosition != null))
			{
				return EntityPointMarkObjectType.PingEntity;
			}
			return EntityPointMarkObjectType.PingPosition;
		}
		if (entity is SectorMapObjectEntity)
		{
			return EntityPointMarkObjectType.Quest;
		}
		return EntityPointMarkObjectType.Quest;
	}
}
