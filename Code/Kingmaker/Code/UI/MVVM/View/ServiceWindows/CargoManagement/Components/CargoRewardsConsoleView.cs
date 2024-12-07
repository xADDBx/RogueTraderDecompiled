using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoRewardsConsoleView : CargoRewardsBaseView, ICullFocusHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private IConsoleEntity m_CulledFocus;

	protected override void CreateInputImpl()
	{
		AddDisposable(m_HintsWidget.BindHint(InputLayer.AddButton(delegate
		{
			HandleComplete();
		}, 8), UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(m_HintsWidget.BindHint(InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information));
	}

	protected override void OnPageFocusChangedImpl(IConsoleEntity entity)
	{
		TooltipConfig tooltipConfig = default(TooltipConfig);
		tooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(0.5f, 0f)
		};
		TooltipConfig config = tooltipConfig;
		List<TooltipBaseTemplate> list = (entity as IHasTooltipTemplates)?.TooltipTemplates();
		if (list == null && entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			list = new List<TooltipBaseTemplate> { hasTooltipTemplate.TooltipTemplate() };
			if (hasTooltipTemplate.TooltipTemplate() is TooltipTemplateGlossary { GlossaryEntry: not null })
			{
				config.IsGlossary = true;
			}
		}
		m_HasTooltip.Value = list != null && list.Count > 0;
		if (ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowConsoleTooltip(list, NavigationBehaviour, config);
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnPageFocusChangedImpl(NavigationBehaviour.DeepestNestedFocus);
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = NavigationBehaviour.DeepestNestedFocus;
		NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			NavigationBehaviour.UpdateDeepestFocusObserve();
			m_CulledFocus = null;
		}
	}
}
