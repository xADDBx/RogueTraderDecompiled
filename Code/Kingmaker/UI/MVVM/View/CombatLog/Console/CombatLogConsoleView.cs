using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CombatLog.Console;

public class CombatLogConsoleView : CombatLogBaseView, ICullFocusHandler, ISubscriber
{
	[SerializeField]
	protected GameObject m_ChannelPanel;

	[SerializeField]
	private GameObject m_PinBackground;

	[SerializeField]
	private TextMeshProUGUI m_ChannelText;

	[SerializeField]
	private List<float> m_YSizesList;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_ChangeSizeHint;

	[SerializeField]
	private ConsoleHint m_ModePinHint;

	[SerializeField]
	private ConsoleHint m_ConsoleHintFilterPrev;

	[SerializeField]
	private ConsoleHint m_ConsoleHintFilterNext;

	[SerializeField]
	private ConsoleHint m_ConsoleHintClose;

	[SerializeField]
	private ConsoleHint m_ConsoleHintOpen;

	[SerializeField]
	private ConsoleHint m_ConsoleHintOpenCombat;

	[SerializeField]
	private ConsoleHint m_ConsoleHintOpenExploration;

	[SerializeField]
	private ConsoleHint m_MoveCameraToHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private readonly BoolReactiveProperty m_ShowModePin = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowMoveCameraTo = new BoolReactiveProperty();

	private bool m_InputLayerActivated;

	private TooltipConfig m_TooltipConfig;

	private IConsoleEntity m_CulledFocus;

	public bool HoldCombatLog => Game.Instance.Player.UISettings.HoldCombatLog;

	private int CurrentSizeIndex
	{
		get
		{
			if (base.ViewModel.CurrentSizeIndex.Value >= m_YSizesList.Count)
			{
				base.ViewModel.CurrentSizeIndex.Value = m_YSizesList.Count - 1;
			}
			else if (base.ViewModel.CurrentSizeIndex.Value < 0)
			{
				base.ViewModel.CurrentSizeIndex.Value = 0;
			}
			return base.ViewModel.CurrentSizeIndex.Value;
		}
		set
		{
			if (value >= m_YSizesList.Count)
			{
				base.ViewModel.CurrentSizeIndex.Value = 0;
			}
			else if (value < 0)
			{
				base.ViewModel.CurrentSizeIndex.Value = m_YSizesList.Count - 1;
			}
			else
			{
				base.ViewModel.CurrentSizeIndex.Value = value;
			}
			Game.Instance.Player.UISettings.CombatLogSizeIndex = base.ViewModel.CurrentSizeIndex.Value;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_ChannelPanel.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetSizeDelta(Game.Instance.Player.UISettings.LogSizeConsole);
		AddDisposable(base.ViewModel.IsActive.Subscribe(OnVisible));
		AddDisposable(base.ViewModel.IsControlActive.Subscribe(delegate(bool value)
		{
			if (value)
			{
				ActivateControl();
			}
			else
			{
				DeactivateControl();
			}
		}));
		foreach (CombatLogToggleWithCustomHint toggle in m_Toggles)
		{
			toggle.SetCustomHint(base.ViewModel.GetChannelName(m_Toggles.IndexOf(toggle)));
		}
		AddDisposable(base.ViewModel.CurrentSizeIndex.Skip(1).Subscribe(OnSizeIndexChanged));
		base.ViewModel.IsActive.Value = HoldCombatLog;
		SetupNavigationBehaviour();
		SetupChannelText();
		SetupShowMode();
		SetupSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		DeactivateControl();
		m_InputLayer = null;
	}

	protected virtual void OnVisible(bool value)
	{
		IsPinned.Value = value;
	}

	private void SetupNavigationBehaviour()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_NavigationBehaviour.SetEntitiesVertical<GridConsoleNavigationBehaviour>(m_VirtualList.GetNavigationBehaviour());
		m_InputLayer = GetInputLayer(m_NavigationBehaviour);
	}

	private InputLayer GetInputLayer(ConsoleNavigationBehaviour navigationBehavior)
	{
		InputLayer inputLayer = navigationBehavior.GetInputLayer(new InputLayer
		{
			ContextName = "CombatLogConsoleViewInput"
		});
		AddDisposable(inputLayer.AddButton(delegate
		{
			OnBack();
		}, 9));
		AddDisposable(m_ChangeSizeHint.Bind(inputLayer.AddButton(OnChangeSize, 10)));
		m_ChangeSizeHint.SetLabel(UIStrings.Instance.CombatLog.ChangeSize.Text);
		AddDisposable(m_ModePinHint.Bind(inputLayer.AddButton(OnChangeShowMode, 11)));
		m_ModePinHint.SetLabel(UIStrings.Instance.CombatLog.ShowModePin.Text);
		AddDisposable(m_ConsoleHintFilterPrev.Bind(inputLayer.AddButton(delegate
		{
			OnChangeChannel(isPrev: true);
		}, 14)));
		AddDisposable(m_ConsoleHintFilterNext.Bind(inputLayer.AddButton(delegate
		{
			OnChangeChannel(isPrev: false);
		}, 15)));
		AddDisposable(m_ConsoleHintClose.Bind(inputLayer.AddButton(delegate
		{
			OnBack();
		}, 9)));
		AddDisposable(m_ShowModePin.Subscribe(delegate(bool value)
		{
			m_ModePinHint.SetLabel(value ? UIStrings.Instance.CombatLog.ShowModeUnpin.Text : UIStrings.Instance.CombatLog.ShowModePin.Text);
		}));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		AddDisposable(m_VirtualList.GetNavigationBehaviour().DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		AddDisposable(m_MoveCameraToHint.Bind(inputLayer.AddButton(delegate
		{
			OnMoveCameraTo();
		}, 8, m_ShowMoveCameraTo)));
		m_MoveCameraToHint.SetLabel(UIStrings.Instance.CombatLog.ShowUnit.Text);
		return inputLayer;
	}

	public override void SwitchPinnedState(bool pinned)
	{
		base.SwitchPinnedState(pinned);
		if (pinned)
		{
			SetContainerState(state: true);
		}
	}

	protected override void SetSizeDeltaImpl(Vector2 size)
	{
		m_PinnedContainer.sizeDelta = size;
		Game.Instance.Player.UISettings.LogSizeConsole = size;
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		if (entity is CombatLogItemConsoleView combatLogItemConsoleView)
		{
			m_TooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			};
			combatLogItemConsoleView.ShowConsoleTooltip(combatLogItemConsoleView.TooltipTemplate, m_NavigationBehaviour, m_TooltipConfig, shouldNotHideLittleTooltip: true);
			m_ShowMoveCameraTo.Value = combatLogItemConsoleView.CanConfirmClick();
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void OnMoveCameraTo()
	{
		if (m_NavigationBehaviour.DeepestNestedFocus is CombatLogItemConsoleView combatLogItemConsoleView)
		{
			combatLogItemConsoleView.OnConfirmClick();
		}
	}

	private void OnChangeChannel(bool isPrev)
	{
		CombatLogToggleWithCustomHint combatLogToggleWithCustomHint = m_ToggleGroup.ActiveToggles().FirstOrDefault() as CombatLogToggleWithCustomHint;
		if (!(combatLogToggleWithCustomHint == null))
		{
			int num = m_Toggles.IndexOf(combatLogToggleWithCustomHint) + (isPrev ? 1 : (-1));
			if (num < 0)
			{
				num = m_Toggles.Count - 1;
			}
			else if (num >= m_Toggles.Count)
			{
				num = 0;
			}
			m_Toggles[num].Set(value: true);
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
			OnToggleClick();
			SetupChannelText();
			m_VirtualList.ScrollController.ForceScrollToBottom();
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VirtualList.GetNavigationBehaviour().FocusOnLastValidEntity();
			}, 3);
		}
	}

	private void OnToggleClick()
	{
		CombatLogToggleWithCustomHint combatLogToggleWithCustomHint = m_ToggleGroup.ActiveToggles().FirstOrDefault() as CombatLogToggleWithCustomHint;
		if (!(combatLogToggleWithCustomHint == null))
		{
			CurrentSelectedIndex = m_Toggles.IndexOf(combatLogToggleWithCustomHint);
			base.ViewModel.SetCurrentChannelById(CurrentSelectedIndex);
		}
	}

	private void SetupChannelText()
	{
		if (m_ChannelText != null)
		{
			m_ChannelText.text = m_ToggleTexts[CurrentSelectedIndex].text;
		}
	}

	private void OnBack()
	{
		if (base.ViewModel.IsControlActive.Value)
		{
			base.ViewModel.CombatLogChangeState();
		}
		if (!HoldCombatLog)
		{
			base.ViewModel.Deactivate();
		}
		TooltipHelper.HideTooltip();
	}

	private void ActivateControl()
	{
		if (!m_InputLayerActivated)
		{
			m_InputLayerActivated = true;
			m_ChannelPanel.gameObject.SetActive(value: true);
			m_VirtualList.ScrollController.ForceScrollToBottom();
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VirtualList.GetNavigationBehaviour().FocusOnLastValidEntity();
			}, 3);
			GamePad.Instance.PushLayer(m_InputLayer);
		}
	}

	private void DeactivateControl()
	{
		if (m_InputLayerActivated)
		{
			m_InputLayerActivated = false;
			m_ChannelPanel.gameObject.SetActive(value: false);
			m_VirtualList.ScrollController.ForceScrollToBottom();
			m_NavigationBehaviour.UnFocusCurrentEntity();
			GamePad.Instance.PopLayer(m_InputLayer);
		}
	}

	private void UpdateEntities()
	{
		if (!(m_VirtualList.GetNavigationBehaviour().CurrentEntity is VirtualListElement virtualListElement))
		{
			return;
		}
		m_VirtualList.ScrollController.ForceScrollToElement(virtualListElement.Data);
		foreach (VirtualListElement visibleElement in m_VirtualList.VisibleElements)
		{
			visibleElement.SetFocus(virtualListElement == visibleElement);
		}
	}

	private void UpdateEntitiesFocus()
	{
		foreach (VirtualListElement visibleElement in m_VirtualList.VisibleElements)
		{
			visibleElement.SetFocus(m_NavigationBehaviour.CurrentEntity == visibleElement);
		}
	}

	private void OnChangeShowMode(InputActionEventData obj)
	{
		Game.Instance.Player.UISettings.HoldCombatLog = !HoldCombatLog;
		SetupShowMode();
	}

	private void SetupShowMode()
	{
		if (m_PinBackground != null)
		{
			m_PinBackground.SetActive(HoldCombatLog);
		}
		m_ShowModePin.Value = HoldCombatLog;
	}

	private void SetupSize()
	{
		m_PinnedContainer.sizeDelta = new Vector2(m_PinnedContainer.sizeDelta.x, m_YSizesList[CurrentSizeIndex]);
	}

	private void OnChangeSize(InputActionEventData obj)
	{
		UISounds.Instance.Sounds.CombatLog.CombatLogSizeChanged.Play();
		CurrentSizeIndex++;
	}

	public void OnSizeIndexChanged(int index)
	{
		Tweener t = m_PinnedContainer.DOSizeDelta(new Vector2(m_PinnedContainer.sizeDelta.x, m_YSizesList[CurrentSizeIndex]), 0.2f).SetUpdate(isIndependentUpdate: true);
		t.OnUpdate(delegate
		{
			UpdateEntitiesFocus();
		});
		t.OnComplete(delegate
		{
			UpdateEntities();
		});
	}

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(m_ConsoleHintOpen.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.CombatLogChangeState();
		}, 15, InputActionEventType.ButtonJustLongPressed)));
	}

	public void AddInputToCombat(InputLayer inputLayer)
	{
		AddDisposable(m_ConsoleHintOpenCombat.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.CombatLogChangeState();
		}, 15, InputActionEventType.ButtonJustLongPressed)));
	}

	public void AddInputToExploration(InputLayer inputLayer)
	{
		AddDisposable(m_ConsoleHintOpenExploration.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.CombatLogChangeState();
		}, 15, InputActionEventType.ButtonJustLongPressed)));
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
}
