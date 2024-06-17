using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings.Entities;

public class FirstLaunchEntityLanguageItemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Title;

	private readonly int m_Index;

	private readonly Action<int> m_SetSelected;

	public readonly ReadOnlyReactiveProperty<bool> IsSelected;

	public FirstLaunchEntityLanguageItemVM(string language, int index, Action<int> setSelected, IObservable<int> selectedIndex)
	{
		m_Index = index;
		m_SetSelected = setSelected;
		AddDisposable(IsSelected = selectedIndex.Select((int i) => i == m_Index).ToReadOnlyReactiveProperty());
		Title = language;
	}

	public void SetSelected()
	{
		m_SetSelected?.Invoke(m_Index);
	}

	protected override void DisposeImplementation()
	{
	}
}
