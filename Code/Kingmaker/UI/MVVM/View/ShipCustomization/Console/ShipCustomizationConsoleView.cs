using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts.Console;
using Kingmaker.UI.MVVM.View.Space.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipCustomizationConsoleView : ShipCustomizationBaseView<ShipUpgradeConsoleView, ShipSkillsConsoleView, ShipHealthAndRepairConsoleView>, ICullFocusHandler, ISubscriber
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_NextConsoleHint;

	[SerializeField]
	private ConsoleHint m_PrevConsoleHint;

	private InputLayer m_UpgradeInputLayer;

	private InputLayer m_SkillsInputLayer;

	private InputLayer m_PostsInputLayer;

	private readonly BoolReactiveProperty m_CanSwitchView = new BoolReactiveProperty();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private CompositeDisposable m_InputLayerDisposable = new CompositeDisposable();

	private IConsoleHint m_ConfirmHint;

	private IConsoleHint m_Func01Hint;

	private readonly BoolReactiveProperty m_ConfirmEnabled = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_Func01Enabled = new BoolReactiveProperty();

	private IShipCustomizationPage m_CurrentPage;

	private IConsoleEntity m_CulledFocus;

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
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_InputLayerDisposable.Clear();
		m_UpgradeInputLayer = null;
		m_SkillsInputLayer = null;
		m_PostsInputLayer = null;
	}

	protected override void BindShip()
	{
		m_ShipHealthAndRepairView.Bind(base.ViewModel.ShipHealthAndRepairVM);
	}

	private void UpdateUpgradeNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_UpgradeInputLayer = null;
		m_UpgradeInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Ship Upgrade Console View"
		});
		InputBindStruct inputBindStruct = m_UpgradeInputLayer.AddButton(delegate
		{
			SetPrevTab();
		}, 14, m_CanSwitchView);
		AddDisposable(m_PrevConsoleHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_UpgradeInputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 15, m_CanSwitchView);
		AddDisposable(m_NextConsoleHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		m_ShipUpgradeView.AddInput(ref m_UpgradeInputLayer, ref m_HintsWidget);
		m_ShipHealthAndRepairView.AddInput(m_UpgradeInputLayer);
		InputBindStruct inputBindStruct3 = m_UpgradeInputLayer.AddButton(delegate
		{
			Close();
		}, 9);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct3);
		ShipConsoleView shipConsoleView = m_SpaceShipPCView as ShipConsoleView;
		ShipStatsConsoleView shipStatsConsoleView = m_ShipStatsPCView as ShipStatsConsoleView;
		if ((bool)shipStatsConsoleView && (bool)shipConsoleView)
		{
			ConsoleNavigationBehaviour navigation = m_ShipUpgradeView.GetNavigation(shipStatsConsoleView.GetNavigation(shipConsoleView.GetNavigation()));
			m_NavigationBehaviour.AddEntityGrid(navigation);
		}
		m_NavigationBehaviour.FocusOnEntityManual(m_ShipUpgradeView.NavigationBehaviour);
		m_InputLayerDisposable.Clear();
		m_InputLayerDisposable.Add(GamePad.Instance.PushLayer(m_UpgradeInputLayer));
	}

	private void UpdateSkillsNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_SkillsInputLayer = null;
		m_SkillsInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Ship Skills Console View"
		});
		InputBindStruct inputBindStruct = m_SkillsInputLayer.AddButton(delegate
		{
			SetPrevTab();
		}, 14, m_CanSwitchView);
		AddDisposable(m_PrevConsoleHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_SkillsInputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 15, m_CanSwitchView);
		AddDisposable(m_NextConsoleHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		m_ShipSkillsPCView.AddInput(ref m_SkillsInputLayer, ref m_HintsWidget);
		m_NavigationBehaviour.AddEntityGrid(m_ShipSkillsPCView.GetNavigationBehaviour());
		m_InputLayerDisposable.Clear();
		m_InputLayerDisposable.Add(GamePad.Instance.PushLayer(m_SkillsInputLayer));
	}

	private void UpdatePostsNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_PostsInputLayer = null;
		m_PostsInputLayer = new InputLayer
		{
			ContextName = "Ship Posts Console View"
		};
		m_PostsInputLayer = m_NavigationBehaviour.GetInputLayer(m_PostsInputLayer);
		InputBindStruct inputBindStruct = m_PostsInputLayer.AddButton(delegate
		{
			UpdateHints();
		}, 8, m_ConfirmEnabled);
		AddDisposable(m_ConfirmHint = m_HintsWidget.BindHint(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_PostsInputLayer.AddButton(delegate
		{
			UpdateHints();
		}, 10, m_Func01Enabled);
		AddDisposable(m_Func01Hint = m_HintsWidget.BindHint(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_PostsInputLayer.AddButton(delegate
		{
			Close();
		}, 9);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_PostsInputLayer.AddButton(delegate
		{
			SetPrevTab();
		}, 14, m_CanSwitchView);
		AddDisposable(m_PrevConsoleHint.Bind(inputBindStruct4));
		AddDisposable(inputBindStruct4);
		InputBindStruct inputBindStruct5 = m_PostsInputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 15, m_CanSwitchView);
		AddDisposable(m_NextConsoleHint.Bind(inputBindStruct5));
		AddDisposable(inputBindStruct5);
		m_NavigationBehaviour.AddEntityGrid(m_ShipPostsView.GetNavigationBehaviour());
		(m_ShipPostsView as PostsConsoleView)?.AddInput(ref m_PostsInputLayer, ref m_HintsWidget);
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		m_InputLayerDisposable.Clear();
		m_InputLayerDisposable.Add(GamePad.Instance.PushLayer(m_PostsInputLayer));
	}

	private void OnEntityFocused(IConsoleEntity entity)
	{
		UpdateHints();
		if (m_ShipUpgradeView.IsBinded)
		{
			m_CanSwitchView.Value = !m_ShipUpgradeView.IsRightWindow.Value;
		}
		else
		{
			m_CanSwitchView.Value = true;
		}
	}

	private void UpdateHints()
	{
		m_ConfirmHint?.SetLabel(m_NavigationBehaviour.DeepestNestedFocus.GetConfirmClickHint());
		m_ConfirmEnabled.Value = m_NavigationBehaviour.DeepestNestedFocus?.CanConfirmClick() ?? false;
		m_Func01Hint?.SetLabel(m_NavigationBehaviour.DeepestNestedFocus?.GetFunc01ClickHint());
		m_Func01Enabled.Value = m_NavigationBehaviour.DeepestNestedFocus?.CanFunc01Click() ?? false;
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
			UpdateUpgradeNavigation();
			m_CurrentPage = m_ShipUpgradeView;
			break;
		case ShipCustomizationTab.Skills:
			m_ShipUpgradeView.Unbind();
			m_ShipSkillsPCView.Bind(base.ViewModel.ShipSkillsVM);
			m_ShipPostsView.Unbind();
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipInfo.SetActive(value: false);
			UpdateSkillsNavigation();
			m_CanSwitchView.Value = true;
			m_CurrentPage = m_ShipSkillsPCView;
			break;
		case ShipCustomizationTab.Posts:
			m_ShipUpgradeView.Unbind();
			m_ShipSkillsPCView.Unbind();
			m_ShipPostsView.Bind(base.ViewModel.ShipPostsVM);
			m_SkillsAndPostsFadeAnimator.AppearAnimation();
			m_ShipInfo.SetActive(value: false);
			UpdatePostsNavigation();
			m_CanSwitchView.Value = true;
			m_CurrentPage = m_ShipHealthAndRepairView;
			break;
		}
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}

	protected override void Close()
	{
		if (!m_CurrentPage.CanOverrideClose())
		{
			base.Close();
		}
	}
}
