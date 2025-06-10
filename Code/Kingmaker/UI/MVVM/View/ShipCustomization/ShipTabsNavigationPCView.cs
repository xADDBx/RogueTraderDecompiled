using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipTabsNavigationPCView : ViewBase<ShipTabsNavigationVM>
{
	[SerializeField]
	private OwlcatMultiButton m_UpgradeButton;

	[SerializeField]
	private OwlcatMultiButton m_SkillsButton;

	[SerializeField]
	private OwlcatMultiButton m_PostsButton;

	[SerializeField]
	private OwlcatMultiButton m_AbilitiesButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_UpgradeLabel;

	[SerializeField]
	private TextMeshProUGUI m_SkillsLabel;

	[SerializeField]
	private TextMeshProUGUI m_PostsLabel;

	[SerializeField]
	private TextMeshProUGUI m_AbilitiesLabel;

	private readonly string m_ActiveTabLayer = "Active";

	private readonly string m_UnactiveTabLayer = "Unactive";

	protected override void BindViewImplementation()
	{
		SetLabels();
		AddDisposable(m_UpgradeButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(ShipCustomizationTab.Upgrade);
		}));
		AddDisposable(m_SkillsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(ShipCustomizationTab.Skills);
		}));
		AddDisposable(m_PostsButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(ShipCustomizationTab.Posts);
		}));
		AddDisposable(m_AbilitiesButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetActiveTab(ShipCustomizationTab.Abilities);
		}));
		AddDisposable(base.ViewModel.ActiveTab.AsObservable().Subscribe(UpdateActiveTab));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void UpdateActiveTab(ShipCustomizationTab activeTab)
	{
		m_UpgradeButton.SetActiveLayer((activeTab == ShipCustomizationTab.Upgrade) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_SkillsButton.SetActiveLayer((activeTab == ShipCustomizationTab.Skills) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_PostsButton.SetActiveLayer((activeTab == ShipCustomizationTab.Posts) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_AbilitiesButton.SetActiveLayer((activeTab == ShipCustomizationTab.Abilities) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		base.ViewModel.SetActiveTab(activeTab);
	}

	public ShipCustomizationTab GetActiveTab()
	{
		return base.ViewModel.ActiveTab.Value;
	}

	public void SetNextTab()
	{
		m_UpgradeButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Upgrade) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_SkillsButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Skills) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_PostsButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Posts) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_AbilitiesButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Abilities) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		base.ViewModel.SetNextTab();
	}

	public void SetPrevTab()
	{
		m_UpgradeButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Upgrade) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_SkillsButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Skills) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_PostsButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Posts) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		m_AbilitiesButton.SetActiveLayer((base.ViewModel.ActiveTab.Value == ShipCustomizationTab.Abilities) ? m_UnactiveTabLayer : m_ActiveTabLayer);
		base.ViewModel.SetPrevTab();
	}

	private void SetLabels()
	{
		m_UpgradeLabel.text = UIStrings.Instance.ShipCustomization.MenuItemComponents;
		m_SkillsLabel.text = UIStrings.Instance.ShipCustomization.MenuItemUpgrade;
		m_PostsLabel.text = UIStrings.Instance.HUDTexts.PostsBar;
		m_AbilitiesLabel.text = UIStrings.Instance.ShipCustomization.Accolades;
	}
}
