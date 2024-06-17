using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public abstract class BaseOvertipMapObjectVM : OvertipEntityVM
{
	public readonly MapObjectEntity MapObjectEntity;

	public ReactiveProperty<bool> IsEnabled = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> MapObjectIsHighlited = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsMouseOverUI = new ReactiveProperty<bool>(initialValue: false);

	public ReactiveCommand VisibilityChanged = new ReactiveCommand();

	public BoolReactiveProperty HasSurrounding = new BoolReactiveProperty();

	public BoolReactiveProperty IsChosen = new BoolReactiveProperty();

	public bool HideFromScreen
	{
		get
		{
			if (MapObjectEntity.IsRevealed && MapObjectEntity.IsVisibleForPlayer && MapObjectEntity.IsInState && MapObjectEntity.IsInGame)
			{
				return MapObjectEntity.IsInFogOfWar;
			}
			return true;
		}
	}

	public bool IsInCameraFrustum => MapObjectEntity.IsInCameraFrustum;

	protected BaseOvertipMapObjectVM(MapObjectEntity mapObjectEntity)
	{
		MapObjectEntity = mapObjectEntity;
		AddDisposable(IsEnabled.CombineLatest(MapObjectIsHighlited, IsMouseOverUI, (bool isEnabled, bool hover, bool mouseOver) => new { isEnabled, hover, mouseOver }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			VisibilityChanged.Execute();
		}));
	}

	protected override Vector3 GetEntityPosition()
	{
		return Vector3.zero;
	}

	public void HandleSurroundingObjectsChanged(bool isInNavigation, bool isChosen)
	{
		HasSurrounding.Value = isInNavigation;
		IsChosen.Value = isChosen;
	}
}
