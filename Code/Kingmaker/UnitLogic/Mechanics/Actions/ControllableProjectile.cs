using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Sound.Base;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[JsonObject]
public class ControllableProjectile
{
	public GameObject GameObject;

	[JsonProperty(IsReference = false)]
	public Vector3 Position;

	[JsonProperty(IsReference = false)]
	public Quaternion Rotation;

	[JsonProperty]
	public BlueprintControllableProjectileReference ControllableProjectileReference;

	public ControllableProjectile()
	{
	}

	public ControllableProjectile([NotNull] GameObject gameObject, BlueprintControllableProjectileReference controllableProjectileReference)
	{
		GameObject = gameObject;
		Position = gameObject.transform.position;
		Rotation = gameObject.transform.rotation;
		ControllableProjectileReference = controllableProjectileReference;
	}

	public void OnPreparationStart()
	{
		SoundEventsManager.PostEvent(ControllableProjectileReference.Get().PreparationStartSound, GameObject);
	}

	public void OnPreparationFinished()
	{
		SoundEventsManager.PostEvent(ControllableProjectileReference.Get().PreparationEndSound, GameObject);
	}
}
