using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsHeaderElementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Header = new ReactiveProperty<string>();

	public ColonyProjectsHeaderElementVM(string header)
	{
		Header.Value = header;
	}

	protected override void DisposeImplementation()
	{
	}
}
