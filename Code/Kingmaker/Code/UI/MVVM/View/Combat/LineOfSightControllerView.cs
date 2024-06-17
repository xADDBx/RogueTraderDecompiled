using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Combat;

public class LineOfSightControllerView : ViewBase<LineOfSightControllerVM>
{
	[SerializeField]
	private LineOfSightView m_SightPCView;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		foreach (LineOfSightVM linesVM in base.ViewModel.LinesVMs)
		{
			DrawLine(linesVM);
		}
		AddDisposable(base.ViewModel.LinesVMs.ObserveAdd().Subscribe(delegate(CollectionAddEvent<LineOfSightVM> value)
		{
			DrawLine(value.Value);
		}));
	}

	private void DrawLine(LineOfSightVM vm)
	{
		LineOfSightView widget = WidgetFactory.GetWidget(m_SightPCView);
		widget.Bind(vm);
		widget.transform.SetParent(base.transform, worldPositionStays: false);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
