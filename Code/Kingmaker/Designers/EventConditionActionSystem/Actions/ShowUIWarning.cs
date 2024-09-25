using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("904a91540b4d40128c59ce26c864705e")]
public class ShowUIWarning : GameAction
{
	public WarningNotificationType Type;

	[HideIf("HasType")]
	public LocalizedString String;

	private bool HasType => Type != WarningNotificationType.None;

	public override string GetCaption()
	{
		return "Show notification (" + (HasType ? Type.ToString() : String.ToString()) + ")";
	}

	protected override void RunAction()
	{
		if (HasType)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(Type);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(String.ToString());
			});
		}
	}
}
