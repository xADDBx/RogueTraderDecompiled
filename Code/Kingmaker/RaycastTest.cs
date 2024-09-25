using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker;

public class RaycastTest : MonoBehaviour
{
	[SerializeField]
	public LayerMask CastLayer = 27;

	[SerializeField]
	public float MaxDistance = 10000f;

	[Space]
	[SerializeField]
	public GameObject CollidersRoot;

	[SerializeField]
	public GameObject[] SearchRoots;

	private Vector3[] castDirections = new Vector3[8]
	{
		Vector3.forward,
		Vector3.forward + Vector3.right,
		Vector3.right,
		Vector3.right + Vector3.back,
		Vector3.back,
		Vector3.back + Vector3.left,
		Vector3.left,
		Vector3.left + Vector3.forward
	};

	private CustomSampler sampler;

	private Recorder recorder;

	private void Start()
	{
		sampler = CustomSampler.Create("MyCustomSampler");
		recorder = sampler.GetRecorder();
		if (recorder.isValid)
		{
			recorder.enabled = true;
		}
	}

	private void Update()
	{
	}

	public void CastRaysSamples(RaycastTest raycastTest)
	{
		Vector3 position = raycastTest.transform.position;
		for (int i = 0; i < castDirections.Length; i++)
		{
			Vector3 direction = castDirections[i];
			Physics.Raycast(position, direction, out var _, raycastTest.MaxDistance, raycastTest.CastLayer);
		}
		Debug.Log($"Sampled time = {(double)recorder.elapsedNanoseconds / 1000000.0} ms");
	}
}
