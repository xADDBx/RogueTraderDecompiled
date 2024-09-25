using System;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.InfoWindow;

public class InfoSectionVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly ReactiveProperty<TooltipBaseTemplate> m_TooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	public IReactiveProperty<InfoBodyVM> InfoVM { get; } = new ReactiveProperty<InfoBodyVM>();


	public TooltipBaseTemplate CurrentTooltip => m_TooltipTemplate.Value;

	public InfoSectionVM()
	{
		AddDisposable(m_TooltipTemplate.ObserveLastValueOnLateUpdate().Subscribe(delegate(TooltipBaseTemplate temp)
		{
			SetTemplate(temp);
		}));
	}

	public void SetTemplate(TooltipBaseTemplate template)
	{
		m_TooltipTemplate.Value = template;
		InfoVM.Value?.Dispose();
		InfoVM.Value = ((template != null) ? new InfoBodyVM(template) : null);
	}

	protected override void DisposeImplementation()
	{
		InfoVM.Value?.Dispose();
	}
}
