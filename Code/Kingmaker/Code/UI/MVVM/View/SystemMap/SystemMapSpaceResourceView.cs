using Kingmaker.Code.UI.MVVM.View.Colonization.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SystemMap;

public class SystemMapSpaceResourceView : ColonyResourceBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_CountAdditional;

	[SerializeField]
	private Image m_CountAdditionalArrowUp;

	[SerializeField]
	private Image m_CountAdditionalArrowDown;

	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private NegativeEffects m_NegativeEffects;

	[SerializeField]
	private GameObject[] m_SeparatorPins;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CountAdditional.Subscribe(SetCountAdditional));
		AddDisposable(base.ViewModel.Count.Subscribe(delegate(int val)
		{
			AddDisposable(m_BackgroundImage.SetTooltip(new TooltipTemplateColonyResource(base.ViewModel.BlueprintResource.Value, val), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace)));
		}));
		AddDisposable(base.ViewModel.IsNegative.Subscribe(SetNegativeEffect));
		SetRandomPins();
	}

	private void SetRandomPins()
	{
		m_SeparatorPins.ForEach(delegate(GameObject p)
		{
			p.SetActive(Random.value > 0.5f);
		});
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

	public void SetFocus(bool value)
	{
		m_Selectable.SetFocus(value);
		if (value)
		{
			this.ShowTooltip(new TooltipTemplateColonyResource(base.ViewModel.BlueprintResource.Value, base.ViewModel.Count.Value), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace));
		}
	}

	public bool IsValid()
	{
		return base.gameObject.activeSelf;
	}
}
