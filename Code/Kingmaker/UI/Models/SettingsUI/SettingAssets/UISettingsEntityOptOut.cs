using Kingmaker.QA.Analytics;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/Opt Out")]
public class UISettingsEntityOptOut : UISettingsEntityBase
{
	public void OpenSettingsInBrowser()
	{
		OwlcatAnalytics.Instance.OpenInBrowser();
	}

	public void DeleteData()
	{
		OwlcatAnalytics.Instance.RequestDataDeletion();
	}
}
