using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
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

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoRewardsBaseView : ViewBase<CargoRewardsVM>, IDialogNavigationCreatedHandler, ISubscriber, IInitializable, IGameModeHandler
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_LootRewardsLabel;

	[SerializeField]
	protected WidgetListMVVM m_WidgetListCargoes;

	[SerializeField]
	private CargoRewardSlotView m_CargoRewardSlotPrefab;

	protected InputLayer m_InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private bool m_IsInputLayerPushed;

	private bool m_IsWaitingForDialogNavigationBuild;

	protected bool m_ShowTooltip;

	public void Initialize()
	{
		m_HeaderLabel.text = UIStrings.Instance.CargoTexts.CargoRewardsHeader;
		m_LootRewardsLabel.text = UIStrings.Instance.ColonyProjectsRewards.LootRewardsHeader;
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		DrawCargoes();
		AddDisposable(base.ViewModel.UpdateCargo.Subscribe(DrawCargoes));
		CreateInput();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnPageFocusChanged));
		if (!m_IsWaitingForDialogNavigationBuild)
		{
			Show();
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	protected void HandleComplete()
	{
		Hide();
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.Rewards.CargoRewardsShowWindow.Play();
		GamePad.Instance.PushLayer(m_InputLayer);
		m_IsInputLayerPushed = true;
		m_IsWaitingForDialogNavigationBuild = false;
		m_ShowTooltip = false;
	}

	private void Hide()
	{
		UISounds.Instance.Sounds.Rewards.CargoRewardsHideWindow.Play();
		base.gameObject.SetActive(value: false);
		if (m_IsInputLayerPushed)
		{
			GamePad.Instance.PopLayer(m_InputLayer);
			m_IsInputLayerPushed = false;
		}
		TooltipHelper.HideTooltip();
		base.ViewModel.Close();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow(m_WidgetListCargoes.GetNavigationEntities());
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CargoRewards"
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

	private void DrawCargoes()
	{
		m_WidgetListCargoes.DrawEntries(base.ViewModel.CargoRewards, m_CargoRewardSlotPrefab);
		CreateNavigation();
	}

	public void HandleDialogNavigationBuildStarted()
	{
		m_IsWaitingForDialogNavigationBuild = true;
	}

	public void HandleDialogNavigationBuildFinished()
	{
		if (m_IsWaitingForDialogNavigationBuild)
		{
			m_IsWaitingForDialogNavigationBuild = false;
			Show();
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (!(gameMode != GameModeType.Dialog) && m_IsWaitingForDialogNavigationBuild)
		{
			Show();
		}
	}
}
