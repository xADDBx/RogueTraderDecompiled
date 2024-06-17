using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogHistoryEntity : MonoBehaviour, ISettingsFontSizeUIHandler, ISubscriber
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private float m_DefaultFontSize = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 24f;

	private IDisposable m_LinkTooltip;

	public TextMeshProUGUI Text => m_Text;

	private static float FontMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public void Initialize(string str)
	{
		EventBus.Subscribe(this);
		m_Text.text = str;
		m_LinkTooltip = m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true));
		SetTextFontSize(FontMultiplier);
	}

	private void SetTextFontSize(float multiplier)
	{
		m_Text.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * multiplier;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
		m_LinkTooltip.Dispose();
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetTextFontSize(size);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
