using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public abstract class ShipAbilitySlotBaseView : ActionBarBaseSlotView
{
	[Header("Ability Cooldown")]
	[SerializeField]
	private GameObject m_CooldownContainer;

	private VisibilityController m_CooldownContainerVisibility;

	[SerializeField]
	private TextMeshProUGUI m_CooldownText;

	[Header("Resource Cost/Amount")]
	[SerializeField]
	private CanvasGroup m_ResourceRatioCanvasGroup;

	[SerializeField]
	private Slider m_ResourceRatioSlider;

	public int Index => base.ViewModel.Index;

	protected override void Awake()
	{
		base.Awake();
		m_CooldownContainerVisibility = VisibilityController.Control(m_CooldownContainer);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsOnCooldown.CombineLatest(base.ViewModel.CooldownText, (bool isOnCooldown, string cooldownText) => new { isOnCooldown, cooldownText }).Subscribe(value =>
		{
			m_CooldownContainerVisibility.SetVisible(value.isOnCooldown && !string.IsNullOrEmpty(value.cooldownText));
			m_CooldownText.text = value.cooldownText;
		}));
		AddDisposable(base.ViewModel.ResourceCost.CombineLatest(base.ViewModel.ResourceAmount, (int cost, int amount) => new { cost, amount }).Subscribe(value =>
		{
			SetResourceRatio(value.cost, value.amount);
		}));
	}

	private void SetResourceRatio(int cost, int amount)
	{
		if (cost <= 0 || amount < 0)
		{
			m_ResourceRatioCanvasGroup.alpha = 0f;
			return;
		}
		m_ResourceRatioCanvasGroup.alpha = 1f;
		m_ResourceRatioSlider.value = Mathf.Clamp((float)amount / (float)cost, 0f, 1f);
	}
}
