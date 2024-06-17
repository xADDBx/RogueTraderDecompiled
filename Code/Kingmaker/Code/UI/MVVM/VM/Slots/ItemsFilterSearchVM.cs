using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public class ItemsFilterSearchVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly ReactiveProperty<string> m_SearchString;

	public ItemsFilterSearchVM(ReactiveProperty<string> searchString)
	{
		m_SearchString = searchString;
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetSearchString(string value)
	{
		m_SearchString.Value = value;
	}
}
