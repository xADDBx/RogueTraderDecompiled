using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsBaseView : ViewBase<ColonyProjectsVM>, IColonyProjectsUpdatedHandler, ISubscriber
{
	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	protected InputLayer m_InputLayer;

	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer m_PageInputLayer;

	protected GridConsoleNavigationBehaviour m_PageNavigationBehavior;

	protected readonly BoolReactiveProperty m_PageMode = new BoolReactiveProperty();

	protected bool m_ShowTooltip;

	private bool m_IsInputLayerPushed;

	public void Initialize()
	{
		m_HeaderLabel.text = UIStrings.Instance.ColonyProjectsTexts.HeaderDefault.Text;
		InitializeImpl();
	}

	protected virtual void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_PageNavigationBehavior = new GridConsoleNavigationBehaviour());
		CreateInput();
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(SetVisibility));
		AddDisposable(m_PageNavigationBehavior.DeepestFocusAsObservable.Subscribe(OnPageFocusChanged));
	}

	protected override void DestroyViewImplementation()
	{
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
		GamePad.Instance.PushLayer(m_InputLayer);
		m_IsInputLayerPushed = true;
	}

	private void Hide()
	{
		base.gameObject.SetActive(value: false);
		if (m_IsInputLayerPushed)
		{
			GamePad.Instance.PopLayer(m_InputLayer);
			m_IsInputLayerPushed = false;
		}
		TooltipHelper.HideTooltip();
		m_PageMode.Value = false;
		m_ShowTooltip = false;
	}

	private void OnPageFocusChanged(IConsoleEntity entity)
	{
		OnPageFocusChangedImpl(entity);
	}

	protected virtual void OnPageFocusChangedImpl(IConsoleEntity entity)
	{
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		UpdateNavigationImpl();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	protected virtual void UpdateNavigationImpl()
	{
	}

	private void CreateInput()
	{
		string contextName = (base.ViewModel.IsColonyManagement ? "ColonyProjectsManagement" : "ColonyProjects");
		string contextName2 = (base.ViewModel.IsColonyManagement ? "ColonyProjectsPageManagement" : "ColonyProjectsPage");
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = contextName
		});
		m_PageInputLayer = m_PageNavigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = contextName2
		});
		CreateInputImpl();
	}

	protected virtual void CreateInputImpl()
	{
	}

	protected void HandleClose()
	{
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIClose();
		});
	}

	protected void HandleStartProject()
	{
		base.ViewModel.StartProject();
	}

	protected void HandleShowBlockedProjects()
	{
		base.ViewModel.SwitchBlockedProjects();
	}

	protected void HandleShowFinishedProjects()
	{
		base.ViewModel.SwitchFinishedProjects();
	}

	public void HandleColonyProjectsUpdated()
	{
		UpdateNavigation();
	}

	protected void ShowPage()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_PageMode.Value = true;
		SetPageNavigation();
		GamePad.Instance.PushLayer(m_PageInputLayer);
		m_PageNavigationBehavior.FocusOnFirstValidEntity();
	}

	protected void ClosePage()
	{
		m_PageNavigationBehavior.UnFocusCurrentEntity();
		m_PageMode.Value = false;
		m_ShowTooltip = false;
		TooltipHelper.HideTooltip();
		GamePad.Instance.PopLayer(m_PageInputLayer);
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void SetPageNavigation()
	{
		m_PageNavigationBehavior.Clear();
		SetPageNavigationImpl();
		if (m_PageMode.Value)
		{
			TooltipHelper.HideTooltip();
		}
	}

	protected virtual void SetPageNavigationImpl()
	{
	}
}
