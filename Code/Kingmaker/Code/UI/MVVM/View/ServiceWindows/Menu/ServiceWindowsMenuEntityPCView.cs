using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;
using Owlcat.Runtime.UI.SelectionGroup.View;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Menu;

public class ServiceWindowsMenuEntityPCView : SelectionGroupEntityView<ServiceWindowsMenuEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
	}
}
