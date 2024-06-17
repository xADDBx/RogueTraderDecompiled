using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Common;

public abstract class CommonStaticComponentVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	protected override void DisposeImplementation()
	{
	}
}
