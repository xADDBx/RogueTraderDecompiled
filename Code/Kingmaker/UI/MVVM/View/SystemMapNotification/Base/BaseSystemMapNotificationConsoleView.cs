using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SystemMapNotification.Base;

public abstract class BaseSystemMapNotificationConsoleView<TViewModel> : BaseSystemMapNotificationView<TViewModel> where TViewModel : SystemMapNotificationBaseVM
{
	public bool HasActionHint;

	[ConditionalShow("HasActionHint")]
	[SerializeField]
	protected ConsoleHint m_ActionHint;

	public void AddSystemMapInput(InputLayer inputLayer)
	{
		if (HasActionHint)
		{
			AddDisposable(m_ActionHint.Bind(inputLayer.AddButton(delegate
			{
				OnActionHintInteract();
			}, 11, base.ViewModel.IsShowUp, InputActionEventType.ButtonJustLongPressed)));
		}
	}

	private void OnActionHintInteract()
	{
		OnActionHintInteractImpl();
	}

	protected virtual void OnActionHintInteractImpl()
	{
	}
}
