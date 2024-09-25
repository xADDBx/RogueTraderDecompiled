using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Exploration;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationExpeditionPCView : ExplorationComponentBaseView<ExplorationExpeditionVM>
{
	[Header("Common")]
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Slider")]
	[SerializeField]
	private TextMeshProUGUI m_PeopleCountLabel;

	[SerializeField]
	private TextMeshProUGUI m_MaxPeopleCountLabel;

	[SerializeField]
	private Slider m_PeopleCountSlider;

	[Header("Rewards")]
	[SerializeField]
	private TextMeshProUGUI m_RewardsDescLabel;

	[SerializeField]
	private CanvasGroup[] m_RewardGroups;

	[Header("Button")]
	[SerializeField]
	private OwlcatButton m_SendExpeditionButton;

	[SerializeField]
	private TextMeshProUGUI m_SendExpeditionButtonLabel;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_HeaderLabel.text = UIStrings.Instance.ExplorationTexts.ExpeditionHeader;
		m_RewardsDescLabel.text = UIStrings.Instance.ExplorationTexts.ExpeditionRewardsDescription;
		m_SendExpeditionButtonLabel.text = UIStrings.Instance.ExplorationTexts.ExpeditionSendButtonLabel;
		m_PeopleCountSlider.minValue = 1f;
		m_PeopleCountSlider.wholeNumbers = true;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(SetVisibility));
		AddDisposable(base.ViewModel.UnlockedRewardIndex.Subscribe(SetRewards));
		AddDisposable(base.ViewModel.PeopleCount.Subscribe(delegate(int val)
		{
			m_PeopleCountLabel.text = val.ToString();
		}));
		AddDisposable(base.ViewModel.MaxPeopleCount.Subscribe(SetMaxPeopleCount));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleCloseClick();
		}));
		AddDisposable(m_SendExpeditionButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			HandleSendExpeditionClick();
		}));
		AddDisposable(m_PeopleCountSlider.OnValueChangedAsObservable().Subscribe(OnSliderChangedValue));
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void HandleCloseClick()
	{
		base.ViewModel.Hide();
	}

	private void HandleSendExpeditionClick()
	{
		base.ViewModel.SendExpedition();
	}

	private void OnSliderChangedValue(float value)
	{
		base.ViewModel.SetPeopleCount((int)value);
	}

	private void SetVisibility(bool value)
	{
		if (value)
		{
			m_FadeAnimator.AppearAnimation();
			return;
		}
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void SetRewards(int index)
	{
		for (int i = 0; i < m_RewardGroups.Length; i++)
		{
			m_RewardGroups[i].alpha = ((index >= i) ? 1f : 0.5f);
		}
	}

	private void SetMaxPeopleCount(int value)
	{
		m_PeopleCountSlider.maxValue = value;
		m_MaxPeopleCountLabel.text = value.ToString();
	}
}
