using Kingmaker.Localization;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Decorative;

public class SettingsEntityHeaderVM : VirtualListElementVMBase
{
	public readonly LocalizedString Tittle;

	public readonly ReactiveCommand LanguageChanged = new ReactiveCommand();

	public SettingsEntityHeaderVM(LocalizedString tittle)
	{
		Tittle = tittle;
	}

	public void UpdateLocalization()
	{
		LanguageChanged.Execute();
	}

	protected override void DisposeImplementation()
	{
	}
}
