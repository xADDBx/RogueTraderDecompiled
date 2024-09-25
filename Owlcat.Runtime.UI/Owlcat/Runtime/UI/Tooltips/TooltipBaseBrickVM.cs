using System;
using Owlcat.Runtime.UI.MVVM;

namespace Owlcat.Runtime.UI.Tooltips;

public abstract class TooltipBaseBrickVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	protected override void DisposeImplementation()
	{
	}
}
