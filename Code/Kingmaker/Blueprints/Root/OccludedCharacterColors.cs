using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class OccludedCharacterColors
{
	public Color Ally = new Color32(34, 86, 124, byte.MaxValue);

	public Color Enemy = Color.red;
}
