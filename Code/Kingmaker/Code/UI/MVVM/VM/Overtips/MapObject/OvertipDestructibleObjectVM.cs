using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public class OvertipDestructibleObjectVM : BaseOvertipMapObjectVM
{
	public readonly UnitState UnitState;

	public readonly OvertipHealthBlockVM HealthBlockVM;

	public readonly OvertipNameBlockVM NameBlockVM;

	public readonly OvertipHitChanceBlockVM HitChanceBlockVM;

	public readonly OvertipCombatTextBlockVM CombatTextBlockVM;

	public readonly UnitBuffPartVM BuffPartVM;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public readonly OvertipCoverBlockVM CoverBlockVM;

	public readonly ReactiveProperty<bool> IsVisibleForPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<Vector3> CameraDistance = new ReactiveProperty<Vector3>();

	private readonly Transform m_Bone;

	private DestructibleEntity DestructibleEntity => MapObjectEntity as DestructibleEntity;

	protected override bool UpdateEnabled => MapObjectEntity.IsVisibleForPlayer;

	protected override Vector3 GetEntityPosition()
	{
		m_EntityPosition = ((m_Bone != null) ? m_Bone.transform.position : DestructibleEntity.View.OvertipPosition);
		return m_EntityPosition;
	}

	public OvertipDestructibleObjectVM(DestructibleEntity destructibleEntity)
		: base(destructibleEntity)
	{
		m_Bone = destructibleEntity.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
		UnitState = UnitStatesHolderVM.Instance.GetOrCreateUnitState(destructibleEntity);
		AddDisposable(HealthBlockVM = new OvertipHealthBlockVM(UnitState));
		AddDisposable(NameBlockVM = new OvertipNameBlockVM(UnitState));
		AddDisposable(HitChanceBlockVM = new OvertipHitChanceBlockVM(UnitState));
		AddDisposable(CombatTextBlockVM = new OvertipCombatTextBlockVM(destructibleEntity, Position));
		AddDisposable(BuffPartVM = new UnitBuffPartVM(destructibleEntity));
		AddDisposable(BarkBlockVM = new OvertipBarkBlockVM());
		AddDisposable(CoverBlockVM = new OvertipCoverBlockVM(UnitState));
	}

	protected override void OnUpdateHandler()
	{
		IsVisibleForPlayer.Value = MapObjectEntity?.IsVisibleForPlayer ?? false;
		CameraDistance.Value = Position.Value - CameraRig.Instance.GetTargetPointPosition();
		base.OnUpdateHandler();
	}

	public void HighlightChanged()
	{
		MapObjectIsHighlited.Value = MapObjectEntity?.View.AreaHighlighted ?? false;
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}
}
