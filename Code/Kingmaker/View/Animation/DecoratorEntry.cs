using System;
using UnityEngine;

namespace Kingmaker.View.Animation;

[Serializable]
public class DecoratorEntry
{
	public GameObject Prefab;

	public string BoneName;

	public Vector3 Position;

	public Vector3 Rotation;

	public Vector3 Scale = Vector3.one;
}
