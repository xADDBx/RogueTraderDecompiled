using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;

public class LightweightUnitOvertipVM : OvertipEntityVM
{
	public readonly UnitState UnitState;

	public readonly OvertipNameBlockVM NameBlockVM;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public float? DeathDelay;

	private bool m_UnitScanned;

	private Transform m_Bone;

	private Bounds m_Bounds;

	public readonly ReactiveProperty<Vector3> CameraDistance = new ReactiveProperty<Vector3>();

	public BoolReactiveProperty HasSurrounding = new BoolReactiveProperty();

	public BoolReactiveProperty IsChosen = new BoolReactiveProperty();

	public MechanicEntity Unit => UnitState.Unit.MechanicEntity;

	public MechanicEntityUIWrapper UnitUIWrapper => UnitState.Unit;

	public ReactiveProperty<bool> IsBarkActive => BarkBlockVM?.IsBarkActive;

	public bool ForceOnScreen => IsBarkActive.Value;

	public bool HideFromScreen
	{
		get
		{
			if (Unit != null)
			{
				if (Game.Instance.IsControllerGamepad)
				{
					if (Unit.IsInState)
					{
						if (!Unit.IsVisibleForPlayer)
						{
							return !Unit.IsDirectlyControllable;
						}
						return false;
					}
					return true;
				}
				if (Unit.IsInState && Unit is AbstractUnitEntity { IsAwake: not false })
				{
					if (!Unit.IsVisibleForPlayer)
					{
						return !Unit.IsDirectlyControllable;
					}
					return false;
				}
				return true;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => Unit?.IsInCameraFrustum ?? false;

	protected override Vector3 GetEntityPosition()
	{
		if (!m_UnitScanned && Unit?.View != null)
		{
			m_Bone = Unit.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
			m_UnitScanned = true;
		}
		if (m_Bone != null)
		{
			m_EntityPosition = m_Bone.position;
		}
		else if (Unit is AbstractUnitEntity abstractUnitEntity)
		{
			m_EntityPosition = abstractUnitEntity.Position;
			m_EntityPosition.y += abstractUnitEntity.View?.CameraOrientedBoundsSize.y ?? Vector2.zero.y;
		}
		else
		{
			m_EntityPosition = Vector3.zero;
		}
		return m_EntityPosition;
	}

	public LightweightUnitOvertipVM(LightweightUnitEntity unit)
	{
		UnitState = UnitStatesHolderVM.Instance.GetOrCreateUnitState(unit);
		AddDisposable(NameBlockVM = new OvertipNameBlockVM(UnitState));
		AddDisposable(BarkBlockVM = new OvertipBarkBlockVM());
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		CameraDistance.Value = Position.Value - CameraRig.Instance.GetTargetPointPosition();
	}

	protected override void DisposeImplementation()
	{
		if (Unit != null)
		{
			UnitStatesHolderVM.Instance.RemoveUnitState(Unit);
		}
		else
		{
			UnitStatesHolderVM.Instance.RemoveUnitState(UnitUIWrapper.UniqueId);
		}
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}

	public void SetDeathDelay(float val)
	{
		DeathDelay = val;
	}

	public void HandleSurroundingObjectsChanged(bool moreThenOne, bool isChosen)
	{
		HasSurrounding.Value = moreThenOne;
		IsChosen.Value = isChosen;
	}
}
