using System;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;

public class EncyclopediaVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<EncyclopediaPageVM> Page = new ReactiveProperty<EncyclopediaPageVM>();

	public readonly EncyclopediaNavigationVM NavigationVM;

	public EncyclopediaVM(INode node = null)
	{
		AddDisposable(NavigationVM = new EncyclopediaNavigationVM());
		HandleEncyclopediaPage(node ?? Game.Instance.Player.UISettings.CurrentEncyclopediaPage);
	}

	public void HandleEncyclopediaPage(INode node)
	{
		if (node != null)
		{
			IPage page = node as IPage;
			if (page != Page.Value?.Page)
			{
				Page.Value?.Dispose();
				NavigationVM.HandleEncyclopediaPage(node);
				EncyclopediaPageVM disposable = (Page.Value = new EncyclopediaPageVM(page));
				AddDisposable(disposable);
			}
		}
	}

	protected override void DisposeImplementation()
	{
		if (Game.Instance.Player.Tutorial.HasShownData)
		{
			EventBus.RaiseEvent(delegate(INewTutorialUIHandler h)
			{
				h.ShowTutorial(Game.Instance.Player.Tutorial.ShowingData);
			});
		}
	}
}
