using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
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

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyRewardsBaseView : ViewBase<ColonyRewardsVM>
{
	[Header("Colony reward")]
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private GameObject m_ColonyRewardBlock;

	[Header("Finished project")]
	[SerializeField]
	private GameObject m_FinishedProjectBlock;

	[SerializeField]
	private TextMeshProUGUI m_ProjectIsFinishedLabel;

	[SerializeField]
	private TextMeshProUGUI m_FinishedProjectNameLabel;

	[SerializeField]
	private Image m_FinishedProjectIcon;

	[Header("Stat rewards this colony")]
	[SerializeField]
	private GameObject m_StatsBlock;

	[SerializeField]
	private TextMeshProUGUI m_ColonyNameLabel;

	[SerializeField]
	private TextMeshProUGUI m_EfficiencyStatValue;

	[SerializeField]
	private TextMeshProUGUI m_ContentmentStatValue;

	[SerializeField]
	private TextMeshProUGUI m_SecurityStatValue;

	[Header("Stat rewards all colonies")]
	[SerializeField]
	private GameObject m_StatsAllColoniesBlock;

	[SerializeField]
	private TextMeshProUGUI m_AllColoniesLabel;

	[SerializeField]
	private TextMeshProUGUI m_EfficiencyStatAllColoniesValue;

	[SerializeField]
	private TextMeshProUGUI m_ContentmentStatAllColoniesValue;

	[SerializeField]
	private TextMeshProUGUI m_SecurityStatAllColoniesValue;

	[Header("Loot rewards")]
	[SerializeField]
	private TextMeshProUGUI m_LootRewardsLabel;

	[SerializeField]
	private GameObject m_LootBlock;

	[SerializeField]
	private GameObject m_ItemsSubBlock;

	[SerializeField]
	protected ItemSlotsGroupView m_SlotsGroup;

	[SerializeField]
	private GameObject m_CargoSubBlock;

	[SerializeField]
	private GameObject m_CargoSeparator;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListCargoes;

	[SerializeField]
	private CargoRewardSlotView m_CargoRewardSlotPrefab;

	[SerializeField]
	private GameObject m_OtherRewardsSubBlock;

	[SerializeField]
	private GameObject m_OtherRewardsSeparator;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListOtherRewards;

	[SerializeField]
	private ColonyRewardsOtherRewardView m_OtherRewardPrefab;

	protected InputLayer m_InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private bool m_IsInputLayerPushed;

	protected bool m_ShowTooltip;

	public void Initialize()
	{
		m_HeaderLabel.text = UIStrings.Instance.ColonyProjectsRewards.ColonyRewardsHeader;
		m_ProjectIsFinishedLabel.text = UIStrings.Instance.ColonyProjectsTexts.ProjectIsFinished;
		m_AllColoniesLabel.text = UIStrings.Instance.ColonyProjectsRewards.AllColonies;
		m_LootRewardsLabel.text = UIStrings.Instance.ColonyProjectsRewards.LootRewardsHeader;
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UpdateRewards.Subscribe(UpdateRewards));
		AddDisposable(base.ViewModel.HasFinishedProject.Subscribe(m_FinishedProjectBlock.SetActive));
		AddDisposable(base.ViewModel.FinishedProjectName.Subscribe(delegate(string val)
		{
			m_FinishedProjectNameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.FinishedProjectIcon.Subscribe(delegate(Sprite val)
		{
			m_FinishedProjectIcon.sprite = val;
		}));
		AddDisposable(base.ViewModel.HasStats.Subscribe(m_StatsBlock.SetActive));
		AddDisposable(base.ViewModel.ColonyName.Subscribe(delegate(string val)
		{
			m_ColonyNameLabel.text = val;
		}));
		AddDisposable(base.ViewModel.EfficiencyStatText.Subscribe(delegate(string val)
		{
			SetStatReward(m_EfficiencyStatValue, val);
		}));
		AddDisposable(base.ViewModel.ContentmentStatText.Subscribe(delegate(string val)
		{
			SetStatReward(m_ContentmentStatValue, val);
		}));
		AddDisposable(base.ViewModel.SecurityStatText.Subscribe(delegate(string val)
		{
			SetStatReward(m_SecurityStatValue, val);
		}));
		AddDisposable(base.ViewModel.HasStatsAllColonies.Subscribe(m_StatsAllColoniesBlock.SetActive));
		AddDisposable(base.ViewModel.EfficiencyStatAllColoniesText.Subscribe(delegate(string val)
		{
			SetStatReward(m_EfficiencyStatAllColoniesValue, val);
		}));
		AddDisposable(base.ViewModel.ContentmentStatAllColoniesText.Subscribe(delegate(string val)
		{
			SetStatReward(m_ContentmentStatAllColoniesValue, val);
		}));
		AddDisposable(base.ViewModel.SecurityStatAllColoniesText.Subscribe(delegate(string val)
		{
			SetStatReward(m_SecurityStatAllColoniesValue, val);
		}));
		AddDisposable(base.ViewModel.HasFinishedProject.CombineLatest(base.ViewModel.HasStats, base.ViewModel.HasStatsAllColonies, (bool hasFinishedProject, bool hasStats, bool hasStatsAllColonies) => hasFinishedProject || hasStats || hasStatsAllColonies).Subscribe(m_ColonyRewardBlock.SetActive));
		AddDisposable(base.ViewModel.HasItems.CombineLatest(base.ViewModel.HasCargo, base.ViewModel.HasOtherRewards, (bool hasItems, bool hasCargo, bool hasOtherRewards) => hasItems || hasCargo || hasOtherRewards).Subscribe(m_LootBlock.SetActive));
		AddDisposable(base.ViewModel.HasItems.Subscribe(delegate(bool val)
		{
			m_ItemsSubBlock.SetActive(val);
			m_CargoSeparator.SetActive(val);
			m_OtherRewardsSeparator.SetActive(val);
		}));
		AddDisposable(base.ViewModel.HasCargo.Subscribe(delegate(bool val)
		{
			m_CargoSubBlock.SetActive(val);
			m_OtherRewardsSeparator.SetActive(val);
		}));
		AddDisposable(base.ViewModel.HasOtherRewards.Subscribe(m_OtherRewardsSubBlock.SetActive));
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		UpdateRewards();
		CreateInput();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnPageFocusChanged));
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(SetVisibility));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void SetVisibility(bool isVisible)
	{
		if (isVisible)
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
		UISounds.Instance.Sounds.Rewards.ColonyRewardsShowWindow.Play();
		GamePad.Instance.PushLayer(m_InputLayer);
		m_IsInputLayerPushed = true;
		m_ShowTooltip = false;
	}

	private void Hide()
	{
		UISounds.Instance.Sounds.Rewards.ColonyRewardsHideWindow.Play();
		base.gameObject.SetActive(value: false);
		if (m_IsInputLayerPushed)
		{
			GamePad.Instance.PopLayer(m_InputLayer);
			m_IsInputLayerPushed = false;
		}
		TooltipHelper.HideTooltip();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_SlotsGroup.GetNavigation());
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.SetEntitiesHorizontal(m_WidgetListCargoes.GetNavigationEntities());
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour);
		m_NavigationBehaviour.AddColumn(m_WidgetListOtherRewards.GetNavigationEntities());
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ColonyRewards"
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

	private void UpdateRewards()
	{
		m_SlotsGroup.Bind(base.ViewModel.SlotsGroup);
		DrawCargoes();
		DrawOtherRewards();
		CreateNavigation();
	}

	protected void HandleComplete()
	{
		base.ViewModel.HandleHide();
	}

	private void SetStatReward(TextMeshProUGUI label, string text)
	{
		label.text = text;
		label.gameObject.SetActive(!string.IsNullOrEmpty(text));
	}

	private void DrawCargoes()
	{
		m_WidgetListCargoes.DrawEntries(base.ViewModel.CargoRewards, m_CargoRewardSlotPrefab);
	}

	private void DrawOtherRewards()
	{
		m_WidgetListOtherRewards.DrawEntries(base.ViewModel.OtherRewards, m_OtherRewardPrefab);
	}
}
