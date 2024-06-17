using System;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.FeedbackPopup;

public class FeedbackPopupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveCollection<FeedbackPopupItemVM> Items = new ReactiveCollection<FeedbackPopupItemVM>();

	private readonly Action m_CloseAction;

	public FeedbackPopupVM(Action closeAction)
	{
		m_CloseAction = closeAction;
		FeedbackPopupItem[] items = FeedbackPopupConfigLoader.Instance.Items;
		foreach (FeedbackPopupItem config in items)
		{
			Items.Add(new FeedbackPopupItemVM(config));
		}
	}

	protected override void DisposeImplementation()
	{
		Items.ForEach(delegate(FeedbackPopupItemVM vm)
		{
			vm.Dispose();
		});
		Items.Clear();
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
	}
}
