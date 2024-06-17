using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("eab2f4a32f2c4ce4a9bc8e59769c56b9")]
public class SoundRagdollSettings : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<SoundRagdollSettings>
	{
	}

	[Header("1x1")]
	public float TimerWait_1x1 = 0.5f;

	public float Impulse01_1x1 = 1.5f;

	public float Impulse02_1x1 = 2.5f;

	[Header("2x2")]
	public float TimerWait_2x2 = 0.6f;

	public float Impulse01_2x2 = 2.1f;

	public float Impulse02_2x2 = 3.5f;
}
