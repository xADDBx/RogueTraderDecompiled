using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.ContextMenu;

public class ContextMenuSeparatorView : ViewBase<ContextMenuEntityVM>
{
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
	}

	protected override void DestroyViewImplementation()
	{
	}
}
