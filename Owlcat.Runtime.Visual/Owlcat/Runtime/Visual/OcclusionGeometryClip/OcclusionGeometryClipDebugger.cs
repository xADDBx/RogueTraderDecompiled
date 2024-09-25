using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public sealed class OcclusionGeometryClipDebugger : MonoBehaviour
{
	public enum DrawType
	{
		None,
		Hierarchy,
		History
	}

	[SerializeField]
	private DrawType m_DrawType;
}
