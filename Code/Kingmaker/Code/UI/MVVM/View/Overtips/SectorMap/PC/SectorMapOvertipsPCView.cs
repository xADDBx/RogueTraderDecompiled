using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap.PC;

public class SectorMapOvertipsPCView : SectorMapOvertipsView<OvertipSystemPCView, OvertipRumourPCView>
{
	[SerializeField]
	private OwlcatButton m_ClosePopup;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UISounds.Instance.SetClickAndHoverSound(m_ClosePopup, UISounds.ButtonSoundsEnum.NoSound);
		AddDisposable(m_ClosePopup.OnLeftClickAsObservable().Subscribe(delegate
		{
			SetClosePopupState();
		}));
	}
}
