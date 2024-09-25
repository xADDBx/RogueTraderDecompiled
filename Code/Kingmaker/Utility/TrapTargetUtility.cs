using Kingmaker.View.MapObjects.SriptZones;
using UnityEngine;

namespace Kingmaker.Utility;

public class TrapTargetUtility : MonoBehaviour
{
	[SerializeField]
	private ScriptZonePolygon _polygon;

	[SerializeField]
	private LayerMask RaycastLayers = 2097409;

	[SerializeField]
	private float RaycastHeightOffset = 1f;

	[SerializeField]
	private float Height = 0.05f;

	public bool HasPolygon => _polygon != null;

	public void SetPolygon(ScriptZonePolygon polygon)
	{
		_polygon = polygon;
	}

	public void MoveToPolygonCenter()
	{
		if (_polygon == null)
		{
			Debug.LogError(base.name + ": _polygon is null! Make sure you set ScriptZonePolygon to field of TrapTargetUtility.");
			return;
		}
		Vector3 position = _polygon.Center();
		base.transform.SetPositionAndRotation(position, Quaternion.identity);
	}

	public void PlaceOnSurface()
	{
		Vector3 position = base.transform.position;
		position = SurfaceRaycastUtils.RaycastPointToSurface(position, RaycastHeightOffset, RaycastLayers);
		position += Vector3.up * Height;
		base.transform.SetPositionAndRotation(position, Quaternion.identity);
	}
}
