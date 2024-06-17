using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationComponentBaseVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	protected override void DisposeImplementation()
	{
	}
}
