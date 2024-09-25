using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public class LensSelectorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly bool NeedToResetPosition;

	public LensSelectorVM(bool needToResetPosition = true)
	{
		NeedToResetPosition = needToResetPosition;
	}

	protected override void DisposeImplementation()
	{
	}
}
