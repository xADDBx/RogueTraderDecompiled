using Kingmaker.Code.UI.MVVM.View.SystemMap;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SystemMap;

public class SystemMapSpaceProfitFactorView : ViewBase<JournalOrderProfitFactorVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Count;

	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private NegativeEffects m_NegativeEffects;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	[SerializeField]
	private TextMeshProUGUI m_CountAdditional;

	[SerializeField]
	private Image m_CountAdditionalArrowUp;

	[SerializeField]
	private Image m_CountAdditionalArrowDown;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite i)
		{
			m_Icon.sprite = i;
		}));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(float c)
		{
			m_Count.text = c.ToString();
		}));
		AddDisposable(base.ViewModel.IsNegative.Subscribe(SetNegativeEffect));
		AddDisposable(base.ViewModel.CountAdditional.Subscribe(SetCountAdditional));
		AddDisposable(m_BackgroundImage.SetTooltip(new TooltipTemplateProfitFactor(base.ViewModel.ProfitFactorVM), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace)));
	}

	private void SetNegativeEffect(bool state)
	{
		NegativeEffects negativeEffects = m_NegativeEffects;
		Color color = (state ? negativeEffects.RedIconColor : negativeEffects.NormalIconColor);
		Color color2 = (state ? negativeEffects.RedSeparatorColor : negativeEffects.NormalSeparatorColor);
		Color color3 = (state ? negativeEffects.RedCountColor : negativeEffects.NormalCountColor);
		Color color4 = (state ? negativeEffects.RedCountAdditionalColor : negativeEffects.NormalCountAdditionalColor);
		if (state)
		{
			negativeEffects.RedBackground.AppearAnimation();
		}
		else
		{
			negativeEffects.RedBackground.DisappearAnimation();
		}
		negativeEffects.ResourceIcon.color = color;
		negativeEffects.ResourceSeparator.color = color2;
		negativeEffects.ResourceCount.color = color3;
		negativeEffects.ResourceCountAdditional.color = color4;
	}

	private void SetCountAdditional(int countAdditional)
	{
		if (countAdditional == 0)
		{
			m_CountAdditional.gameObject.SetActive(value: false);
			m_CountAdditionalArrowUp.enabled = false;
			m_CountAdditionalArrowDown.enabled = false;
		}
		else
		{
			string text = countAdditional.ToString();
			m_CountAdditional.text = ((countAdditional > 0) ? ("+" + text) : text);
			m_CountAdditional.gameObject.SetActive(value: true);
			m_CountAdditionalArrowUp.enabled = countAdditional > 0;
			m_CountAdditionalArrowDown.enabled = countAdditional < 0;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
		if (value)
		{
			this.ShowTooltip(new TooltipTemplateProfitFactor(base.ViewModel.ProfitFactorVM), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace));
		}
	}

	public bool IsValid()
	{
		return base.gameObject.activeSelf;
	}
}
