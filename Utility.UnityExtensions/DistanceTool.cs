using UnityEngine;

[ExecuteInEditMode]
public class DistanceTool : MonoBehaviour
{
	public Transform From;

	public Transform To;

	public float DistanceResult;

	[Space]
	public Transform LookAt;

	private void Update()
	{
		if ((bool)From && (bool)To)
		{
			DistanceResult = Vector3.Distance(From.position, To.position);
		}
		if ((bool)LookAt)
		{
			base.transform.LookAt(LookAt);
		}
	}
}
