using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA.Analytics;

namespace Kingmaker.Settings;

public class GameSettingsController
{
	public GameSettingsController()
	{
		SettingsRoot.Game.Main.SendGameStatistic.OnValueChanged += SendStatisticOnChanged;
		SettingsRoot.Game.Main.BloodOnCharacters.OnValueChanged += delegate
		{
			EventBus.RaiseEvent(delegate(IBloodSettingsHandler h)
			{
				h.HandleBloodSettingChanged();
			});
		};
	}

	private void SendStatisticOnChanged(bool sendStatistics)
	{
		if (sendStatistics)
		{
			OwlcatAnalytics.Instance.StartDataCollection();
			return;
		}
		OwlcatAnalytics.Instance.StopDataCollection();
		SettingsRoot.Game.Main.SendSaves.SetValueAndConfirm(value: false);
	}
}
