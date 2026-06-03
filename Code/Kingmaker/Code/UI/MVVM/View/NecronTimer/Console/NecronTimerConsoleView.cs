using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NecronTimer.Console;

public class NecronTimerConsoleView : NecronTimerView
{
	[SerializeField]
	private ConsoleHint m_ConsoleHintOpen;

	[SerializeField]
	private ConsoleHint m_ConsoleHintClose;

	private readonly BoolReactiveProperty m_IsTooltipShown = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(HideTooltip));
	}

	protected override void DestroyViewImplementation()
	{
		if (m_IsTooltipShown.Value)
		{
			HideTooltip();
		}
		base.DestroyViewImplementation();
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> actionBarVisible)
	{
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = base.ViewModel.IsUnlockedAndVisible.And(actionBarVisible.Not()).ToReactiveProperty();
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty2 = readOnlyReactiveProperty.And(m_IsTooltipShown.Not()).ToReactiveProperty();
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			ShowTooltip();
		}, 14, readOnlyReactiveProperty2, InputActionEventType.ButtonJustLongPressed);
		AddDisposable(m_ConsoleHintOpen.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			HideTooltip();
		}, 9, m_IsTooltipShown);
		AddDisposable(m_ConsoleHintClose.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		AddDisposable(readOnlyReactiveProperty.Subscribe(delegate(bool value)
		{
			if (!value && m_IsTooltipShown.Value)
			{
				HideTooltip();
			}
		}));
	}

	private void ShowTooltip()
	{
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		TooltipTemplateSimple template = new TooltipTemplateSimple(tooltips.NecronTimerHeader, tooltips.NecronTimerDescription);
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.TooltipPlace = m_TooltipPlace;
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0f, 1f),
			new Vector2(0f, 0.75f),
			new Vector2(0f, 0.5f),
			new Vector2(0f, 0.25f)
		};
		TooltipConfig config = tooltipConfig;
		this.ShowConsoleTooltip(template, null, config);
		m_IsTooltipShown.Value = true;
	}

	private void HideTooltip()
	{
		TooltipHelper.HideTooltip();
		m_IsTooltipShown.Value = false;
	}
}
