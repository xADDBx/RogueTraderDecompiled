using System;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Inspect;

public abstract class InspectVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitClickUIHandler, ISubscriber, IShowInspectChanged
{
	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	protected InspectVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		HideInspect();
	}

	public void HandleShowInspect(bool state)
	{
		if (!state)
		{
			HideInspect();
		}
	}

	protected virtual void HideInspect()
	{
		Tooltip.Value = null;
	}

	public void HandleUnitRightClick(AbstractUnitEntity baseUnitEntity)
	{
		if (!Game.Instance.IsControllerGamepad)
		{
			OnUnitInvoke(baseUnitEntity);
		}
	}

	public void HandleUnitConsoleInvoke(AbstractUnitEntity baseUnitEntity)
	{
		OnUnitInvoke(baseUnitEntity);
	}

	protected abstract void OnUnitInvoke(AbstractUnitEntity baseUnitEntity);
}
