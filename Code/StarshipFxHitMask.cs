using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Space/StarshipFxHitMask")]
public class StarshipFxHitMask : ScriptableObject
{
	public Mesh mesh;

	public int hitPointsPerSide = 20;

	public List<Vector3> frontHitPositions = new List<Vector3>();

	public List<Vector3> backHitPositions = new List<Vector3>();

	public List<Vector3> leftHitPositions = new List<Vector3>();

	public List<Vector3> rightHitPositions = new List<Vector3>();

	public void FillHitPositionsFromMesh(Mesh meshFromView = null)
	{
		frontHitPositions.Clear();
		backHitPositions.Clear();
		leftHitPositions.Clear();
		rightHitPositions.Clear();
		if (meshFromView != null)
		{
			mesh = meshFromView;
		}
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 item = vertices[i];
			Vector3 vector = -Vector3.right;
			float num = Vector2.Dot(new Vector2(vector.x, vector.z).normalized, new Vector2(item.x, item.z).normalized);
			if (num >= 0.5f)
			{
				leftHitPositions.Add(item);
			}
			else if (num < -0.5f)
			{
				rightHitPositions.Add(item);
			}
			if (item.z <= 0f)
			{
				backHitPositions.Add(item);
			}
			else
			{
				frontHitPositions.Add(item);
			}
		}
		frontHitPositions.Shuffle(PFStatefulRandom.Visuals.Starship);
		frontHitPositions.RemoveRange(hitPointsPerSide, frontHitPositions.Count - hitPointsPerSide);
		backHitPositions.Shuffle(PFStatefulRandom.Visuals.Starship);
		backHitPositions.RemoveRange(hitPointsPerSide, backHitPositions.Count - hitPointsPerSide);
		leftHitPositions.Shuffle(PFStatefulRandom.Visuals.Starship);
		leftHitPositions.RemoveRange(hitPointsPerSide, leftHitPositions.Count - hitPointsPerSide);
		rightHitPositions.Shuffle(PFStatefulRandom.Visuals.Starship);
		rightHitPositions.RemoveRange(hitPointsPerSide, rightHitPositions.Count - hitPointsPerSide);
	}
}
