using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipCustomizationPCView : ShipCustomizationBaseView<ShipUpgradePCView, ShipSkillsPCView, ShipHealthAndRepairPCView>
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
			m_ShipSkillsPCView.Unbind();
			m_ShipPostsView.Unbind();
			m_SkillsAndPostsFadeAnimator.DisappearAnimation();
			m_ShipInfo.SetActive(value: true);
			break;
		case ShipCustomizationTab.Skills:
			m_ShipUpgradeView.Unbind();
			m_ShipSkillsPCView.Bind(base.ViewModel.ShipSkillsVM);
			m_ShipPostsView.Unbind();
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipInfo.SetActive(value: false);
			break;
		case ShipCustomizationTab.Posts:
			m_ShipUpgradeView.Unbind();
			m_ShipSkillsPCView.Unbind();
			m_ShipPostsView.Bind(base.ViewModel.ShipPostsVM);
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipInfo.SetActive(value: false);
			break;
		}
	}
}
