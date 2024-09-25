using Owlcat.Runtime.UI.VirtualListSystem.Horizontal;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public class VirtualListHorizontal : VirtualListComponent
{
	[SerializeField]
	private VirtualListLayoutSettingsHorizontal m_LayoutSettings;

	protected override IVirtualListLayoutSettings LayoutSettings => m_LayoutSettings;
}
