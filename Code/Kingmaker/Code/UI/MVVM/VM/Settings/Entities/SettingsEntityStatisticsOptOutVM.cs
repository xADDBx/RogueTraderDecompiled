using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingsEntityStatisticsOptOutVM : SettingsEntityVM
{
	private UISettingsEntityOptOut m_StatisticsEntity;

	public SettingsEntityStatisticsOptOutVM(UISettingsEntityOptOut uiSettingsEntity)
		: base(uiSettingsEntity)
	{
		m_StatisticsEntity = uiSettingsEntity;
	}

	public void OpenSettingsInBrowser()
	{
		m_StatisticsEntity.OpenSettingsInBrowser();
	}

	public void DeleteData()
	{
		m_StatisticsEntity.DeleteData();
	}

	public void DeleteStatisticsData()
	{
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(UIStrings.Instance.SettingsUI.DeleteStatisticsDataDialogue, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton buttonPressed)
			{
				if (buttonPressed == DialogMessageBoxBase.BoxButton.Yes)
				{
					DeleteData();
				}
			});
		});
	}
}
