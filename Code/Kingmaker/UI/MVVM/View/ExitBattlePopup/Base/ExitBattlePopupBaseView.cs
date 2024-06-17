using System.Collections;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ExitBattlePopup;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ExitBattlePopup.Base;

public class ExitBattlePopupBaseView : ViewBase<ExitBattlePopupVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ExitBattleTitleLabel;

	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Exp")]
	[SerializeField]
	protected Image m_ExperiencePanel;

	[SerializeField]
	private TextMeshProUGUI m_ExpLabel;

	[SerializeField]
	private TextMeshProUGUI m_CurrentLevel;

	[SerializeField]
	private TextMeshProUGUI m_GainedExpAmount;

	[SerializeField]
	private TextMeshProUGUI m_CurrentExpAmount;

	[SerializeField]
	private TextMeshProUGUI m_NextLevelExpAmount;

	[SerializeField]
	private Slider m_ExpMaxSlider;

	[SerializeField]
	private DelayedSlider m_ExpCurrentSlider;

	[SerializeField]
	private Slider m_FakeExpMaxSlider;

	[SerializeField]
	private DelayedSlider m_FakeExpCurrentSlider;

	[SerializeField]
	private Image m_UpgradeAvailableIcon;

	[Header("Loot rewards")]
	[SerializeField]
	private TextMeshProUGUI m_LootRewardsLabel;

	[SerializeField]
	private GameObject m_LootBlock;

	[SerializeField]
	private GameObject m_ItemsSubBlock;

	[SerializeField]
	protected ItemSlotsGroupView m_ItemsSlotsGroup;

	[SerializeField]
	private GridLayoutGroup m_ItemsGridLayoutGroup;

	[SerializeField]
	private GameObject m_CargoSubBlock;

	[SerializeField]
	private GameObject m_CargoSeparator;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListCargoes;

	[SerializeField]
	private GridLayoutGroup m_CargoGridLayoutGroup;

	[SerializeField]
	private CargoRewardSlotView m_CargoRewardSlotPrefab;

	[Header("Scrap")]
	[SerializeField]
	private ScrapRewardSlotView m_ScrapRewardSlotView;

	protected InputLayer m_InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private bool m_IsInputLayerPushed;

	protected Tweener m_ExpValueTweener;

	protected bool m_ShowTooltip;

	public void Initialize()
	{
		m_Animator.Initialize();
		m_ScrapRewardSlotView.Initialize();
		m_ExitBattleTitleLabel.text = UIStrings.Instance.SpaceCombatTexts.ExitBattle;
		m_ExpLabel.text = UIStrings.Instance.ShipCustomization.XP;
		m_LootRewardsLabel.text = UIStrings.Instance.ColonyProjectsRewards.LootRewardsHeader;
		m_ExpCurrentSlider.Initialize();
		m_FakeExpCurrentSlider.Initialize();
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		m_ScrapRewardSlotView.Bind(base.ViewModel.ScrapVM);
		AddDisposable(base.ViewModel.IsActive.Subscribe(delegate(bool value)
		{
			if (value)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}));
		AddDisposable(base.ViewModel.GainedExpAmount.Subscribe(SetGainedExpAmount));
		AddDisposable(base.ViewModel.NextLevelExp.Subscribe(delegate(int value)
		{
			m_ExpMaxSlider.maxValue = value;
			m_NextLevelExpAmount.text = "/ " + value;
		}));
		AddDisposable(base.ViewModel.PrevExp.Subscribe(delegate(int value)
		{
			m_ExpMaxSlider.minValue = value;
		}));
		AddDisposable(base.ViewModel.CurrentExp.Subscribe(PlayExpAnimation));
		AddDisposable(base.ViewModel.IsUpgradeAvailable.Subscribe(SetUpgradeAvailable));
		AddDisposable(base.ViewModel.HasItems.CombineLatest(base.ViewModel.HasCargo, (bool hasItems, bool hasCargo) => hasItems || hasCargo).Subscribe(m_LootBlock.SetActive));
		AddDisposable(base.ViewModel.HasItems.Subscribe(delegate(bool val)
		{
			m_ItemsSubBlock.SetActive(val);
			m_CargoSeparator.SetActive(val);
			m_CargoGridLayoutGroup.childAlignment = ((!val) ? TextAnchor.UpperCenter : TextAnchor.UpperLeft);
		}));
		AddDisposable(base.ViewModel.HasCargo.Subscribe(delegate(bool val)
		{
			m_CargoSubBlock.SetActive(val);
			m_ItemsGridLayoutGroup.childAlignment = ((!val) ? TextAnchor.UpperCenter : TextAnchor.UpperRight);
		}));
		AddDisposable(m_ExperiencePanel.SetTooltip(new TooltipTemplateSimple(UIStrings.Instance.Tooltips.CurrentLevelExperience, UIStrings.Instance.ShipCustomization.ShipExperienceDescription)));
		AddDisposable(base.ViewModel.UpdateRewards.Subscribe(UpdateRewards));
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateNavigation();
		CreateInput();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnPageFocusChanged));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_ExpValueTweener?.Kill();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		UISounds.Instance.Sounds.Combat.ExitBattlePopupShow.Play();
		GamePad.Instance.PushLayer(m_InputLayer);
		m_IsInputLayerPushed = true;
	}

	private void Hide()
	{
		m_ShowTooltip = false;
		UISounds.Instance.Play(UISounds.Instance.Sounds.Combat.ExitBattlePopupExperienceGrowStop, isButton: false, playAnyway: true);
		m_Animator.DisappearAnimation();
		if (m_IsInputLayerPushed)
		{
			GamePad.Instance.PopLayer(m_InputLayer);
			m_IsInputLayerPushed = false;
		}
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_ItemsSlotsGroup.GetNavigation());
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.SetEntitiesGrid(m_WidgetListCargoes.GetNavigationEntities(), m_CargoGridLayoutGroup.constraintCount);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour);
		m_NavigationBehaviour.AddRow<ScrapRewardSlotView>(m_ScrapRewardSlotView);
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SpaceCombatRewards"
		}, null, leftStick: true, rightStick: true);
		CreateInputImpl();
	}

	protected virtual void CreateInputImpl()
	{
	}

	private void OnPageFocusChanged(IConsoleEntity entity)
	{
		OnPageFocusChangedImpl(entity);
	}

	protected virtual void OnPageFocusChangedImpl(IConsoleEntity entity)
	{
	}

	private void SetGainedExpAmount(int amount)
	{
		m_GainedExpAmount.text = "+" + amount + " XP";
	}

	private void PlayExpAnimation(int expValue)
	{
		m_CurrentLevel.text = string.Format(UIStrings.Instance.CharacterSheet.RankLabel, base.ViewModel.CurrentLevel.Value);
		if (base.ViewModel.LevelDiff.Value < 1)
		{
			m_FakeExpMaxSlider.gameObject.SetActive(value: false);
			m_ExpMaxSlider.gameObject.SetActive(value: true);
			m_ExpCurrentSlider.SetValue(expValue);
		}
		else
		{
			m_FakeExpMaxSlider.gameObject.SetActive(value: true);
			m_ExpMaxSlider.gameObject.SetActive(value: false);
			m_FakeExpCurrentSlider.SetValue(base.ViewModel.ExpRatio.Value, showDelta: false);
			DelayedInvoker.InvokeInTime(delegate
			{
				PlayExpSliderAnimation(expValue);
			}, 2f);
		}
		m_ExpValueTweener?.Kill();
		m_ExpValueTweener = DOTween.To(() => base.ViewModel.PrevExp.Value, delegate(int x)
		{
			float in_value = (float)x / (float)base.ViewModel.NextLevelExp.Value;
			m_CurrentExpAmount.text = x.ToString();
			AkSoundEngine.SetRTPCValue("SpaceCombat_WinProgressBar", in_value);
		}, expValue, base.ViewModel.LevelDiff.Value + 1).ChangeStartValue(base.ViewModel.PrevExp.Value).SetDelay(2f)
			.SetUpdate(isIndependentUpdate: true)
			.OnPlay(delegate
			{
				UISounds.Instance.Sounds.Combat.ExitBattlePopupExperienceGrowStart.Play();
			})
			.OnComplete(delegate
			{
				UISounds.Instance.Play(UISounds.Instance.Sounds.Combat.ExitBattlePopupExperienceGrowStop, isButton: false, playAnyway: true);
			})
			.OnKill(delegate
			{
				UISounds.Instance.Play(UISounds.Instance.Sounds.Combat.ExitBattlePopupExperienceGrowStop, isButton: false, playAnyway: true);
			})
			.SetAutoKill(autoKillOnCompletion: true);
	}

	private void PlayExpSliderAnimation(int expValue)
	{
		StartCoroutine(SetSliderValuesCoroutine(expValue));
	}

	private IEnumerator SetSliderValuesCoroutine(int expValue)
	{
		int level = base.ViewModel.CurrentLevel.Value;
		for (int i = 0; i < base.ViewModel.LevelDiff.Value; i++)
		{
			m_FakeExpCurrentSlider.SetValue(1f, showDelta: true, noDelay: true);
			yield return new WaitForSeconds(m_FakeExpCurrentSlider.DeltaMoveTime);
			m_CurrentLevel.text = string.Format(UIStrings.Instance.CharacterSheet.RankLabel, level + i + 1);
			m_FakeExpCurrentSlider.SetValue(0f, showDelta: false);
		}
		m_FakeExpMaxSlider.gameObject.SetActive(value: false);
		m_ExpMaxSlider.gameObject.SetActive(value: true);
		m_ExpCurrentSlider.SetValue(expValue, showDelta: true, noDelay: true);
	}

	private void SetUpgradeAvailable(bool isAvailable)
	{
		Color color = m_UpgradeAvailableIcon.color;
		m_UpgradeAvailableIcon.color = new Color(color.r, color.g, color.b, isAvailable ? 1f : 0f);
	}

	private void UpdateRewards()
	{
		m_ItemsSlotsGroup.Bind(base.ViewModel.ItemsSlotsGroup);
		DrawCargoes();
		CreateNavigation();
	}

	private void DrawCargoes()
	{
		m_WidgetListCargoes.DrawEntries(base.ViewModel.CargoRewards, m_CargoRewardSlotPrefab);
	}
}
