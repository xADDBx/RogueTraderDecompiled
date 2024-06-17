using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Kingmaker.Settings;
using Kingmaker.TextTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.Common;

public class ConsoleHintsTest : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private ConsoleType m_Type;

	private const string BindingsText = "{console_bind|LeftStickY} {console_bind|LeftStick} {console_bind|RightStickY} {console_bind|RightStick} {console_bind|DPadLeft} {console_bind|DPadRight} {console_bind|DPadUp} {console_bind|DPadDown} {console_bind|DPadVertical} {console_bind|DPadHorizontal} {console_bind|DPadFull} {console_bind|Confirm} {console_bind|Decline} {console_bind|Func01} {console_bind|Func02} {console_bind|LeftBottom} {console_bind|RightBottom} {console_bind|LeftUp} {console_bind|RightUp} {console_bind|LeftStickButton} {console_bind|RightStickButton} {console_bind|Options} {console_bind|FuncAdditional}";

	private void OnEnable()
	{
		GamePadIcons.SetInstance(ConsoleRoot.Instance.Icons);
		LocalizationManager.Instance.Init(SettingsRoot.Game.Main.Localization, SettingsController.Instance, !SettingsRoot.Game.Main.LocalizationWasTouched.GetValue());
		RefreshText();
	}

	private void RefreshText()
	{
		GamePad.Instance.ConsoleTypeProperty.Value = m_Type;
		string text = TextTemplateEngine.Instance.Process("{console_bind|LeftStickY} {console_bind|LeftStick} {console_bind|RightStickY} {console_bind|RightStick} {console_bind|DPadLeft} {console_bind|DPadRight} {console_bind|DPadUp} {console_bind|DPadDown} {console_bind|DPadVertical} {console_bind|DPadHorizontal} {console_bind|DPadFull} {console_bind|Confirm} {console_bind|Decline} {console_bind|Func01} {console_bind|Func02} {console_bind|LeftBottom} {console_bind|RightBottom} {console_bind|LeftUp} {console_bind|RightUp} {console_bind|LeftStickButton} {console_bind|RightStickButton} {console_bind|Options} {console_bind|FuncAdditional}");
		m_Text.text = text;
	}
}
