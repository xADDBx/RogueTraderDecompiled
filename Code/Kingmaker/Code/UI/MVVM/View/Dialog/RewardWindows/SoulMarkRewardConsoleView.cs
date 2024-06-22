using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.RewardWindows;

public class SoulMarkRewardConsoleView : SoulMarkRewardBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private Vector2 m_TooltipPivot = new Vector2(0f, 0.5f);

	[SerializeField]
	private RectTransform m_TooltipPlace;

	private readonly BoolReactiveProperty m_TooltipShown = new BoolReactiveProperty();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private TooltipConfig m_TooltipConfig;

	private IConsoleHint m_DeclineBind;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_TooltipConfig = new TooltipConfig
		{
			PriorityPivots = new List<Vector2> { m_TooltipPivot },
			TooltipPlace = m_TooltipPlace
		};
		CreateInput();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(GamePad.Instance.OnLayerPoped.Subscribe(OnCurrentInputLayerChanged));
	}

	private void CreateInput()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "RewardWindow"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			CloseTooltip();
		}, 9, m_TooltipShown);
		m_DeclineBind = m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Back);
		AddDisposable(m_DeclineBind);
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			OnAccept();
		}, 8, m_TooltipShown.Not().ToReactiveProperty());
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
			ToggleTooltip();
		}, 10, m_TooltipShown.Not().ToReactiveProperty());
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.Tooltips.ShowTooltipHint));
		AddDisposable(inputBindStruct3);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void CloseTooltip()
	{
		if (m_TooltipShown.Value)
		{
			ToggleTooltip();
		}
	}

	private void OnAccept()
	{
		base.ViewModel.OnDeclinePressed();
	}

	private void ToggleTooltip()
	{
		m_TooltipShown.Value = !m_TooltipShown.Value;
		TooltipHelper.HideTooltip();
		if (m_TooltipShown.Value)
		{
			this.ShowConsoleTooltip(base.ViewModel.Tooltip, m_NavigationBehaviour, m_TooltipConfig);
		}
	}

	private void OnCurrentInputLayerChanged()
	{
		if (GamePad.Instance.CurrentInputLayer == m_InputLayer)
		{
			TooltipHelper.HideTooltip();
			m_TooltipShown.Value = false;
		}
	}
}
