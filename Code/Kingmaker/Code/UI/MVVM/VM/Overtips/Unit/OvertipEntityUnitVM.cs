using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;

public class OvertipEntityUnitVM : OvertipEntityVM
{
	public readonly UnitState UnitState;

	public readonly OvertipHealthBlockVM HealthBlockVM;

	public readonly OvertipNameBlockVM NameBlockVM;

	public readonly UnitBuffPartVM BuffPartVM;

	public readonly OvertipCombatTextBlockVM CombatTextBlockVM;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public readonly OvertipHitChanceBlockVM HitChanceBlockVM;

	public readonly OvertipCoverBlockVM CoverBlockVM;

	public readonly OvertipVoidshipHealthVM VoidshipHealthVM;

	public readonly OvertipTorpedoVM OvertipTorpedoVM;

	public float? DeathDelay;

	private bool m_UnitScanned;

	private Transform m_Bone;

	private Bounds m_Bounds;

	public readonly ReactiveProperty<Vector3> CameraDistance = new ReactiveProperty<Vector3>();

	public readonly BoolReactiveProperty HasSurrounding = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsChosen = new BoolReactiveProperty();

	public bool ImmediatelyDestroyActivated;

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
		if (m_Bone != null && !UnitState.Unit.IsDeadOrUnconscious)
		{
			if (Game.Instance.CurrentMode != GameModeType.StarSystem)
			{
				m_EntityPosition = m_Bone.position;
			}
			else
			{
				m_EntityPosition = m_Bone.position;
				m_EntityPosition.y += 5f;
			}
		}
		else if (UnitState.Unit.IsDeadOrUnconscious && UnitState.Unit.IsDeadAndHasAttachedDroppedLoot)
		{
			m_EntityPosition = UnitState.Unit.MechanicEntity.GetOptional<PartInventory>()?.AttachedDroppedLootData.Position ?? Vector3.zero;
			m_EntityPosition.y += Vector2.zero.y;
		}
		else if (UnitState.Unit.IsDeadOrUnconscious && Unit is BaseUnitEntity baseUnitEntity)
		{
			m_EntityPosition = baseUnitEntity.View?.CorpseOvertipPosition ?? baseUnitEntity.Position;
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

	public OvertipEntityUnitVM(AbstractUnitEntity unit)
	{
		UnitState = UnitStatesHolderVM.Instance.GetOrCreateUnitState(unit);
		AddDisposable(HealthBlockVM = new OvertipHealthBlockVM(UnitState));
		AddDisposable(NameBlockVM = new OvertipNameBlockVM(UnitState));
		AddDisposable(BuffPartVM = new UnitBuffPartVM(unit));
		AddDisposable(CombatTextBlockVM = new OvertipCombatTextBlockVM(unit, Position));
		AddDisposable(BarkBlockVM = new OvertipBarkBlockVM());
		AddDisposable(CoverBlockVM = new OvertipCoverBlockVM(UnitState));
		AddDisposable(HitChanceBlockVM = new OvertipHitChanceBlockVM(UnitState));
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			AddDisposable(VoidshipHealthVM = new OvertipVoidshipHealthVM());
		}
		AddDisposable(OvertipTorpedoVM = new OvertipTorpedoVM(UnitState));
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

	public void SetDestroyViewImmediately()
	{
		ImmediatelyDestroyActivated = true;
	}
}
