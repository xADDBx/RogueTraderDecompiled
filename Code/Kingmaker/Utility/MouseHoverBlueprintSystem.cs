using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Utility;

public class MouseHoverBlueprintSystem : IService, ITooltipHandler, ISubscriber, IDisposable
{
	public static MouseHoverBlueprintSystem Instance => Services.GetInstance<MouseHoverBlueprintSystem>();

	public string UnderMouseBlueprintName { get; private set; } = string.Empty;


	public TooltipData TooltipData { get; private set; }

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public MouseHoverBlueprintSystem()
	{
		EventBus.Subscribe(this);
	}

	public void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip = false, bool showScrollbar = false)
	{
		if (data != null)
		{
			HandleTooltipCreated(data);
		}
		else
		{
			HandleTooltipDeleted();
		}
	}

	public void HandleComparativeTooltipRequest(IEnumerable<TooltipData> data, bool showScrollbar = false)
	{
		HandleTooltipRequest(data?.LastOrDefault(), shouldNotHideLittleTooltip: false, showScrollbar);
	}

	public void HandleInfoRequest(TooltipBaseTemplate template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false)
	{
	}

	public void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
	}

	public void HandleGlossaryInfoRequest(TooltipTemplateGlossary template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
	}

	public void HandleHintRequest(HintData data, bool shouldShow)
	{
	}

	private void HandleTooltipCreated(TooltipData tooltipData)
	{
		TooltipBaseTemplate mainTemplate = tooltipData.MainTemplate;
		string underMouseBlueprintName = ((mainTemplate is TooltipTemplateBuff tooltipTemplateBuff) ? Utilities.GetBlueprintName(tooltipTemplateBuff.Buff?.Blueprint ?? tooltipTemplateBuff.BlueprintBuff) : ((mainTemplate is TooltipTemplateFeature tooltipTemplateFeature) ? Utilities.GetBlueprintName(tooltipTemplateFeature.BlueprintFeatureBase) : ((mainTemplate is TooltipTemplateUIFeature tooltipTemplateUIFeature) ? Utilities.GetBlueprintName(tooltipTemplateUIFeature.UIFeature.Feature) : ((mainTemplate is TooltipTemplateAbility tooltipTemplateAbility) ? Utilities.GetBlueprintName(tooltipTemplateAbility.BlueprintAbility) : ((mainTemplate is TooltipTemplateActivatableAbility tooltipTemplateActivatableAbility) ? Utilities.GetBlueprintName(tooltipTemplateActivatableAbility.BlueprintActivatableAbility) : ((!(mainTemplate is TooltipTemplateItem tooltipTemplateItem)) ? string.Empty : Utilities.GetBlueprintName(tooltipTemplateItem.Item?.Blueprint)))))));
		UnderMouseBlueprintName = underMouseBlueprintName;
		if (!string.IsNullOrWhiteSpace(UnderMouseBlueprintName))
		{
			TooltipData = tooltipData;
		}
	}

	private void HandleTooltipDeleted()
	{
		TooltipData = null;
		UnderMouseBlueprintName = string.Empty;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}
}
