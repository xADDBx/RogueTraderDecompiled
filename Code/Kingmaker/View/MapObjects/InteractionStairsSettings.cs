using System;
using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionStairsSettings : InteractionSettings
{
	[SerializeField]
	public WarhammerNodeLink NodeLink;
}
