using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ShipCustomization.Posts;

public class ShipPostsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IOnNewUnitOnPostHandler, ISubscriber
{
	public readonly PostSelectorVM PostsSelectorVM;

	public readonly PostOfficerSelectorVM PostOfficerSelectorVM;

	public readonly ReactiveProperty<PostEntityVM> CurrentSelectedPost = new ReactiveProperty<PostEntityVM>();

	public readonly ReactiveCommand OnPostUpdated = new ReactiveCommand();

	public readonly BoolReactiveProperty IsLocked = new BoolReactiveProperty();

	public ShipPostsVM(bool isLocked)
	{
		IsLocked.Value = isLocked;
		AddDisposable(PostsSelectorVM = new PostSelectorVM(CurrentSelectedPost));
		AddDisposable(PostOfficerSelectorVM = new PostOfficerSelectorVM(CurrentSelectedPost));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleNewUnit()
	{
		OnPostUpdated.Execute();
	}
}
