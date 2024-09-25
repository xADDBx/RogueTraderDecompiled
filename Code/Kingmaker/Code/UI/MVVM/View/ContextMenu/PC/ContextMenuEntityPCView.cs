using Kingmaker.Code.UI.MVVM.View.ContextMenu.Common;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu.PC;

public class ContextMenuEntityPCView : ContextMenuEntityView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_ButtonFx != null)
		{
			AddDisposable(m_Button.OnHoverAsObservable().Subscribe(m_ButtonFx.DoHovered));
		}
	}
}
