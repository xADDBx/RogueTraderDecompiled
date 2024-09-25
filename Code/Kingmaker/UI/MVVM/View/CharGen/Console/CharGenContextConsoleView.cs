using Kingmaker.ResourceLinks;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console;

public class CharGenContextConsoleView : ViewBase<CharGenContextVM>
{
	[SerializeField]
	private UIDestroyViewLink<CharGenConsoleView, CharGenVM> m_CharGenConsoleView;

	[SerializeField]
	private CanvasScalerWorkaround m_CanvasScaler;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_CharGenConsoleView.CustomInitialize = InitializeScalers;
		AddDisposable(base.ViewModel.CharGenVM.Subscribe(m_CharGenConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	private void InitializeScalers(CharGenConsoleView view)
	{
		DollRoomTargetController[] componentsInChildren = view.gameObject.GetComponentsInChildren<DollRoomTargetController>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CanvasScaler = m_CanvasScaler;
		}
	}
}
