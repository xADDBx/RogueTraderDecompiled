using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.TwitchDrops;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.TwitchDrops;

public class TwitchDropsRewardsBaseView : ViewBase<TwitchDropsRewardsVM>, IInitializable
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_LootRewardsLabel;

	[SerializeField]
	protected ItemSlotsGroupView m_SlotsGroup;

	[SerializeField]
	private RectTransform m_StatusBlock;

	[SerializeField]
	private TextMeshProUGUI m_StatusLabel;

	[SerializeField]
	private RectTransform m_WaitingAnimation;

	protected InputLayer m_InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private bool m_IsInputLayerPushed;

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
		AddDisposable(base.ViewModel.IsAwaiting.Subscribe(m_WaitingAnimation.gameObject.SetActive));
		AddDisposable(base.ViewModel.HasItems.Subscribe(m_SlotsGroup.gameObject.SetActive));
		AddDisposable(base.ViewModel.HasStatus.Subscribe(m_StatusBlock.gameObject.SetActive));
		AddDisposable(base.ViewModel.StatusText.Subscribe(delegate(string status)
		{
			m_StatusLabel.text = status;
		}));
		DrawItems();
		AddDisposable(base.ViewModel.UpdateRewards.Subscribe(DrawItems));
		CreateInput();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnPageFocusChanged));
		Show();
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
		m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(m_SlotsGroup.GetNavigation());
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "ItemRewards"
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

	private void DrawItems()
	{
		m_SlotsGroup.Bind(base.ViewModel.SlotsGroup);
		CreateNavigation();
	}
}
