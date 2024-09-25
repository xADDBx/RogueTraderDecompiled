using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class TooltipContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITooltipHandler, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInteractionHighlightUIHandler
{
	public readonly ReactiveProperty<TooltipVM> TooltipVM = new ReactiveProperty<TooltipVM>();

	public readonly ReactiveProperty<HintVM> HintVM = new ReactiveProperty<HintVM>();

	public readonly ReactiveProperty<InfoWindowVM> InfoWindowVM = new ReactiveProperty<InfoWindowVM>();

	public readonly ReactiveProperty<InfoWindowVM> GlossaryInfoWindowVM = new ReactiveProperty<InfoWindowVM>();

	public readonly ReactiveProperty<ComparativeTooltipVM> ComparativeTooltipVM = new ReactiveProperty<ComparativeTooltipVM>();

	public readonly TooltipsDataCache TooltipsDataCache;

	private IDisposable m_DelayedShowTooltipHandle;

	private IDisposable m_DelayedShowHintHandle;

	private bool m_MussHide;

	public TooltipContextVM()
	{
		AddDisposable(TooltipsDataCache = new TooltipsDataCache());
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeAll();
	}

	public void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip = false, bool showScrollbar = false)
	{
		DisposeTooltip();
		if (data != null)
		{
			m_DelayedShowTooltipHandle = DelayedInvoker.InvokeInTime(delegate
			{
				TooltipContextVM tooltipContextVM = this;
				TooltipVM disposable = (TooltipVM.Value = new TooltipVM(data, isComparative: false, shouldNotHideLittleTooltip, showScrollbar));
				tooltipContextVM.AddDisposable(disposable);
			}, SettingsRoot.Game.Tooltips.ShowDelay);
		}
	}

	public void HandleInfoRequest(TooltipBaseTemplate template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false)
	{
		DisposeInfoWindow();
		if (template != null)
		{
			ownerNavigationBehaviour?.UnFocusCurrentEntity();
			InfoWindowVM disposable = (InfoWindowVM.Value = new InfoWindowVM(template, delegate
			{
				DisposeInfoWindow();
				ownerNavigationBehaviour?.FocusOnCurrentEntity();
			}, shouldNotHideLittleTooltip));
			AddDisposable(disposable);
		}
	}

	public void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		DisposeInfoWindow();
		if (templates != null)
		{
			ownerNavigationBehaviour?.UnFocusCurrentEntity();
			InfoWindowVM disposable = (InfoWindowVM.Value = new InfoWindowVM(templates, delegate
			{
				DisposeInfoWindow();
				ownerNavigationBehaviour?.FocusOnCurrentEntity();
			}));
			AddDisposable(disposable);
		}
	}

	public void HandleGlossaryInfoRequest(TooltipTemplateGlossary template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		if (template != null)
		{
			ownerNavigationBehaviour?.UnFocusCurrentEntity();
			TooltipHelper.AddGlossaryHistory(template.GlossaryEntry);
			InfoWindowVM disposable = (GlossaryInfoWindowVM.Value = new InfoWindowVM(template, delegate
			{
				DisposeGlossaryInfoWindow();
				ownerNavigationBehaviour?.FocusOnCurrentEntity();
			}));
			AddDisposable(disposable);
		}
		else
		{
			DisposeGlossaryInfoWindow();
		}
	}

	public void HandleComparativeTooltipRequest(IEnumerable<TooltipData> data, bool showScrollbar = false)
	{
		DisposeComparativeTooltip();
		if (!data.Empty())
		{
			ComparativeTooltipVM disposable = (ComparativeTooltipVM.Value = new ComparativeTooltipVM(data, showScrollbar));
			AddDisposable(disposable);
		}
	}

	public void HandleHintRequest(HintData data, bool shouldShow)
	{
		if (data != null && shouldShow)
		{
			m_MussHide = false;
			m_DelayedShowHintHandle = DelayedInvoker.InvokeInTime(delegate
			{
				if (!m_MussHide)
				{
					TooltipContextVM tooltipContextVM = this;
					HintVM disposable = (HintVM.Value = new HintVM(data));
					tooltipContextVM.AddDisposable(disposable);
				}
			}, SettingsRoot.Game.Tooltips.ShowDelay);
		}
		else
		{
			DisposeHint();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			DisposeHint();
		}
	}

	public void HandleHighlightChange(bool isOn)
	{
		if (!isOn)
		{
			DisposeHint();
		}
	}

	private void DisposeTooltip()
	{
		m_DelayedShowTooltipHandle?.Dispose();
		m_DelayedShowTooltipHandle = null;
		TooltipVM.Value?.Dispose();
		TooltipVM.Value = null;
	}

	private void DisposeHint()
	{
		m_MussHide = true;
		m_DelayedShowHintHandle?.Dispose();
		m_DelayedShowHintHandle = null;
		HintVM.Value?.Dispose();
		HintVM.Value = null;
	}

	private void DisposeInfoWindow()
	{
		InfoWindowVM.Value?.Dispose();
		InfoWindowVM.Value = null;
	}

	private void DisposeGlossaryInfoWindow()
	{
		GlossaryInfoWindowVM.Value?.Dispose();
		GlossaryInfoWindowVM.Value = null;
	}

	private void DisposeComparativeTooltip()
	{
		ComparativeTooltipVM.Value?.Dispose();
		ComparativeTooltipVM.Value = null;
	}

	public void DisposeAll()
	{
		TooltipsDataCache.Clear();
		DisposeTooltip();
		DisposeHint();
		DisposeInfoWindow();
		DisposeGlossaryInfoWindow();
	}
}
