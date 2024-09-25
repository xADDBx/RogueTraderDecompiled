using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.SystemMapNotification.Base;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using UniRx;

namespace Kingmaker.UI.MVVM.View.SystemMapNotification.Console;

public class MiningNotificationConsoleView : BaseSystemMapNotificationConsoleView<MiningNotificationVM>
{
	protected override void InitializeImpl()
	{
		m_Status.text = UIStrings.Instance.ExplorationTexts.ResourceMiner.Text;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Message.Subscribe(delegate(string text)
		{
			m_Title.text = text;
		}));
	}
}
