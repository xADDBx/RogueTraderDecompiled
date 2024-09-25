using Owlcat.Runtime.UI.VirtualListSystem.Grid;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public class VirtualListGridVertical : VirtualListComponent
{
	[SerializeField]
	private VirtualListLayoutSettingsGrid m_LayoutSettings;

	protected override IVirtualListLayoutSettings LayoutSettings => m_LayoutSettings;
}
