using UnityEngine;

namespace Owlcat.Runtime.Visual.Utilities;

public static class TetrahedronUtils
{
	public static readonly Vector4[] FaceVectors;

	public static readonly Vector3[] FacePlaneNormals;

	static TetrahedronUtils()
	{
		FaceVectors = new Vector4[4]
		{
			new Vector3(0f, -0.57735026f, 0.8164966f),
			new Vector3(0f, -0.57735026f, -0.8164966f),
			new Vector3(-0.8164966f, 0.57735026f, 0f),
			new Vector3(0.8164966f, 0.57735026f, 0f)
		};
		FacePlaneNormals = new Vector3[12]
		{
			CalculateNormal(-FaceVectors[3], -FaceVectors[2]),
			CalculateNormal(-FaceVectors[2], -FaceVectors[1]),
			CalculateNormal(-FaceVectors[1], -FaceVectors[3]),
			CalculateNormal(-FaceVectors[2], -FaceVectors[3]),
			CalculateNormal(-FaceVectors[3], -FaceVectors[0]),
			CalculateNormal(-FaceVectors[0], -FaceVectors[2]),
			CalculateNormal(-FaceVectors[3], -FaceVectors[1]),
			CalculateNormal(-FaceVectors[0], -FaceVectors[3]),
			CalculateNormal(-FaceVectors[1], -FaceVectors[0]),
			CalculateNormal(-FaceVectors[1], -FaceVectors[2]),
			CalculateNormal(-FaceVectors[0], -FaceVectors[1]),
			CalculateNormal(-FaceVectors[2], -FaceVectors[0])
		};
	}

	private static Vector3 CalculateNormal(Vector3 v0, Vector3 v1)
	{
		return Vector3.Normalize(Vector3.Cross(v0, v1));
	}

	public static void GetFacePlanes(int faceId, Vector3 lightPos, ref Plane[] planes)
	{
		planes[0] = new Plane(FacePlaneNormals[faceId * 3], lightPos);
		planes[1] = new Plane(FacePlaneNormals[faceId * 3 + 1], lightPos);
		planes[2] = new Plane(FacePlaneNormals[faceId * 3 + 2], lightPos);
	}
}
