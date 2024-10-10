using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Loot.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.TwitchDrops;

public class TwitchDropsRewardsConsoleView : TwitchDropsRewardsBaseView, ICullFocusHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private LootSlotConsoleView m_SlotPrefab;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private IConsoleEntity m_CulledFocus;

	protected override void InitializeImpl()
	{
		m_SlotsGroup.Initialize(m_SlotPrefab);
	}

	protected override void CreateInputImpl()
	{
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			HandleComplete();
		}, 8, base.ViewModel.IsAwaiting.Not().ToReactiveProperty()), UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information));
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
		if (m_ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowConsoleTooltip(list, m_NavigationBehaviour, config);
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnPageFocusChangedImpl(m_NavigationBehaviour.DeepestNestedFocus);
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
