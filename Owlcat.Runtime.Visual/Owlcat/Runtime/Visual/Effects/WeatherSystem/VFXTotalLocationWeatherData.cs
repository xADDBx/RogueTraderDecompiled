using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

public static class VFXTotalLocationWeatherData
{
	private static Texture2D s_Texture;

	private static List<VFXLocationWeatherData> s_Areas = new List<VFXLocationWeatherData>();

	private static bool s_AreaSetChanged = false;

	private static RenderTexture s_CurrentRT;

	private static RenderTexture s_PreviousRT;

	private static Material s_Material;

	public static bool HasAreas => s_Areas.Count > 0;

	public static Texture2D Texture
	{
		get
		{
			if (!HasAreas)
			{
				return null;
			}
			if (s_AreaSetChanged)
			{
				Bake();
				s_AreaSetChanged = false;
			}
			return s_Texture;
		}
	}

	public static bool AreaSetChanged => s_AreaSetChanged;

	public static void ForceBake()
	{
		s_Areas.Clear();
		VFXLocationWeatherData[] array = Resources.FindObjectsOfTypeAll<VFXLocationWeatherData>();
		for (int i = 0; i < array.Length; i++)
		{
			AddArea(array[i]);
		}
		Bake();
	}

	public static void AddArea(VFXLocationWeatherData area)
	{
		if (!s_Areas.Contains(area))
		{
			s_Areas.Add(area);
			s_AreaSetChanged = true;
		}
	}

	public static void RemoveArea(VFXLocationWeatherData area)
	{
		s_AreaSetChanged |= s_Areas.Remove(area);
	}

	public static void RemoveAllEmptyAreas()
	{
		s_AreaSetChanged |= s_Areas.RemoveAll((VFXLocationWeatherData area) => area.Texture == null) > 0;
	}

	private static void Bake()
	{
		if (s_Areas.Count == 0)
		{
			return;
		}
		if (s_Areas.Count == 1)
		{
			s_Texture = s_Areas[0].Texture;
			return;
		}
		if (s_Material == null)
		{
			s_Material = new Material(Shader.Find("Hidden/Owlcat/Weather/Location Weather Data Combination"));
		}
		RenderTextureDescriptor desc = default(RenderTextureDescriptor);
		desc.width = s_Areas[0].Texture.width;
		desc.height = s_Areas[0].Texture.height;
		desc.colorFormat = RenderTextureFormat.ARGBHalf;
		desc.msaaSamples = 1;
		desc.volumeDepth = 1;
		desc.dimension = TextureDimension.Tex2D;
		desc.depthBufferBits = 0;
		s_CurrentRT = RenderTexture.GetTemporary(desc);
		s_PreviousRT = RenderTexture.GetTemporary(desc);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = s_CurrentRT;
		GL.Clear(clearDepth: true, clearColor: true, new Color(float.MinValue, float.MinValue, float.MinValue));
		RenderTexture.active = s_PreviousRT;
		GL.Clear(clearDepth: true, clearColor: true, new Color(float.MinValue, float.MinValue, float.MinValue));
		for (int i = 0; i < s_Areas.Count; i++)
		{
			s_Material.SetTexture("_MainTex", s_Areas[i].Texture);
			s_Material.SetTexture("_PreviousTex", s_PreviousRT);
			Graphics.Blit(s_Areas[i].Texture, s_CurrentRT, s_Material);
			Graphics.Blit(s_CurrentRT, s_PreviousRT);
		}
		s_Texture = new Texture2D(s_CurrentRT.width, s_CurrentRT.height, TextureFormat.RGBAHalf, mipChain: false);
		s_Texture.name = "VFXLocationWeatherData s_Texture";
		RenderTexture.active = s_CurrentRT;
		s_Texture.ReadPixels(new Rect(0f, 0f, s_CurrentRT.width, s_CurrentRT.height), 0, 0);
		RenderTexture.active = active;
		s_Texture.Apply();
		s_CurrentRT.Release();
		s_PreviousRT.Release();
	}
}
