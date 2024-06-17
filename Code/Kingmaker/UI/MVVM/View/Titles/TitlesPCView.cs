using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Titles;
using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.Utility.GameConst;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Titles;

public class TitlesPCView : TitlesBaseView
{
	[SerializeField]
	private TextMeshProUGUI m_HoldToSpeedupTitle;

	[SerializeField]
	private Color m_HintColor = Color.white;

	private string HintColorTag => ColorUtility.ToHtmlStringRGB(m_HintColor);

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.OpenCancelSettingsDialog));
		string prettyString = UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.GetBinding(0).GetPrettyString();
		m_HoldToSpeedupTitle.text = "<color=#" + HintColorTag + ">[" + prettyString + "]</color> " + UIStrings.Instance.Credits.SpeedUp.Text;
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOn, delegate
		{
			SpeedUp(state: true);
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipCutscene.name + UIConsts.SuffixOff, delegate
		{
			SpeedUp(state: false);
		}));
	}
}
