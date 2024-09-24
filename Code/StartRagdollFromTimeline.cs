using Kingmaker.Visual.CharactersRigidbody;
using UnityEngine;

[ExecuteInEditMode]
public class StartRagdollFromTimeline : MonoBehaviour
{
	private RigidbodyCreatureController testControl;

	public Vector3 vectDirection = new Vector3(0f, 0f, 0f);

	public float vectMag = 1f;

	[SerializeField]
	private bool isStart;

	private void Start()
	{
		if (isStart)
		{
			runRagdoll();
		}
	}

	private void Awake()
	{
		if (isStart)
		{
			isStart = false;
			runRagdoll();
		}
	}

	public void runRagdoll()
	{
		testControl = GetComponent<RigidbodyCreatureController>();
		testControl.ApplyImpulse(vectDirection * vectMag, vectMag);
		testControl.StartRagdoll();
	}
}
