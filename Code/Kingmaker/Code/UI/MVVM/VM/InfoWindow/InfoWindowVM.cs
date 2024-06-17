using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.InfoWindow;

public class InfoWindowVM : InfoBaseVM, IInfoWindowHandler, ISubscriber, IDialogStartHandler
{
	private readonly Action m_OnClose;

	public readonly bool IsStartPos;

	private readonly IEnumerable<TooltipBaseTemplate> m_TooltipTemplates;

	private readonly bool m_ShouldNotHideLittleTooltip;

	public readonly ReactiveCommand ForceClose = new ReactiveCommand();

	protected override TooltipTemplateType TemplateType => TooltipTemplateType.Info;

	public InfoWindowVM(TooltipBaseTemplate template, Action onClose, bool shouldNotHideLittleTooltip = false)
		: base(template)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_TooltipTemplates = new TooltipBaseTemplate[1] { template };
		m_OnClose = onClose;
		m_ShouldNotHideLittleTooltip = shouldNotHideLittleTooltip;
		if (template is TooltipTemplateUnitInspect)
		{
			IsStartPos = true;
		}
	}

	public InfoWindowVM(IEnumerable<TooltipBaseTemplate> templates, Action onClose, bool shouldNotHideLittleTooltip = false)
		: base(templates)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_TooltipTemplates = templates;
		m_OnClose = onClose;
		m_ShouldNotHideLittleTooltip = shouldNotHideLittleTooltip;
	}

	public void OnClose()
	{
		m_OnClose();
		if (!m_ShouldNotHideLittleTooltip)
		{
			TooltipHelper.HideTooltip();
		}
	}

	[ItemCanBeNull]
	public IEnumerable<TooltipBaseTemplate> GetTooltipTemplates()
	{
		return m_TooltipTemplates;
	}

	public void HandleCloseTooltipInfoWindow()
	{
		OnClose();
	}

	public void HandleDialogStarted(BlueprintDialog dialog)
	{
		ForceClose.Execute();
	}
}
