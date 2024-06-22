using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;

public abstract class LocalMapMarkerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetPingEntity, ISubscriber
{
	public readonly ReactiveProperty<Vector3> Position = new ReactiveProperty<Vector3>();

	public readonly ReactiveProperty<string> Description = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsEnemy = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<Sprite> Portrait = new ReactiveProperty<Sprite>(null);

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsMapObject = new ReactiveProperty<bool>(initialValue: false);

	public LocalMapMarkType MarkerType = LocalMapMarkType.Invalid;

	public readonly ReactiveCommand<(NetPlayer player, Entity entity)> CoopPingEntity = new ReactiveCommand<(NetPlayer, Entity)>();

	protected LocalMapMarkerVM()
	{
		AddDisposable(Observable.EveryUpdate().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	protected abstract void OnUpdateHandler();

	public abstract Entity GetEntity();

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		CoopPingEntity.Execute((player, entity));
	}
}
