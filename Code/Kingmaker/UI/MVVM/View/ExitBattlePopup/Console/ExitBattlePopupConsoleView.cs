using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Loot.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.ExitBattlePopup.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ExitBattlePopup.Console;

public class ExitBattlePopupConsoleView : ExitBattlePopupBaseView
{
	[Header("Console")]
	[SerializeField]
	private LootSlotConsoleView m_SlotPrefab;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	protected override void InitializeImpl()
	{
		m_ItemsSlotsGroup.Initialize(m_SlotPrefab);
	}

	protected override void CreateInputImpl()
	{
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.ExitBattle();
		}, 8), UIStrings.Instance.SettingsUI.DialogOk));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.ExitBattle(forceOpenVoidshipUpgrade: true);
		}, 10, base.ViewModel.IsUpgradeAvailable), UIStrings.Instance.ShipCustomization.Attune));
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
}
