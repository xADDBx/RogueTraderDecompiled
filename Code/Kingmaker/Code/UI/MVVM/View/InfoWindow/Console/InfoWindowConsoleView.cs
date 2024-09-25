using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;

public sealed class InfoWindowConsoleView : InfoWindowBaseView
{
	public static readonly string InputLayerContextName = "InfoWindowConsoleView";

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private TooltipConfig m_TooltipConfig;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_IsShowTooltip = new ReactiveProperty<bool>();

	private IReadOnlyReactiveProperty<bool> m_HasTooltip;

	public BoolReactiveProperty IsWindowOpen = new BoolReactiveProperty();

	private RectTransform m_CurrentFocusedRect;

	private Vector3 m_LastPosition;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		Game.Instance.RequestPauseUi(isPaused: true);
		m_TooltipConfig.IsGlossary = true;
		m_IsShowTooltip.Value = false;
		AddDisposable(m_IsShowTooltip.Skip(1).Subscribe(ShowTooltip));
		DelayedInvoker.InvokeInFrames(CreateNavigation, 2);
		AddDisposable(GamePad.Instance.OnLayerPoped.Subscribe(OnCurrentInputLayerChanged));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		Game.Instance.RequestPauseUi(isPaused: false);
		m_LastPosition = base.transform.localPosition;
		m_ConsoleHintsWidget.Dispose();
		GamePad.Instance.BaseLayer?.Bind();
		TooltipHelper.HideTooltip();
	}

	private void CreateNavigation()
	{
		if (RootUIContext.Instance.IsBugReportOpen)
		{
			TooltipHelper.HideInfo();
			return;
		}
		IsWindowOpen.Value = true;
		AddDisposable(m_NavigationBehaviour = GetNavigationBehaviour());
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused));
		m_HasTooltip = m_NavigationBehaviour.DeepestFocusAsObservable.Select((IConsoleEntity f) => (f as IHasTooltipTemplate)?.TooltipTemplate() != null).ToReactiveProperty();
		CreateInput();
	}

	public void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		AddDisposable(m_InputLayer.AddAxis(Scroll, 3));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Back));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(SwitchShowTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
			ShowInfo(value: true);
		}, 8, m_HasTooltip);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Expand));
		AddDisposable(inputBindStruct3);
		foreach (IConsoleInputHandler item in Bricks.Where((MonoBehaviour b) => b is IConsoleInputHandler).Cast<IConsoleInputHandler>())
		{
			item.AddInputTo(m_InputLayer, m_ConsoleHintsWidget, m_NavigationBehaviour);
		}
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		GamePad.Instance.BaseLayer?.Unbind();
	}

	private void SwitchShowTooltip(InputActionEventData data)
	{
		m_IsShowTooltip.Value = !RootUIContext.Instance.TooltipIsShown;
	}

	protected override void OnClose()
	{
		base.OnClose();
		if (RootUIContext.Instance.TooltipIsShown)
		{
			m_IsShowTooltip.Value = false;
			return;
		}
		IsWindowOpen.Value = false;
		base.ViewModel.OnClose();
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_ScrollRect.Scroll(value, smooth: true);
		if (Game.Instance.Player.IsShowConsoleTooltip)
		{
			m_IsShowTooltip.Value = false;
		}
		if (m_CurrentFocusedRect != null && !m_ScrollRect.IsInViewport(m_CurrentFocusedRect))
		{
			m_NavigationBehaviour?.UnFocusCurrentEntity();
		}
	}

	private void OnEntityFocused(IConsoleEntity entity)
	{
		RectTransform rectTransform = (m_CurrentFocusedRect = ((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform);
		if (!m_ScrollRect.IsInViewport(rectTransform) && (bool)rectTransform)
		{
			m_ScrollRect.EnsureVisibleVertical(rectTransform.transform as RectTransform, 50f);
		}
		ShowTooltip(m_IsShowTooltip.Value);
	}

	private void ShowTooltip(bool value)
	{
		IConsoleEntity value2 = m_NavigationBehaviour.DeepestFocusAsObservable.Value;
		if (value2 == null || !value)
		{
			TooltipHelper.HideTooltip();
			return;
		}
		MonoBehaviour component = (value2 as MonoBehaviour) ?? (value2 as IMonoBehaviour)?.MonoBehaviour;
		TooltipBaseTemplate template = (value2 as IHasTooltipTemplate)?.TooltipTemplate();
		component.ShowConsoleTooltip(template, m_NavigationBehaviour, m_TooltipConfig);
	}

	private void ShowInfo(bool value)
	{
		IConsoleEntity value2 = m_NavigationBehaviour.DeepestFocusAsObservable.Value;
		if (value2 == null || !value)
		{
			TooltipHelper.HideInfo();
			return;
		}
		TooltipBaseTemplate tooltipBaseTemplate = (value2 as IHasTooltipTemplate)?.TooltipTemplate();
		if (tooltipBaseTemplate is TooltipTemplateGlossary { GlossaryEntry: not null } tooltipTemplateGlossary)
		{
			TooltipHelper.ShowGlossaryInfo(tooltipTemplateGlossary, m_NavigationBehaviour);
		}
		else
		{
			TooltipHelper.ShowInfo(tooltipBaseTemplate, m_NavigationBehaviour);
		}
	}

	protected override void SetPosition()
	{
		if (m_IsStartPosition)
		{
			base.transform.localPosition = m_Position;
		}
		else
		{
			base.transform.localPosition = m_LastPosition;
		}
	}

	private void OnCurrentInputLayerChanged()
	{
		if (GamePad.Instance.CurrentInputLayer != m_InputLayer)
		{
			return;
		}
		foreach (IConsoleInputHandler item in Bricks.Where((MonoBehaviour b) => b is IConsoleInputHandler).Cast<IConsoleInputHandler>())
		{
			item.UpdateTooltipBrick();
		}
	}
}
