using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class DebugDraw
{
	public enum DepthTestType
	{
		NoDepth,
		Hide,
		Alpha
	}

	public static Material s_LineMaterial;

	public static Material s_LineMaterialWithDepth;

	private static Vector3[] s_CurvePoints;

	static DebugDraw()
	{
		s_CurvePoints = new Vector3[64];
		CreateLineMaterial();
	}

	private static void CreateLineMaterial()
	{
		if (!s_LineMaterial)
		{
			s_LineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
			s_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
			s_LineMaterial.SetInt(ShaderProps._SrcBlend, 5);
			s_LineMaterial.SetInt(ShaderProps._DstBlend, 10);
			s_LineMaterial.SetInt(ShaderProps._Cull, 0);
			s_LineMaterial.SetInt(ShaderProps._ZWrite, 0);
			s_LineMaterial.SetInt(ShaderProps._ZTest, 0);
		}
		if (!s_LineMaterialWithDepth)
		{
			s_LineMaterialWithDepth = new Material(Shader.Find("Hidden/Internal-Colored"));
			s_LineMaterialWithDepth.hideFlags = HideFlags.HideAndDontSave;
			s_LineMaterialWithDepth.SetInt(ShaderProps._SrcBlend, 5);
			s_LineMaterialWithDepth.SetInt(ShaderProps._DstBlend, 10);
			s_LineMaterialWithDepth.SetInt(ShaderProps._Cull, 0);
			s_LineMaterialWithDepth.SetInt(ShaderProps._ZWrite, 1);
		}
	}

	public static void DrawPolyLine(Vector3[] points)
	{
		DrawPolyLine(points, Gizmos.color, DepthTestType.Alpha);
	}

	public static void DrawPolyLine(Vector3[] points, Color color, DepthTestType depth)
	{
		CreateLineMaterial();
		((depth == DepthTestType.NoDepth) ? s_LineMaterial : s_LineMaterialWithDepth).SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(Gizmos.matrix);
		GL.Begin(1);
		GL.Color(color);
		for (int i = 0; i < points.Length - 1; i++)
		{
			Vector3 v = points[i % points.Length];
			Vector3 v2 = points[(i + 1) % points.Length];
			GL.Vertex(v);
			GL.Vertex(v2);
		}
		GL.End();
		GL.PopMatrix();
		if (depth == DepthTestType.Alpha)
		{
			color.a *= 0.2f;
			DrawPolyLine(points, color, DepthTestType.NoDepth);
		}
	}

	public static void DrawLine(Vector3 point1, Vector3 point2)
	{
		DrawLine(point1, point2, Gizmos.color, DepthTestType.Alpha);
	}

	public static void DrawLine(Vector3 point1, Vector3 point2, Color color, DepthTestType depth)
	{
		CreateLineMaterial();
		((depth == DepthTestType.NoDepth) ? s_LineMaterial : s_LineMaterialWithDepth).SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(Gizmos.matrix);
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex(point1);
		GL.Vertex(point2);
		GL.End();
		GL.PopMatrix();
		if (depth == DepthTestType.Alpha)
		{
			color.a *= 0.2f;
			DrawLine(point1, point2, color, DepthTestType.NoDepth);
		}
	}

	public static void DrawCircle(Vector3 center, Vector3 normal, float radius)
	{
		DrawCircle(center, normal, radius, Gizmos.color, DepthTestType.Alpha);
	}

	public static void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color, DepthTestType depth)
	{
		float num = 360f / (float)(s_CurvePoints.Length - 1);
		Vector3 vector = ((normal == Vector3.forward) ? Vector3.up : Vector3.Cross(Vector3.forward, normal));
		for (int i = 0; i < s_CurvePoints.Length; i++)
		{
			Vector3 vector2 = center + Quaternion.AngleAxis(num * (float)i, normal) * vector * radius;
			s_CurvePoints[i] = vector2;
		}
		DrawPolyLine(s_CurvePoints, color, depth);
	}

	public static void DrawArc(Vector3 center, Vector3 normal, float radius, Vector3 start, float angle, Color color, DepthTestType depth = DepthTestType.Alpha)
	{
		s_CurvePoints[0] = center;
		s_CurvePoints[s_CurvePoints.Length - 1] = center;
		float num = angle / (float)(s_CurvePoints.Length - 3);
		for (int i = 0; i < s_CurvePoints.Length - 2; i++)
		{
			Vector3 vector = center + Quaternion.AngleAxis(num * (float)i, normal) * start * radius;
			s_CurvePoints[i + 1] = vector;
		}
		DrawPolyLine(s_CurvePoints, color, depth);
	}

	public static void DrawOrientedPosition(Transform t, Color color)
	{
	}

	public static void DrawPosition(Transform t, Color color)
	{
	}

	public static Mesh CreateQuadMesh()
	{
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[4]
		{
			new Vector3(1f, 0f, 1f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0f, 0f, 0f)
		};
		Vector2[] uv = new Vector2[4]
		{
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(0f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		return mesh;
	}
}
