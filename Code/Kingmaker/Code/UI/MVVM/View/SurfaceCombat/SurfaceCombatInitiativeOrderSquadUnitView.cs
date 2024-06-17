using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public class SurfaceCombatInitiativeOrderSquadUnitView : ViewBase<SurfaceCombatUnitVM>, IWidgetView
{
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private UnitHealthPartProgressPCView m_UnitHealthPartProgressPCView;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		m_Portrait.sprite = base.ViewModel.SmallPortrait;
		AddDisposable(base.ViewModel.UnitHealthPartVM.Subscribe(m_UnitHealthPartProgressPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel viewModel)
	{
		Bind(viewModel as SurfaceCombatUnitVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is SurfaceCombatUnitVM;
	}
}
