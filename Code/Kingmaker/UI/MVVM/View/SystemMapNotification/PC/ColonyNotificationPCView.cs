using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.GameCommands;
using Kingmaker.UI.MVVM.View.SystemMapNotification.Base;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using UniRx;

namespace Kingmaker.UI.MVVM.View.SystemMapNotification.PC;

public class ColonyNotificationPCView : BaseSystemMapNotificationPCView<ColonyNotificationVM>
{
	protected override void InitializeImpl()
	{
		m_ActionButtonLabel.text = UIStrings.Instance.ColonyNotificationTexts.ColonyManagementButtonText.Text;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Data.Subscribe(SetData));
	}

	protected override void OnButtonClickImpl()
	{
		Game.Instance.GameCommandQueue.ColonyManagementUIOpen();
	}

	private void SetData(ColonyNotificationData data)
	{
		if (data.Name != null)
		{
			bool flag = data.Type == ColonyNotificationType.Chronicle;
			m_Status.text = (flag ? UIStrings.Instance.ColonyNotificationTexts.NewChronicleStatus.Text : UIStrings.Instance.ColonyNotificationTexts.NewEventStatus.Text);
			string format = (flag ? UIStrings.Instance.ColonyNotificationTexts.ChronicleMessage.Text : UIStrings.Instance.ColonyNotificationTexts.EventMessage.Text);
			m_Title.text = string.Format(format, data.Name);
		}
	}
}
