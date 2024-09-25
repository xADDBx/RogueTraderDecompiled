using Owlcat.Runtime.UI.VirtualListSystem.Vertical;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public class VirtualListVertical : VirtualListComponent
{
	[SerializeField]
	private VirtualListLayoutSettingsVertical m_LayoutSettings;

	protected override IVirtualListLayoutSettings LayoutSettings => m_LayoutSettings;
}
