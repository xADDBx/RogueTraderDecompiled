using System.Collections;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public class SillyCheatCritterSprinkler : MonoBehaviour, IFxSpawner
{
	public GameObject[] Critters;

	public int MinCount = 2;

	public int MaxCount = 4;

	public float ForceMin = 3f;

	public float ForceMax = 5f;

	private static StatefulRandom Random => PFStatefulRandom.NonDeterministic;

	public FxSpawnerPriority Priority => (FxSpawnerPriority)201;

	public void SpawnFxOnGameObject(GameObject target)
	{
		StartCoroutine(SpawnCritters());
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
		StartCoroutine(SpawnCritters());
	}

	private IEnumerator SpawnCritters()
	{
		yield return null;
		int num = Random.Range(MinCount, MaxCount + 1);
		while (num-- > 0)
		{
			GameObject obj = Object.Instantiate(Critters.Random(PFStatefulRandom.NonDeterministic), base.transform.position, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			Rabbit component = obj.GetComponent<Rabbit>();
			component.enabled = false;
			component.StopAllCoroutines();
			Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
			obj.AddComponent<SphereCollider>().radius = 0.3f;
			rigidbody.AddForce(-(base.transform.forward + base.transform.right * Random.Range(-1f, 1f)) * Random.Range(ForceMin, ForceMax));
			StartCoroutine(EnableRabbit(component, rigidbody));
		}
	}

	private IEnumerator EnableRabbit(Rabbit rabbit, Rigidbody rb)
	{
		yield return new WaitForSeconds(1.5f);
		rabbit.enabled = true;
		rabbit.Init(force: true);
		Object.Destroy(rb);
		Object.Destroy(rabbit.GetComponent<SphereCollider>());
		rabbit.transform.rotation = Quaternion.Euler(0f, rabbit.transform.rotation.eulerAngles.y, 0f);
		Object.Destroy(rabbit.gameObject, Random.Range(10f, 30f));
	}
}
