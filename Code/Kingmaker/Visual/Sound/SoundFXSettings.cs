using System;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[Serializable]
public class SoundFXSettings
{
	[AkEventReference]
	public string Event;

	[Range(0f, 100f)]
	public float Gain = 100f;

	[Range(0f, 100f)]
	public float Pitch = 50f;

	public float Delay;
}
