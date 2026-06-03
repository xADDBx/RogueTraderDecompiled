using UnityEngine;

public class FX_NecronPuzzleCoverController : MonoBehaviour
{
	public ParticleSystem particleSystem1;

	public ParticleSystem particleSystem2;

	public ParticleSystem particleSystem3;

	public ParticleSystem particleSystem4;

	public Material screenMat1;

	public Material screenMat2;

	public Material screenMat3;

	public Material screenMat4;

	public Transform character;

	private void Start()
	{
		screenMat1 = particleSystem1.GetComponent<ParticleSystemRenderer>().material;
		screenMat2 = particleSystem2.GetComponent<ParticleSystemRenderer>().material;
		screenMat3 = particleSystem3.GetComponent<ParticleSystemRenderer>().material;
		screenMat4 = particleSystem4.GetComponent<ParticleSystemRenderer>().material;
	}

	private void Update()
	{
		screenMat1.SetVector("_Mask_Position", character.position);
		screenMat2.SetVector("_Mask_Position", character.position);
		screenMat3.SetVector("_Mask_Position", character.position);
		screenMat4.SetVector("_Mask_Position", character.position);
	}
}
