using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class SpaceCombatHealthPartProgressPCView : ViewBase<UnitHealthPartVM>
{
	[SerializeField]
	private DelayedSlider m_Health;

	public void Initialize()
	{
		m_Health.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentHpRatio.Subscribe(delegate(float value)
		{
			m_Health.SetValue(value);
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
