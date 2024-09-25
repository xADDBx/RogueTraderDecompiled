using UnityEngine;

namespace Kingmaker.Visual.Decals;

[ExecuteInEditMode]
public class Decal : MonoBehaviour
{
	[SerializeField]
	[EditorWeakReference(typeof(Material))]
	private string m_RefMaterial;

	[Range(1f, 180f)]
	public float MaxAngle = 90f;

	[Range(0.0001f, 0.1f)]
	public float PushDistance = 0.001f;

	public LayerMask AffectedLayers = -1;

	public bool BakeSubstrate = true;

	public bool ScreenSpaceDecalsReceiver = true;

	private void OnDrawGizmosSelected()
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = matrix;
	}
}
