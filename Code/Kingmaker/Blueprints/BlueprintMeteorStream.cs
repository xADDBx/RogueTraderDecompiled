using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("751a76a1090f416bbec1990d90844579")]
public class BlueprintMeteorStream : BlueprintMechanicEntityFact
{
	[SerializeField]
	public int Width;

	[SerializeField]
	public Vector2 Direction;

	[SerializeField]
	public Vector2 Position;

	[SerializeField]
	public float MeteorsFloatingSpeed = 1.5f;

	[SerializeField]
	public int StreamLengthInCells = 200;

	[SerializeField]
	[Tooltip("Density From is a value that represents number of cells. For example value 2 will mean that new meteor can be created 2 cells away MIN from the previous.")]
	public int DensityFrom = 2;

	[SerializeField]
	[Tooltip("Density To is a value that represents number of cells. For example value 4 will mean that new meteor can be created 4 cells away MAX from the previous")]
	public int DensityTo = 3;
}
