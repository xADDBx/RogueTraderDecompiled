using Kingmaker.Code.UI.MVVM.VM.Retrain;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Respec;

public class RespecContextCommonView : ViewBase<RespecContextVM>
{
	[SerializeField]
	private UIViewLink<RespecWindowCommonView, RespecVM> m_RespecWindowCommonView;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.RespecVM.Subscribe(m_RespecWindowCommonView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
