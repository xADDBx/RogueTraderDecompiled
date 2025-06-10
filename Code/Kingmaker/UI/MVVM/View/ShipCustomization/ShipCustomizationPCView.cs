using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipCustomizationPCView : ShipCustomizationBaseView<ShipUpgradePCView, ShipSkillsPCView, ShipHealthAndRepairPCView, ShipAbilitiesPCView>
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	public override void Initialize()
	{
		base.Initialize();
		m_FadeAnimator.Initialize();
		m_ShipUpgradeView.Initialize();
		m_ShipSkillsPCView.Initialize();
		m_ShipPostsView.Initialize();
		m_ShipAbilitiesView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Close();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, base.SetPrevTab));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, base.SetNextTab));
	}

	protected override void BindShip()
	{
		m_ShipHealthAndRepairView.Bind(base.ViewModel.ShipHealthAndRepairVM);
	}

	protected override void BindSelectedView(ShipCustomizationTab tab)
	{
		base.BindSelectedView(tab);
		switch (tab)
		{
		case ShipCustomizationTab.Upgrade:
			m_ShipUpgradeView.Bind(base.ViewModel.ShipUpgradeVm);
			m_SkillsAndPostsFadeAnimator.DisappearAnimation();
			m_ShipSkillsPCView.Unbind();
			m_ShipPostsView.Unbind();
			m_ShipAbilitiesView.Unbind();
			m_ShipInfo.SetActive(value: true);
			break;
		case ShipCustomizationTab.Skills:
			m_ShipSkillsPCView.Bind(base.ViewModel.ShipSkillsVM);
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipUpgradeView.Unbind();
			m_ShipPostsView.Unbind();
			m_ShipAbilitiesView.Unbind();
			m_ShipInfo.SetActive(value: false);
			break;
		case ShipCustomizationTab.Posts:
			m_ShipPostsView.Bind(base.ViewModel.ShipPostsVM);
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipUpgradeView.Unbind();
			m_ShipSkillsPCView.Unbind();
			m_ShipAbilitiesView.Unbind();
			m_ShipInfo.SetActive(value: false);
			break;
		case ShipCustomizationTab.Abilities:
			m_ShipAbilitiesView.Bind(base.ViewModel.ShipAbilitiesVM);
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipUpgradeView.Unbind();
			m_ShipSkillsPCView.Unbind();
			m_ShipPostsView.Unbind();
			m_ShipInfo.SetActive(value: false);
			break;
		}
	}
}
