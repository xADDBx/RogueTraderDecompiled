using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UI.Common;

public static class UIUtilityWindows
{
	public static void OpenAugmentationsWindow(Action onClosed = null)
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenWindowOfType(ServiceWindowsType.Augmentations, onClosed);
		});
	}
}
