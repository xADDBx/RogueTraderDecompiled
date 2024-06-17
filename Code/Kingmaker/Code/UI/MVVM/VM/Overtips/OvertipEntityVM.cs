using System;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips;

public abstract class OvertipEntityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetPingEntity, ISubscriber
{
	public readonly ReactiveProperty<Vector3> Position = new ReactiveProperty<Vector3>(Vector3.zero);

	protected Vector3 m_EntityPosition;

	public float OvertipVerticalCorrection;

	public readonly ReactiveCommand<Entity> CoopPingEntity = new ReactiveCommand<Entity>();

	public bool IsCutscene => Game.Instance.CurrentMode == GameModeType.Cutscene;

	public bool IsInDialog => Game.Instance.CurrentMode == GameModeType.Dialog;

	protected virtual bool UpdateEnabled => true;

	protected abstract Vector3 GetEntityPosition();

	protected OvertipEntityVM()
	{
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			InternalUpdate();
		}));
	}

	private void InternalUpdate()
	{
		if (UpdateEnabled)
		{
			OnUpdateHandler();
		}
	}

	protected virtual void OnUpdateHandler()
	{
		Position.Value = GetEntityPosition();
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		CoopPingEntity.Execute(entity);
	}
}
