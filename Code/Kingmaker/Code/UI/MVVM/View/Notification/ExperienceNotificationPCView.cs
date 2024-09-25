using Kingmaker.Code.UI.MVVM.VM.Notification;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Notification;

public class ExperienceNotificationPCView : NotificationPCView<ExperienceNotificationVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.ShowExperienceAmount.Subscribe(delegate
		{
			m_Text.text = "+" + base.ViewModel.ShowExperienceAmount.ToString() + " xp";
		}));
	}
}
