using Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap.AllSystemsInformationWindow;

public class SystemInfoAllSystemsInformationWindowPCView : SystemInfoAllSystemsInformationWindowBaseView
{
	[SerializeField]
	private OwlcatButton m_MoveCameraToSystemButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_MoveCameraToSystemButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			SetCameraOnSystem();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
