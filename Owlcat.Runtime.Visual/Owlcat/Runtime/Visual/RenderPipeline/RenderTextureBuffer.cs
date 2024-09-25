using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public class RenderTextureBuffer
{
	private RenderTexture[] m_Buffer;

	private int m_CurrentIndex;

	public RenderTexture Rt0
	{
		get
		{
			if (IsDouble)
			{
				return m_Buffer[m_CurrentIndex];
			}
			return m_Buffer[0];
		}
	}

	public RenderTexture Rt1
	{
		get
		{
			if (IsDouble)
			{
				return m_Buffer[1 - m_CurrentIndex];
			}
			return m_Buffer[0];
		}
	}

	public bool IsDouble { get; }

	public RenderTextureBuffer(bool doubleBuffer, int width, int height, int depth, RenderTextureFormat format, string name)
	{
		IsDouble = doubleBuffer;
		int num = ((!IsDouble) ? 1 : 2);
		m_Buffer = new RenderTexture[num];
		for (int i = 0; i < num; i++)
		{
			RenderTexture renderTexture = new RenderTexture(width, height, depth, format, 0);
			renderTexture.name = $"{name}_rt{i}";
			renderTexture.filterMode = FilterMode.Bilinear;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.useMipMap = false;
			renderTexture.Create();
			m_Buffer[i] = renderTexture;
		}
	}

	public void Swap()
	{
		m_CurrentIndex = 1 - m_CurrentIndex;
	}

	public void Dispose()
	{
		for (int i = 0; i < m_Buffer.Length; i++)
		{
			if ((bool)m_Buffer[i])
			{
				if (RenderTexture.active == m_Buffer[i])
				{
					RenderTexture.active = null;
				}
				m_Buffer[i].Release();
				Object.DestroyImmediate(m_Buffer[i]);
			}
		}
	}
}
