using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Retrain;

public class RespecContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICharacterSelectorHandler, ISubscriber
{
	public readonly ReactiveProperty<RespecVM> RespecVM = new ReactiveProperty<RespecVM>();

	public RespecContextVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeRespec();
	}

	private void DisposeRespec()
	{
		DisposeAndRemove(RespecVM);
	}

	public void HandleSelectCharacter(List<BaseUnitEntity> characters, Action<BaseUnitEntity> successAction)
	{
		if (RespecVM.Value == null)
		{
			RespecVM disposable = (RespecVM.Value = new RespecVM(characters, successAction, DisposeRespec));
			AddDisposable(disposable);
		}
	}
}
