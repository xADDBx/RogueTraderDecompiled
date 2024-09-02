using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.GroupChanger;

public class GroupChangerContextBaseView : ViewBase<GroupChangerContextVM>
{
	[SerializeField]
	private UIDestroyViewLink<GroupChangerBaseView, GroupChangerVM> m_GroupChangerPCView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.GroupChangerVm.Subscribe(m_GroupChangerPCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
