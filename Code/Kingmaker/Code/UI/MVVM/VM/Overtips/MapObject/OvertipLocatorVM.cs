using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public class OvertipLocatorVM : OvertipEntityVM
{
	public readonly LocatorEntity LocatorEntity;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public readonly ReactiveProperty<bool> IsVisibleForPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<Vector3> CameraDistance = new ReactiveProperty<Vector3>();

	private readonly Transform m_Bone;

	public ReactiveProperty<bool> IsBarkActive => BarkBlockVM?.IsBarkActive;

	protected override bool UpdateEnabled => LocatorEntity.IsVisibleForPlayer;

	public bool HideFromScreen
	{
		get
		{
			if (LocatorEntity.IsRevealed && LocatorEntity.IsVisibleForPlayer && LocatorEntity.IsInState && LocatorEntity.IsInGame)
			{
				return LocatorEntity.IsInFogOfWar;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => LocatorEntity.IsInCameraFrustum;

	public bool ForceOnScreen => IsBarkActive.Value;

	protected override Vector3 GetEntityPosition()
	{
		m_EntityPosition = ((m_Bone != null) ? m_Bone.transform.position : LocatorEntity.View.ViewTransform.position);
		return m_EntityPosition;
	}

	public OvertipLocatorVM(LocatorEntity locatorEntity)
	{
		LocatorEntity = locatorEntity;
		m_Bone = locatorEntity?.View?.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
		AddDisposable(BarkBlockVM = new OvertipBarkBlockVM());
	}

	protected override void OnUpdateHandler()
	{
		IsVisibleForPlayer.Value = LocatorEntity?.IsVisibleForPlayer ?? false;
		CameraDistance.Value = Position.Value - CameraRig.Instance.GetTargetPointPosition();
		base.OnUpdateHandler();
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
