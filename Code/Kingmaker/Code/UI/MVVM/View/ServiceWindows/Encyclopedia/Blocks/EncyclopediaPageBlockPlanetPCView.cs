using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockPlanetPCView : EncyclopediaPageBlockPCView<EncyclopediaPageBlockPlanetVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_AdminKnowAboutIt;

	[SerializeField]
	private TextMeshProUGUI m_SystemName;

	[SerializeField]
	private TextMeshProUGUI m_IsColonized;

	[SerializeField]
	private TextMeshProUGUI m_Security;

	[SerializeField]
	private TextMeshProUGUI m_HaveQuest;

	[SerializeField]
	private TextMeshProUGUI m_HaveRumour;

	[SerializeField]
	private float m_DefaultFontSizeTitle = 24f;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 24f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 21f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_Title.text = "<sprite=0> " + value + " <rotate=180><sprite=0>";
		}));
		AddDisposable(base.ViewModel.AdminKnowAboutIt.Subscribe(delegate(bool value)
		{
			m_AdminKnowAboutIt.text = (value ? UIStrings.Instance.EncyclopediaTexts.EncyclopediaIsReportedToAdministratum : UIStrings.Instance.EncyclopediaTexts.EncyclopediaIsNotReportedToAdministratum);
		}));
		AddDisposable(base.ViewModel.SystemName.Subscribe(delegate(string value)
		{
			m_SystemName.text = string.Format(UIStrings.Instance.EncyclopediaTexts.EncyclopediaPlanetPageSystem, value);
		}));
		AddDisposable(base.ViewModel.HaveQuest.Subscribe(delegate(bool value)
		{
			m_HaveQuest.text = string.Concat(UIStrings.Instance.EncyclopediaTexts.EncyclopediaPlanetPageHaveQuest, " ", value ? (Environment.NewLine + base.ViewModel.QuestObjectiveName.Value) : ((string)UIStrings.Instance.SettingsUI.DialogNo));
		}));
		AddDisposable(base.ViewModel.HaveRumour.Subscribe(delegate(bool value)
		{
			m_HaveRumour.text = string.Concat(UIStrings.Instance.EncyclopediaTexts.EncyclopediaPlanetPageHaveRumour, " ", value ? (Environment.NewLine + base.ViewModel.RumourObjectiveName.Value) : ((string)UIStrings.Instance.SettingsUI.DialogNo));
		}));
		AddDisposable(base.ViewModel.HaveColony.Subscribe(delegate(bool value)
		{
			m_IsColonized.gameObject.SetActive(value);
			m_Security.gameObject.SetActive(value);
			if (value)
			{
				m_IsColonized.text = string.Concat(UIStrings.Instance.EncyclopediaTexts.EncyclopediaPlanetPageIsColonized, " ", UIStrings.Instance.SettingsUI.DialogYes);
				AddDisposable(base.ViewModel.Security.Subscribe(delegate(int valueS)
				{
					m_Security.text = string.Format(UIStrings.Instance.EncyclopediaTexts.EncyclopediaPlanetPageSecurity, valueS);
				}));
			}
		}));
		SetTextFontSize();
		SetLinks();
	}

	private void SetTextFontSize()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Title.fontSize = (isControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * base.ViewModel.FontMultiplier;
		m_AdminKnowAboutIt.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_SystemName.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_IsColonized.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_Security.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_HaveQuest.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
		m_HaveRumour.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * base.ViewModel.FontMultiplier;
	}

	private void SetLinks()
	{
		AddDisposable(m_Title.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_AdminKnowAboutIt.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_SystemName.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_IsColonized.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_Security.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_HaveQuest.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
		AddDisposable(m_HaveRumour.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: true)));
	}

	public override List<TextMeshProUGUI> GetLinksTexts()
	{
		return new List<TextMeshProUGUI> { m_Title, m_AdminKnowAboutIt, m_SystemName, m_IsColonized, m_Security, m_HaveQuest, m_HaveRumour };
	}
}
