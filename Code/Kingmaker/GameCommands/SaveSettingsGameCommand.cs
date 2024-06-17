using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;

namespace Kingmaker.GameCommands;

public sealed class SaveSettingsGameCommand : GameCommand
{
	protected override void ExecuteInternal()
	{
		Game.Instance.UISettingsManager.OnSettingsApplied();
		EventBus.RaiseEvent(delegate(ISaveSettingsHandler h)
		{
			h.HandleSaveSettings();
		});
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
	}
}
