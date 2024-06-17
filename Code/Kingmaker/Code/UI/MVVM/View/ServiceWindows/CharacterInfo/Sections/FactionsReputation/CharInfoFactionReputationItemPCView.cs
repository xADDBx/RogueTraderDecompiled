using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoFactionReputationItemPCView : ViewBase<CharInfoFactionReputationItemVM>, IWidgetView
{
	[SerializeField]
	private Image m_FactionImage;

	[SerializeField]
	private Image m_InfoButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_FactionLabel;

	[SerializeField]
	private TextMeshProUGUI m_ReputationLabel;

	[Header("Progress")]
	[SerializeField]
	private TextMeshProUGUI m_ReputationProgressValue;

	[SerializeField]
	private Image m_ReputationProgressBar;

	[SerializeField]
	private TextMeshProUGUI m_ReputationLevelRomeNumber;

	[SerializeField]
	private Color m_NormalColor;

	[SerializeField]
	private Color m_MaxLevelColor;

	[Header("Vendors Location Info")]
	[SerializeField]
	private TextMeshProUGUI m_VendorsLabel;

	[SerializeField]
	private TextMeshProUGUI m_FactionDescription;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private FactionVendorInformationBaseView m_FactionVendorInformationPCViewPrefab;

	private AccessibilityTextHelper m_TextHelper;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_FactionLabel, m_ReputationLabel, m_ReputationProgressValue, m_VendorsLabel, m_FactionDescription);
		}
		m_FactionImage.sprite = UIConfig.Instance.UIIcons.GetFactionIcon(base.ViewModel.FactionType);
		m_FactionLabel.text = base.ViewModel.Label;
		if ((bool)m_ReputationLabel)
		{
			m_ReputationLabel.text = UIStrings.Instance.CharacterSheet.FactionsReputation;
		}
		if ((bool)m_VendorsLabel)
		{
			m_VendorsLabel.text = UIStrings.Instance.CharacterSheet.Vendors;
		}
		if ((bool)m_FactionDescription)
		{
			m_FactionDescription.text = base.ViewModel.Description;
		}
		DrawReputation();
		AddDisposable(base.ViewModel.CurrentReputation.Subscribe(delegate
		{
			DrawReputation();
		}));
		AddDisposable(base.ViewModel.ReputationLevel.Subscribe(delegate(int level)
		{
			SetVendorReputationLevel(level);
		}));
		AddDisposable(base.ViewModel.IsMaxLevel.Subscribe(SetColorMaxLevel));
		if ((bool)m_InfoButton)
		{
			AddDisposable(m_InfoButton.SetTooltip(base.ViewModel.Tooltip));
		}
		else if ((bool)m_Background)
		{
			AddDisposable(m_Background.SetTooltip(base.ViewModel.Tooltip));
		}
		DrawVendors();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	private void DrawVendors()
	{
		if (!(m_WidgetList == null))
		{
			m_WidgetList.Clear();
			FactionVendorInformationVM[] vmCollection = base.ViewModel.Vendors.ToArray();
			m_WidgetList.DrawEntries(vmCollection, m_FactionVendorInformationPCViewPrefab);
		}
	}

	private void SetColorMaxLevel(bool max)
	{
		m_ReputationProgressBar.color = (max ? m_MaxLevelColor : m_NormalColor);
		m_ReputationProgressValue.color = (max ? m_MaxLevelColor : m_NormalColor);
		m_ReputationLevelRomeNumber.color = (max ? m_MaxLevelColor : m_NormalColor);
	}

	private void DrawReputation()
	{
		m_ReputationProgressValue.text = base.ViewModel.GetCurrentAndNextLevelProgress();
		m_ReputationProgressBar.fillAmount = base.ViewModel.GetNextLevelReputationPoints() / (float)base.ViewModel.GetCurrentReputationPoints();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoFactionReputationItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoFactionReputationItemVM;
	}

	private void SetVendorReputationLevel(int level)
	{
		m_ReputationLevelRomeNumber.text = level.ToString();
	}
}
