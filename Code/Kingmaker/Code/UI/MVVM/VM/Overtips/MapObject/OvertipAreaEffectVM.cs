using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.View;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public class OvertipAreaEffectVM : OvertipEntityVM
{
	public readonly AreaEffectEntity AreaEffectEntity;

	public readonly ReactiveProperty<bool> IsVisibleForPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<Vector3> CameraDistance = new ReactiveProperty<Vector3>();

	public BlueprintBuff Buff { get; }

	protected override bool UpdateEnabled => AreaEffectEntity.IsVisibleForPlayer;

	public bool HideFromScreen
	{
		get
		{
			if (AreaEffectEntity.IsRevealed && AreaEffectEntity.IsVisibleForPlayer && AreaEffectEntity.IsInState && AreaEffectEntity.IsInGame)
			{
				return AreaEffectEntity.IsInFogOfWar;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => AreaEffectEntity.IsInCameraFrustum;

	protected override Vector3 GetEntityPosition()
	{
		m_EntityPosition = AreaEffectEntity.View.ViewTransform.position;
		return m_EntityPosition;
	}

	public OvertipAreaEffectVM(AreaEffectEntity areaEffectEntity)
	{
		AreaEffectEntity = areaEffectEntity;
		Buff = areaEffectEntity.Blueprint.BlueprintBuffForTooltip;
	}

	protected override void OnUpdateHandler()
	{
		IsVisibleForPlayer.Value = AreaEffectEntity?.IsVisibleForPlayer ?? false;
		CameraDistance.Value = Position.Value - CameraRig.Instance.GetTargetPointPosition();
		base.OnUpdateHandler();
	}
}
