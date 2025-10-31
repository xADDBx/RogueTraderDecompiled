using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks.AbilityPattern;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickIconPatternView : TooltipBaseBrickView<TooltipBrickIconPatternVM>
{
	[Header("SkillBlock")]
	[SerializeField]
	private Image m_SkillIcon;

	[SerializeField]
	private Image m_AcronymBackground;

	[SerializeField]
	private TextMeshProUGUI m_AcronymText;

	[SerializeField]
	private TalentGroupView m_TalentGroup;

	[Header("IconBlock")]
	[SerializeField]
	private Image m_Icon;

	[Tooltip("Has one of states of IconPatternMode enum : SkillMode, IconMode, NoneMode")]
	[SerializeField]
	private OwlcatMultiSelectable m_FrameSelectable;

	[Header("Pattern")]
	[SerializeField]
	private AbilityPatternView m_AbilityPatternView;

	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[Header("Secondary")]
	[SerializeField]
	protected TextMeshProUGUI m_SecondaryText;

	[SerializeField]
	private TextMeshProUGUI m_SecondaryValue;

	[Header("Tertiary")]
	[SerializeField]
	protected TextMeshProUGUI m_TertiaryText;

	[SerializeField]
	private TextMeshProUGUI m_TertiaryValue;

	[SerializeField]
	private float m_DefaultFontSizeTitle = 20f;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeTitle = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	[SerializeField]
	private TextMeshProUGUI m_AbilityPropertyNameText;

	[SerializeField]
	private TextMeshProUGUI m_AbilityValueText;

	[SerializeField]
	private GameObject m_AbilityValuesBlock;

	private readonly Dictionary<TextMeshProUGUI, TextFieldParams> m_DefaultParams = new Dictionary<TextMeshProUGUI, TextFieldParams>();

	private Color32? m_DefaultFrameColor;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_AbilityPatternView.Initialize(base.ViewModel.PatternData);
		m_Icon.sprite = base.ViewModel.Icon;
		m_AcronymText.text = base.ViewModel.Acronym;
		bool flag = !string.IsNullOrEmpty(base.ViewModel.Acronym);
		(flag ? m_AcronymBackground : m_SkillIcon).sprite = base.ViewModel.Icon;
		m_AcronymBackground.gameObject.SetActive(flag);
		m_SkillIcon.gameObject.SetActive(!flag);
		m_AcronymBackground.color = UIUtility.GetColorByText(base.ViewModel.Acronym);
		if (m_TalentGroup != null)
		{
			m_TalentGroup.SetupView(base.ViewModel.TalentIconInfo);
		}
		TextMeshProUGUI acronymText = m_AcronymText;
		TalentIconInfo talentIconInfo = base.ViewModel.TalentIconInfo;
		acronymText.color = ((talentIconInfo != null && talentIconInfo.HasGroups) ? UIConfig.Instance.GroupAcronymColor : UIConfig.Instance.SingleAcronymColor);
		m_FrameSelectable.Or(null)?.SetActiveLayer(base.ViewModel.IconMode.ToString());
		ApplyValues(m_TitleText, null, base.ViewModel.TitleValues);
		ApplyValues(m_SecondaryText, m_SecondaryValue, base.ViewModel.SecondaryValues);
		ApplyValues(m_TertiaryText, m_TertiaryValue, base.ViewModel.TertiaryValues);
		AddDisposable(m_SecondaryText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		AddDisposable(m_TertiaryText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(m_SkillIcon.SetTooltip(base.ViewModel.Tooltip));
		}
		SetTextSize();
		if (!(m_AbilityValuesBlock == null) && !(m_AbilityPropertyNameText == null) && !(m_AbilityValueText == null))
		{
			m_AbilityValuesBlock.SetActive(!base.ViewModel.AbilityPropertyName.IsNullOrEmpty());
			m_AbilityPropertyNameText.text = base.ViewModel.AbilityPropertyName;
			m_AbilityValueText.text = base.ViewModel.AbilityPropertyValue;
			AddDisposable(m_AbilityValueText.SetTooltip(new TooltipTemplateHint(base.ViewModel.AbilityPropertyDesc)));
		}
	}

	private void SetText(string text)
	{
	}

	private void SetTextSize()
	{
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_TitleText.fontSize = (isControllerMouse ? m_DefaultFontSizeTitle : m_DefaultConsoleFontSizeTitle) * FontMultiplier;
		m_SecondaryText.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_SecondaryValue.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_TertiaryText.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
		m_TertiaryValue.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
		m_AbilityPatternView.Destroy();
	}

	private void ApplyValues(TextMeshProUGUI text, TextMeshProUGUI value, TooltipBrickIconPattern.TextFieldValues textFieldValues)
	{
		text.ApplyTextFieldParams(textFieldValues?.TextParams, GetOrCreateDefaultParams(text));
		value.ApplyTextFieldParams(textFieldValues?.ValueParams, GetOrCreateDefaultParams(value));
		if (text != null)
		{
			text.text = textFieldValues?.Text;
			text.gameObject.SetActive(!string.IsNullOrEmpty(textFieldValues?.Text));
		}
		if (value != null)
		{
			value.text = textFieldValues?.Value;
			value.gameObject.SetActive(!string.IsNullOrEmpty(textFieldValues?.Value));
		}
	}

	private TextFieldParams GetOrCreateDefaultParams(TextMeshProUGUI textField)
	{
		TextFieldParams value = null;
		if (textField != null && !m_DefaultParams.TryGetValue(textField, out value))
		{
			TextFieldParams textFieldParams2 = (m_DefaultParams[textField] = textField.GetTextFieldParams());
			value = textFieldParams2;
		}
		return value;
	}
}
