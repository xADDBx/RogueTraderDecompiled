using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class MeshBody : Body
{
	public List<Vector3> Vertices;

	public List<Vector3> Normals;

	public List<Vector4> Tangents;

	public List<Vector2> BaseUvs;

	public List<int> Indices;

	public List<int> VertexTriangleMap;

	public List<int> VertexTriangleMapOffsetCount;

	public int ParticlesOffset;

	public int VertexOffset;

	public bool RecalculateNormalsAndTangents;

	public MeshBody(string name, List<Particle> particles, List<Constraint> constraints, List<int2> disconnectedConstraintsOffsetCount, List<Vector3> vertices, List<Vector3> normals, List<Vector4> tangents, List<Vector2> baseUvs, List<int> indices, List<int> vertexTriangleMap, List<int> vertexTriangleMapOffsetCount)
		: base(name, particles, constraints, disconnectedConstraintsOffsetCount)
	{
		Vertices = vertices;
		Normals = normals;
		Tangents = tangents;
		BaseUvs = baseUvs;
		Indices = indices;
		VertexTriangleMap = vertexTriangleMap;
		VertexTriangleMapOffsetCount = vertexTriangleMapOffsetCount;
	}
}
