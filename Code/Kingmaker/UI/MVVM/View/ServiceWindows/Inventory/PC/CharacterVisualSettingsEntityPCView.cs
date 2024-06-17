using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.PC;

public class CharacterVisualSettingsEntityPCView : CharacterVisualSettingsEntityView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(base.ViewModel.Switch));
	}
}
