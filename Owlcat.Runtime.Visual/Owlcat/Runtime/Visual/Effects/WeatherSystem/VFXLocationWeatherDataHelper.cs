using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public static class VFXLocationWeatherDataHelper
{
	public static List<Mesh> CreateMeshes(Color[] pixels)
	{
		List<Mesh> list = new List<Mesh>();
		List<Vector3> list2 = new List<Vector3>();
		List<int> list3 = new List<int>();
		int num = 0;
		Vector3 vector = Vector3.up * 0.025f;
		Vector3 vector2 = Vector3.forward * 0.144f;
		Vector3 vector3 = -Vector3.forward * 0.072f + Vector3.right * 0.125f;
		Vector3 vector4 = -Vector3.forward * 0.072f - Vector3.right * 0.125f;
		for (int i = 0; i < pixels.Length; i++)
		{
			Color color = pixels[i];
			if (!(color.r < -1.7014117E+38f) && !(color.g < -1.7014117E+38f) && !(color.b < -1.7014117E+38f))
			{
				Vector3 vector5 = new Vector3(color.r, color.g, color.b) + vector;
				list2.Add(vector5 + vector2);
				list2.Add(vector5 + vector3);
				list2.Add(vector5 + vector4);
				list3.Add(num++);
				list3.Add(num++);
				list3.Add(num++);
				if (list2.Count > 65532)
				{
					list.Add(new Mesh
					{
						vertices = list2.ToArray(),
						triangles = list3.ToArray()
					});
					list2.Clear();
					list3.Clear();
					num = 0;
				}
			}
		}
		if (list2.Count > 0)
		{
			list.Add(new Mesh
			{
				vertices = list2.ToArray(),
				triangles = list3.ToArray()
			});
		}
		foreach (Mesh item in list)
		{
			item.RecalculateNormals();
		}
		return list;
	}
}
