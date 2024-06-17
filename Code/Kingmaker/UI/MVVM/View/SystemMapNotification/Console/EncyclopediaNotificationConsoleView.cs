using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.SystemMapNotification.Base;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;

namespace Kingmaker.UI.MVVM.View.SystemMapNotification.Console;

public class EncyclopediaNotificationConsoleView : BaseSystemMapNotificationConsoleView<EncyclopediaNotificationVM>
{
	protected override void InitializeImpl()
	{
		m_Status.text = UIStrings.Instance.EncyclopediaTexts.EncyclopediaGlossaryButton.Text;
		m_ActionHint.SetLabel(UIStrings.Instance.EncyclopediaTexts.ToEncyclopedia.Text);
	}

	protected override void ShowNotificationImpl()
	{
		if (base.ViewModel.EncyclopediaName != null)
		{
			m_Title.text = base.ViewModel.EncyclopediaName + " " + UIStrings.Instance.EncyclopediaTexts.AddedToEncyclopedia.Text;
		}
	}

	protected override void OnActionHintInteractImpl()
	{
		EventBus.RaiseEvent(delegate(IEncyclopediaHandler x)
		{
			x.HandleEncyclopediaPage(base.ViewModel.EncyclopediaLink);
		});
	}
}
