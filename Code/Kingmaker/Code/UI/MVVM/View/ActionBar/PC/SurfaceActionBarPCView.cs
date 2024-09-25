using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.PC;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarPCView : ViewBase<SurfaceActionBarVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private FadeAnimator m_AdditionalAnimator;

	[Header("Parts")]
	[SerializeField]
	private SurfaceActionBarPartConsumablesPCView m_ConsumablesView;

	[SerializeField]
	private SurfaceActionBarPartWeaponsPCView m_WeaponsView;

	[SerializeField]
	private SurfaceActionBarPartAbilitiesPCView m_AbilitiesView;

	[SerializeField]
	private SurfaceMomentumPCView m_SurfaceMomentumPCView;

	[SerializeField]
	private VeilThicknessPCView m_VeilThicknessView;

	[SerializeField]
	private GameObject m_ContainerForMarkers;

	[Header("Alerts")]
	[SerializeField]
	private Image m_ClearMPAlertGroup;

	[SerializeField]
	private Image m_AttackAbilityGroupCooldownAlertGroup;

	[Header("Other")]
	[SerializeField]
	private TextMeshProUGUI m_AnotherPlayerTurnLabel;

	public void Initialize()
	{
		m_Animator.SetAlwaysActive(state: true);
		m_AdditionalAnimator.SetAlwaysActive(state: true);
		m_ConsumablesView.Initialize();
		m_WeaponsView.Initialize();
		m_AbilitiesView.Initialize();
		m_SurfaceMomentumPCView.Initialize();
		m_VeilThicknessView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_ConsumablesView.Bind(base.ViewModel.Consumables);
		m_WeaponsView.Bind(base.ViewModel.Weapons);
		m_AbilitiesView.Bind(base.ViewModel.Abilities);
		m_SurfaceMomentumPCView.Bind(base.ViewModel.SurfaceMomentumVM);
		m_VeilThicknessView.Bind(base.ViewModel.VeilThickness);
		AddDisposable(base.ViewModel.IsVisible.Subscribe(OnVisibleChanged));
		AddDisposable(base.ViewModel.CurrentCombatUnit.Subscribe(delegate
		{
			if (base.ViewModel.IsVisible.Value)
			{
				UISounds.Instance.Sounds.ActionBar.ActionBarSwitch.Play();
			}
		}));
		AddDisposable(base.ViewModel.EndTurnText.Subscribe(delegate(string value)
		{
			m_ClearMPAlertGroup.gameObject.SetActive(!string.IsNullOrEmpty(value));
		}));
		AddDisposable(base.ViewModel.IsAttackAbilityGroupCooldownAlertActive.Subscribe(m_AttackAbilityGroupCooldownAlertGroup.gameObject.SetActive));
		AddDisposable(m_AttackAbilityGroupCooldownAlertGroup.SetHint(UIStrings.Instance.Tooltips.AttackAbilityGroupCooldown));
		AddDisposable(m_ClearMPAlertGroup.SetHint(UIStrings.Instance.Tooltips.SpendAllMovementPoints));
		AddDisposable(base.ViewModel.IsNotControllableCharacter.CombineLatest(base.ViewModel.ControllablePlayerNickname, (bool notControllable, string playerNickName) => new { notControllable, playerNickName }).Subscribe(value =>
		{
			m_AnotherPlayerTurnLabel.transform.parent.parent.gameObject.SetActive(value.notControllable);
			m_AnotherPlayerTurnLabel.text = ((!string.IsNullOrWhiteSpace(value.playerNickName)) ? string.Format(UIStrings.Instance.ActionBar.AnotherPlayerTurnWithNickname, value.playerNickName) : ((string)UIStrings.Instance.ActionBar.AnotherPlayerTurn));
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void OnVisibleChanged(bool visible)
	{
		if (visible)
		{
			m_Animator.AppearAnimation();
			m_AdditionalAnimator.AppearAnimation();
			m_ContainerForMarkers.SetActive(value: true);
			UISounds.Instance.Sounds.ActionBar.ActionBarShow.Play();
		}
		else
		{
			m_Animator.DisappearAnimation();
			m_AdditionalAnimator.DisappearAnimation();
			m_ContainerForMarkers.SetActive(value: false);
			m_ClearMPAlertGroup.gameObject.SetActive(value: false);
			m_AttackAbilityGroupCooldownAlertGroup.gameObject.SetActive(value: false);
			UISounds.Instance.Sounds.ActionBar.ActionBarHide.Play();
		}
	}
}
