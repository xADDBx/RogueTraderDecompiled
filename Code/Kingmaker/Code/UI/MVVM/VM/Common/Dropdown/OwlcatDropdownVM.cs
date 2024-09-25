using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;

public class OwlcatDropdownVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly IReadOnlyList<IViewModel> VMCollection;

	private readonly ReactiveProperty<IViewModel> m_SelectedVM = new ReactiveProperty<IViewModel>();

	private readonly ReactiveProperty<int> m_Index = new ReactiveProperty<int>();

	public IReadOnlyReactiveProperty<IViewModel> SelectedVM => m_SelectedVM;

	public IReadOnlyReactiveProperty<int> Index => m_Index;

	public OwlcatDropdownVM(IReadOnlyList<IViewModel> vmCollection, IViewModel selectedVM)
	{
		VMCollection = vmCollection;
		SetSelected(selectedVM);
	}

	public OwlcatDropdownVM(IReadOnlyList<IViewModel> vmCollection, int index = 0)
	{
		VMCollection = vmCollection;
		SetIndex(index);
	}

	protected override void DisposeImplementation()
	{
		VMCollection?.ForEach(delegate(IViewModel viewModel)
		{
			viewModel.Dispose();
		});
	}

	public void SetSelected(IViewModel viewModel)
	{
		if (VMCollection == null || !VMCollection.Contains(viewModel))
		{
			UberDebug.LogError("Selected ViewModel not contains in VMCollection");
			return;
		}
		m_SelectedVM.Value = viewModel;
		m_Index.Value = VMCollection.IndexOf(viewModel);
	}

	public void SetIndex(int index)
	{
		if (VMCollection == null || index >= VMCollection.Count || index < 0)
		{
			UberDebug.LogError("Index is out of range");
			return;
		}
		m_SelectedVM.Value = VMCollection[index];
		m_Index.Value = index;
	}
}
