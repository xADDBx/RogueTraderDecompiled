using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;

public abstract class ContextButtonFx : MonoBehaviour
{
	public abstract void DoHovered(bool value);

	public abstract void DoBlink();
}
