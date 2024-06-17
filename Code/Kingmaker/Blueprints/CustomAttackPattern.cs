using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
public class CustomAttackPattern
{
	[HideInInspector]
	public List<Vector2Int> cells;
}
