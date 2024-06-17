using System;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
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

	public Vector3 UnitPositionInUI = Vector3.zero;

	public readonly BaseUnitEntity Unit;

	public ReactiveProperty<LineOfSightVM> LineOfSight = new ReactiveProperty<LineOfSightVM>();

	public Vector3 UnitPosition => Unit.Position;

	public PointMarkerVM(BaseUnitEntity unitEntity)
	{
		Unit = unitEntity;
		Portrait.Value = UIUtilityUnit.GetSurfaceCombatStandardPortrait(unitEntity, UIUtilityUnit.PortraitCombatSize.Icon);
		Relation.Value = GetUnitRelation(unitEntity);
		UsedSubtypeIcons = UIUtilityUnit.UsedSubtypeIcon(unitEntity);
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
		Game.Instance.CameraController?.Follower?.Release();
		CameraRig.Instance.ScrollTo(UnitPosition);
	}

	private void UpdateVisibility()
	{
		if ((Game.Instance.CurrentMode != GameModeType.Default && Game.Instance.CurrentMode != GameModeType.SpaceCombat && Game.Instance.CurrentMode != GameModeType.Pause) || Game.GetCamera() == null)
		{
			IsVisible.Value = false;
			return;
		}
		UnitPositionInUI = Game.GetCamera().WorldToScreenPoint(UnitPosition);
		IsVisible.Value = UnitPositionInUI.x <= 0f || UnitPositionInUI.x >= (float)Game.GetCamera().pixelWidth || UnitPositionInUI.y <= 0f || UnitPositionInUI.y >= (float)Game.GetCamera().pixelHeight;
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
}
