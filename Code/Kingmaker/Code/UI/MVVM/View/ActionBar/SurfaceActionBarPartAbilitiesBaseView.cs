using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class SurfaceActionBarPartAbilitiesBaseView : ViewBase<SurfaceActionBarPartAbilitiesVM>
{
	[SerializeField]
	protected RectTransform m_TooltipPlace;

	protected int SlotsInRow => 10;

	public abstract void Initialize();
}
