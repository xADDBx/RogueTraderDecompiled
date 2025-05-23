using Kingmaker.Code.UI.MVVM.VM.ChangeAppearance;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.ChangeAppearance.PC;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.PC;

public class CharGenContextPCView : ViewBase<CharGenContextVM>
{
	[SerializeField]
	private UIDestroyViewLink<CharGenPCView, CharGenVM> m_CharGenPCView;

	[SerializeField]
	private UIDestroyViewLink<ChangeAppearancePCView, ChangeAppearanceVM> m_AppearancePCView;

	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScaler;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_CharGenPCView.CustomInitialize = InitializeScalers;
		AddDisposable(base.ViewModel.CharGenVM.Subscribe(m_CharGenPCView.Bind));
		m_AppearancePCView.CustomInitialize = InitializeScalers;
		AddDisposable(base.ViewModel.ChangeAppearanceVM.Subscribe(m_AppearancePCView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	private void InitializeScalers(MonoBehaviour view)
	{
		DollRoomTargetController[] componentsInChildren = view.gameObject.GetComponentsInChildren<DollRoomTargetController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CanvasScaler = m_CanvasScaler;
		}
	}
}
