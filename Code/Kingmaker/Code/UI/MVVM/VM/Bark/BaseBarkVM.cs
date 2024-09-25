using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class BaseBarkVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> Text = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> IsBarkActive = new ReactiveProperty<bool>(initialValue: false);

	protected override void DisposeImplementation()
	{
	}

	public void ShowBark(string text)
	{
		Text.Value = text;
		IsBarkActive.Value = true;
	}

	public void HideBark()
	{
		IsBarkActive.Value = false;
	}
}
