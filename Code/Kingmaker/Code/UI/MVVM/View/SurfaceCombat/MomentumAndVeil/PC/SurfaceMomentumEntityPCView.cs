using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.MomentumAndVeil.PC;

public class SurfaceMomentumEntityPCView : SurfaceMomentumEntityView
{
	[Header("PC")]
	[SerializeField]
	private Image m_HintPlace;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_HintPlace.SetTooltip(base.ViewModel.Tooltip));
	}
}
