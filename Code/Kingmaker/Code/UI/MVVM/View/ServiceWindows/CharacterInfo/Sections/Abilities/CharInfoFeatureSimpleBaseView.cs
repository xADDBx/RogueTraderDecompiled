using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeatureSimpleBaseView : VirtualListElementViewBase<CharInfoFeatureVM>, IWidgetView
{
	[FormerlySerializedAs("m_Icon")]
	[Header("Icon")]
	[SerializeField]
	protected Image m_FeatureIcon;

	[SerializeField]
	protected TalentGroupView m_GroupsView;

	[SerializeField]
	protected TextMeshProUGUI m_DisplayName;

	[SerializeField]
	protected TextMeshProUGUI m_AcronymText;

	protected AccessibilityTextHelper TextHelper;

	public bool ShouldShowTalentIcons => base.ViewModel.ShouldShowTalentIcons;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		if (TextHelper == null)
		{
			TextHelper = new AccessibilityTextHelper(m_DisplayName);
		}
		Clear();
		Show();
		SetupIcon();
		SetupName();
		TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		TextHelper.Dispose();
	}

	protected virtual void Clear()
	{
		m_AcronymText.text = string.Empty;
		m_FeatureIcon.enabled = false;
		m_FeatureIcon.sprite = null;
		m_FeatureIcon.color = Color.white;
	}

	public void SetActiveState(bool state)
	{
		if (state)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Hide()
	{
		Clear();
		base.gameObject.SetActive(value: false);
	}

	public void SetupIcon()
	{
		m_FeatureIcon.enabled = true;
		if (base.ViewModel.Icon == null)
		{
			SetupAcronym();
			if (m_GroupsView != null && base.ViewModel.TalentIconsInfo.HasGroups)
			{
				m_GroupsView.SetupView(base.ViewModel.TalentIconsInfo);
				return;
			}
			m_FeatureIcon.color = UIUtility.GetColorByText(base.ViewModel.Acronym);
			m_FeatureIcon.sprite = UIUtility.GetIconByText(base.ViewModel.Acronym);
			m_GroupsView.Or(null)?.SetActiveState(state: false);
		}
		else
		{
			m_AcronymText.gameObject.SetActive(value: false);
			m_GroupsView.Or(null)?.SetActiveState(state: false);
			m_FeatureIcon.color = Color.white;
			m_FeatureIcon.sprite = base.ViewModel.Icon;
		}
	}

	private void SetupAcronym()
	{
		m_AcronymText.gameObject.SetActive(value: true);
		m_AcronymText.text = base.ViewModel.Acronym;
		m_AcronymText.color = (((bool)m_GroupsView && base.ViewModel.TalentIconsInfo.HasGroups) ? UIConfig.Instance.GroupAcronymColor : UIConfig.Instance.SingleAcronymColor);
	}

	public void SetupName()
	{
		if (!(m_DisplayName == null))
		{
			m_DisplayName.text = base.ViewModel.DisplayName;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoFeatureVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoFeatureVM;
	}

	[UnityEngine.ContextMenu("Show Random Icons")]
	private void Test()
	{
		int num = UnityEngine.Random.Range(1, 4);
		TalentGroup talentGroup = (TalentGroup)0;
		TalentGroup mainGroup = (TalentGroup)0;
		int length = Enum.GetValues(typeof(TalentGroup)).Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = UnityEngine.Random.Range(0, length);
			TalentGroup talentGroup2 = (TalentGroup)Mathf.Pow(2f, num2);
			talentGroup |= talentGroup2;
			if (i == 0)
			{
				mainGroup = talentGroup2;
			}
		}
		m_GroupsView.Or(null)?.SetupView(talentGroup, mainGroup);
	}
}
