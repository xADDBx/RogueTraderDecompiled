using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public abstract class ShipPostsPanelBaseView : ViewBase<ShipPostsPanelVM>
{
	[SerializeField]
	protected RectTransform m_TooltipPlace;
}
