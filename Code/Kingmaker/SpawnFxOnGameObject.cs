using System.Collections;
using System.Collections.Generic;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker;

[HelpURL("https://confluence.owlcat.local/display/WH40K/FXs+on+static+objects")]
public class SpawnFxOnGameObject : MonoBehaviour
{
	[Tooltip("Prefab of FX to spawn")]
	public GameObject fxPrefab;

	[Tooltip("The object on which we want to spawn your FX")]
	public GameObject targetGameObject;

	[Tooltip("Toggle if you want to spawn FX on start")]
	public bool spawnOnStart = true;

	public float spawnOnStartDelay = 1f;

	[Tooltip("Toggle if you want to spawn FX in runtime right now")]
	public bool spawnRightNow;

	[HideInInspector]
	public GameObject spawnedFx;

	[HideInInspector]
	public bool SubmeshesInRenderers;

	[HideInInspector]
	public bool IsPrefabInstance;

	[HideInInspector]
	public bool IsUsedWithStaticPrefab;

	[Tooltip("List of new meshes on disk (required only if you need to cut source mesh)")]
	public List<Mesh> cutMeshes;

	private void Update()
	{
		if (spawnRightNow)
		{
			if (spawnedFx != null)
			{
				Object.Destroy(spawnedFx);
			}
			SpawnFx();
			spawnRightNow = false;
		}
	}

	private IEnumerator SpawnDelayed()
	{
		yield return new WaitForSeconds(spawnOnStartDelay);
		SpawnFx();
	}

	private void Start()
	{
		if (IsUsedWithStaticPrefab)
		{
			PFLog.TechArt.Error("SpawnFxOnGameObject component usage is forbidden with StaticPrefab component. Use mesh instead.");
			Object.DestroyImmediate(this);
		}
		if (targetGameObject == null)
		{
			targetGameObject = base.gameObject;
		}
		if (spawnOnStart)
		{
			StartCoroutine(SpawnDelayed());
		}
	}

	private void SpawnFx()
	{
		if (fxPrefab == null || targetGameObject == null)
		{
			PFLog.TechArt.Error(base.gameObject, "Can't spawn FX, either prefab is empty or the target game object.");
			return;
		}
		if (spawnedFx != null)
		{
			Object.Destroy(spawnedFx);
		}
		spawnedFx = FxHelper.SpawnFxOnGameObject(fxPrefab, targetGameObject);
	}
}
