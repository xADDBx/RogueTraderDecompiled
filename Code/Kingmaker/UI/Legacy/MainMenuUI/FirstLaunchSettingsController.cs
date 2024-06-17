using System;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class FirstLaunchSettingsController : MonoBehaviour
{
	public static FirstLaunchSettingsController Instance;

	[SerializeField]
	private FirstLaunchSettingsPCView m_FirstLaunchSettingsPCView;

	private Action m_OnCompleteCallback;

	private FirstLaunchSettingsVM m_FirstLaunchSettingsVM;
}
