using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[Serializable]
public class LightCookieSettings
{
	private static GraphicsFormat[][] s_LightCookieFormatList = new GraphicsFormat[5][]
	{
		new GraphicsFormat[1] { GraphicsFormat.R8_UNorm },
		new GraphicsFormat[1] { GraphicsFormat.R16_UNorm },
		new GraphicsFormat[4]
		{
			GraphicsFormat.R5G6B5_UNormPack16,
			GraphicsFormat.B5G6R5_UNormPack16,
			GraphicsFormat.R5G5B5A1_UNormPack16,
			GraphicsFormat.B5G5R5A1_UNormPack16
		},
		new GraphicsFormat[3]
		{
			GraphicsFormat.A2B10G10R10_UNormPack32,
			GraphicsFormat.R8G8B8A8_SRGB,
			GraphicsFormat.B8G8R8A8_SRGB
		},
		new GraphicsFormat[1] { GraphicsFormat.B10G11R11_UFloatPack32 }
	};

	[SerializeField]
	private LightCookieFormat m_LightCookieFormat = LightCookieFormat.ColorHigh;

	[SerializeField]
	private LightCookieResolution m_Resolution = LightCookieResolution._2048;

	public int2 Resolution => new int2((int)m_Resolution);

	public GraphicsFormat Format
	{
		get
		{
			GraphicsFormat graphicsFormat = GraphicsFormat.None;
			GraphicsFormat[] array = s_LightCookieFormatList[(int)m_LightCookieFormat];
			foreach (GraphicsFormat graphicsFormat2 in array)
			{
				if (SystemInfo.IsFormatSupported(graphicsFormat2, FormatUsage.Render))
				{
					graphicsFormat = graphicsFormat2;
					break;
				}
			}
			if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
			{
				graphicsFormat = GraphicsFormatUtility.GetLinearFormat(graphicsFormat);
			}
			if (graphicsFormat == GraphicsFormat.None)
			{
				graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
				Debug.LogWarning($"Lights Cookie Format ({m_LightCookieFormat.ToString()}) is not supported by the platform. Falling back to {GraphicsFormatUtility.GetBlockSize(graphicsFormat) * 8}-bit format ({GraphicsFormatUtility.GetFormatString(graphicsFormat)})");
			}
			return graphicsFormat;
		}
	}
}
