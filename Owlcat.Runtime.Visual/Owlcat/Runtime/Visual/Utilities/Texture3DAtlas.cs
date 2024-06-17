using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Utilities;

public class Texture3DAtlas
{
	public static class ShaderPropertyID
	{
		public static readonly int _Dst3DTexture = Shader.PropertyToID("_Dst3DTexture");

		public static readonly int _Src3DTexture = Shader.PropertyToID("_Src3DTexture");

		public static readonly int _AlphaOnlyTexture = Shader.PropertyToID("_AlphaOnlyTexture");

		public static readonly int _SrcSize = Shader.PropertyToID("_SrcSize");

		public static readonly int _SrcMip = Shader.PropertyToID("_SrcMip");

		public static readonly int _SrcScale = Shader.PropertyToID("_SrcScale");

		public static readonly int _SrcOffset = Shader.PropertyToID("_SrcOffset");

		public static readonly int _DstOffset = Shader.PropertyToID("_DstOffset");
	}

	private class AtlasElement
	{
		public Vector3Int position;

		public int size;

		public Texture texture;

		public int hash;

		public AtlasElement[] children;

		public AtlasElement parent;

		public bool IsFree()
		{
			if (texture == null)
			{
				return children == null;
			}
			return false;
		}

		public AtlasElement(Vector3Int position, int size, Texture texture = null)
		{
			this.position = position;
			this.size = size;
			this.texture = texture;
			hash = 0;
		}

		public void PopulateChildren()
		{
			children = new AtlasElement[8];
			int num = size / 2;
			children[0] = new AtlasElement(position + new Vector3Int(0, 0, 0), num);
			children[1] = new AtlasElement(position + new Vector3Int(num, 0, 0), num);
			children[2] = new AtlasElement(position + new Vector3Int(0, 0, num), num);
			children[3] = new AtlasElement(position + new Vector3Int(num, 0, num), num);
			children[4] = new AtlasElement(position + new Vector3Int(0, num, 0), num);
			children[5] = new AtlasElement(position + new Vector3Int(num, num, 0), num);
			children[6] = new AtlasElement(position + new Vector3Int(0, num, num), num);
			children[7] = new AtlasElement(position + new Vector3Int(num, num, num), num);
			AtlasElement[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].parent = this;
			}
		}

		public void RemoveChildrenIfEmpty()
		{
			bool flag = true;
			AtlasElement[] array = children;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].texture != null)
				{
					flag = false;
				}
			}
			if (flag)
			{
				children = null;
			}
		}

		public override string ToString()
		{
			return $"3D Atlas Element, pos: {position}, size: {size}, texture:{texture}, children: {children != null}";
		}
	}

	private struct MipGenerationSwapData
	{
		public RenderTexture target;

		public Vector3Int offset;

		public int mipOffset;
	}

	private List<AtlasElement> m_Elements = new List<AtlasElement>();

	private Dictionary<Texture, AtlasElement> m_TextureElementsMap = new Dictionary<Texture, AtlasElement>();

	private RenderTexture m_Atlas;

	private RenderTexture m_MipMapGenerationTemp;

	private GraphicsFormat m_format;

	private ComputeShader m_Texture3DAtlasCompute;

	private int m_CopyKernel;

	private int m_GenerateMipKernel;

	private Vector3Int m_KernelGroupSize;

	private int m_MaxElementSize;

	private int m_MaxElementCount;

	private bool m_HasMipMaps;

	private const float k_MipmapFactorApprox = 1.33f;

	public Texture3DAtlas(ComputeShader texture3DAtlasCompute, GraphicsFormat format, int maxElementSize, int maxElementCount, bool hasMipMaps = true)
	{
		m_format = format;
		m_MaxElementSize = maxElementSize;
		m_MaxElementCount = maxElementCount;
		m_HasMipMaps = hasMipMaps;
		int num = 2048 / maxElementSize;
		int num2 = Mathf.Min(maxElementCount, num);
		int num3 = ((maxElementCount < num) ? 1 : Mathf.CeilToInt(maxElementCount / num));
		m_Atlas = new RenderTexture(num2 * maxElementSize, num3 * maxElementSize, 0, format)
		{
			volumeDepth = maxElementSize,
			dimension = TextureDimension.Tex3D,
			hideFlags = HideFlags.HideAndDontSave,
			enableRandomWrite = true,
			useMipMap = hasMipMaps,
			autoGenerateMips = false,
			name = $"Texture 3D Atlas - {num2 * maxElementSize}x{num3 * maxElementSize}x{maxElementSize}"
		};
		m_Atlas.Create();
		m_MipMapGenerationTemp = new RenderTexture(maxElementSize / 4, maxElementSize / 4, 0, format)
		{
			volumeDepth = maxElementSize / 4,
			dimension = TextureDimension.Tex3D,
			hideFlags = HideFlags.HideAndDontSave,
			enableRandomWrite = true,
			useMipMap = hasMipMaps,
			autoGenerateMips = false,
			name = $"Texture 3D MipMap Temp - {maxElementSize / 4}x{maxElementSize / 4}x{maxElementSize / 4}"
		};
		m_MipMapGenerationTemp.Create();
		for (int i = 0; i < maxElementCount; i++)
		{
			AtlasElement item = new AtlasElement(new Vector3Int(i % num2 * maxElementSize, Mathf.FloorToInt((float)i / (float)num2) * maxElementSize, 0), maxElementSize);
			m_Elements.Add(item);
		}
		m_Texture3DAtlasCompute = texture3DAtlasCompute;
		m_CopyKernel = m_Texture3DAtlasCompute.FindKernel("Copy");
		m_GenerateMipKernel = m_Texture3DAtlasCompute.FindKernel("GenerateMipMap");
		m_Texture3DAtlasCompute.GetKernelThreadGroupSizes(m_CopyKernel, out var x, out var y, out var z);
		m_KernelGroupSize = new Vector3Int((int)x, (int)y, (int)z);
	}

	private int GetTextureDepth(Texture t)
	{
		if (t is Texture3D texture3D)
		{
			return texture3D.depth;
		}
		if (t is RenderTexture renderTexture)
		{
			return renderTexture.volumeDepth;
		}
		return 0;
	}

	protected int GetTextureHash(Texture texture)
	{
		int hashCode = texture.GetHashCode();
		hashCode = 23 * hashCode + texture.GetInstanceID().GetHashCode();
		hashCode = 23 * hashCode + texture.graphicsFormat.GetHashCode();
		hashCode = 23 * hashCode + texture.width.GetHashCode();
		hashCode = 23 * hashCode + texture.height.GetHashCode();
		return 23 * hashCode + texture.updateCount.GetHashCode();
	}

	public bool IsTextureValid(Texture tex)
	{
		if (tex.width != tex.height || tex.height != GetTextureDepth(tex))
		{
			Debug.LogError($"3D Texture Atlas: Added texture {tex} is not doesn't have a cubic size {tex.width}x{tex.height}x{GetTextureDepth(tex)}.");
			return false;
		}
		if (tex.width > m_MaxElementSize)
		{
			Debug.LogError($"3D Texture Atlas: Added texture {tex} size {tex.width} is bigger than the max element atlas size {m_MaxElementSize}.");
			return false;
		}
		if (tex.width < 1)
		{
			Debug.LogError($"3D Texture Atlas: Added texture {tex} size {tex.width} is smaller than 1.");
			return false;
		}
		if (!Mathf.IsPowerOfTwo(tex.width))
		{
			Debug.LogError($"3D Texture Atlas: Added texture {tex} size {tex.width} is not power of two.");
			return false;
		}
		return true;
	}

	public bool AddTexture(Texture tex)
	{
		if (m_TextureElementsMap.ContainsKey(tex))
		{
			return true;
		}
		if (!IsTextureValid(tex))
		{
			return false;
		}
		if (!TryAddTextureToTree(tex))
		{
			return false;
		}
		return true;
	}

	private bool TryAddTextureToTree(Texture tex)
	{
		if (tex.width == m_MaxElementSize)
		{
			AtlasElement atlasElement = m_Elements.FirstOrDefault((AtlasElement e) => e.IsFree());
			if (atlasElement != null)
			{
				SetTextureToElem(atlasElement, tex);
				return true;
			}
			return false;
		}
		AtlasElement atlasElement2 = FindFreeElementWithSize(tex.width);
		if (atlasElement2 != null)
		{
			SetTextureToElem(atlasElement2, tex);
			return true;
		}
		atlasElement2 = m_Elements.FirstOrDefault((AtlasElement e) => e.IsFree());
		if (atlasElement2 == null)
		{
			return true;
		}
		while (atlasElement2.size > tex.width)
		{
			atlasElement2.PopulateChildren();
			atlasElement2 = atlasElement2.children[0];
		}
		SetTextureToElem(atlasElement2, tex);
		return true;
		void SetTextureToElem(AtlasElement element, Texture texture)
		{
			element.texture = texture;
			m_TextureElementsMap.Add(texture, element);
		}
	}

	private AtlasElement FindFreeElementWithSize(int size)
	{
		foreach (AtlasElement element in m_Elements)
		{
			AtlasElement atlasElement = FindFreeElement(size, element);
			if (atlasElement != null)
			{
				return atlasElement;
			}
		}
		return null;
		static AtlasElement FindFreeElement(int size, AtlasElement elem)
		{
			if (elem.size == size)
			{
				if (elem.IsFree())
				{
					return elem;
				}
				return null;
			}
			if (elem.children == null)
			{
				return null;
			}
			AtlasElement[] children = elem.children;
			foreach (AtlasElement atlasElement2 in children)
			{
				if (atlasElement2.children != null && atlasElement2.size >= size)
				{
					AtlasElement atlasElement3 = FindFreeElement(size, atlasElement2);
					if (atlasElement3 != null)
					{
						return atlasElement3;
					}
				}
				else if (atlasElement2.IsFree())
				{
					return atlasElement2;
				}
			}
			return null;
		}
	}

	public void RemoveTexture(Texture tex)
	{
		if (m_TextureElementsMap.TryGetValue(tex, out var value))
		{
			value.texture = null;
			if (value.parent != null)
			{
				value.parent.RemoveChildrenIfEmpty();
			}
			m_TextureElementsMap.Remove(tex);
		}
	}

	public void ClearTextures()
	{
		foreach (AtlasElement element in m_Elements)
		{
			element.texture = null;
			element.children = null;
		}
		m_TextureElementsMap.Clear();
	}

	public Vector3 GetTextureOffset(Texture tex)
	{
		if (tex != null && m_TextureElementsMap.TryGetValue(tex, out var value))
		{
			return value.position;
		}
		return -Vector3.one;
	}

	public void Update(CommandBuffer cmd)
	{
		if (m_TextureElementsMap.Count == 0)
		{
			return;
		}
		foreach (AtlasElement element in m_Elements)
		{
			Texture texture = element.texture;
			if (!(texture == null) && texture.width != element.size)
			{
				RemoveTexture(texture);
				AddTexture(texture);
			}
		}
		foreach (AtlasElement value in m_TextureElementsMap.Values)
		{
			if (!(value.texture == null))
			{
				int textureHash = GetTextureHash(value.texture);
				if (value.hash != textureHash)
				{
					value.hash = textureHash;
					CopyTexture(cmd, value);
				}
			}
		}
	}

	private void CopyTexture(CommandBuffer cmd, AtlasElement element)
	{
		CopyMip(cmd, element.texture, 0, m_Atlas, element.position, 0);
		if (!m_HasMipMaps)
		{
			return;
		}
		int num = ((!m_HasMipMaps) ? 1 : (Mathf.FloorToInt(Mathf.Log(element.texture.width, 2f)) + 1));
		if (element.texture.mipmapCount > 1)
		{
			CopyMips(cmd, element.texture, m_Atlas, element.position);
			return;
		}
		GenerateMip(cmd, element.texture, Vector3Int.zero, 0, m_Atlas, element.position, 1);
		MipGenerationSwapData mipGenerationSwapData = default(MipGenerationSwapData);
		mipGenerationSwapData.target = m_Atlas;
		mipGenerationSwapData.offset = element.position;
		mipGenerationSwapData.mipOffset = 0;
		MipGenerationSwapData mipGenerationSwapData2 = mipGenerationSwapData;
		int num2 = (int)Mathf.Log(m_MipMapGenerationTemp.width / (element.size >> 2), 2f);
		mipGenerationSwapData = default(MipGenerationSwapData);
		mipGenerationSwapData.target = m_MipMapGenerationTemp;
		mipGenerationSwapData.offset = Vector3Int.zero;
		mipGenerationSwapData.mipOffset = num2 - 2;
		MipGenerationSwapData mipGenerationSwapData3 = mipGenerationSwapData;
		for (int i = 2; i < num; i++)
		{
			GenerateMip(cmd, mipGenerationSwapData2.target, mipGenerationSwapData2.offset, i + mipGenerationSwapData2.mipOffset - 1, mipGenerationSwapData3.target, mipGenerationSwapData3.offset, i + mipGenerationSwapData3.mipOffset);
			MipGenerationSwapData mipGenerationSwapData4 = mipGenerationSwapData2;
			mipGenerationSwapData2 = mipGenerationSwapData3;
			mipGenerationSwapData3 = mipGenerationSwapData4;
		}
		for (int j = 2; j < num; j += 2)
		{
			CopyMip(destinationOffset: new Vector3Int(element.position.x >> j, element.position.y >> j, element.position.z >> j), cmd: cmd, source: m_MipMapGenerationTemp, sourceMip: j - 2 + num2, destination: m_Atlas, destinationMip: j);
		}
	}

	private void CopyMips(CommandBuffer cmd, Texture source, Texture destination, Vector3Int destinationOffset)
	{
		int num = Mathf.FloorToInt(Mathf.Log(source.width, 2f)) + 1;
		for (int i = 1; i < num; i++)
		{
			Vector3Int destinationOffset2 = new Vector3Int(destinationOffset.x >> i, destinationOffset.y >> i, destinationOffset.z >> i);
			CopyMip(cmd, source, i, destination, destinationOffset2, i);
		}
	}

	private void CopyMip(CommandBuffer cmd, Texture source, int sourceMip, Texture destination, Vector3Int destinationOffset, int destinationMip)
	{
		cmd.SetComputeTextureParam(m_Texture3DAtlasCompute, m_CopyKernel, ShaderPropertyID._Src3DTexture, source);
		cmd.SetComputeFloatParam(m_Texture3DAtlasCompute, ShaderPropertyID._SrcMip, sourceMip);
		cmd.SetComputeTextureParam(m_Texture3DAtlasCompute, m_CopyKernel, ShaderPropertyID._Dst3DTexture, destination, destinationMip);
		cmd.SetComputeVectorParam(m_Texture3DAtlasCompute, ShaderPropertyID._DstOffset, (Vector3)destinationOffset);
		bool flag = source is Texture3D texture3D && texture3D.format == TextureFormat.Alpha8;
		cmd.SetComputeFloatParam(m_Texture3DAtlasCompute, ShaderPropertyID._AlphaOnlyTexture, flag ? 1 : 0);
		int num = source.width >> sourceMip;
		cmd.SetComputeIntParam(m_Texture3DAtlasCompute, ShaderPropertyID._SrcSize, num);
		cmd.DispatchCompute(m_Texture3DAtlasCompute, m_CopyKernel, Mathf.Max(num / m_KernelGroupSize.x, 1), Mathf.Max(num / m_KernelGroupSize.y, 1), Mathf.Max(num / m_KernelGroupSize.z, 1));
	}

	private void GenerateMip(CommandBuffer cmd, Texture source, Vector3Int sourceOffset, int sourceMip, Texture destination, Vector3Int destinationOffset, int destinationMip)
	{
		Vector3 vector = new Vector3((float)sourceOffset.x / (float)source.width, (float)sourceOffset.y / (float)source.height, (float)sourceOffset.z / (float)GetTextureDepth(source));
		Vector3Int vector3Int = new Vector3Int(destinationOffset.x >> destinationMip, destinationOffset.y >> destinationMip, destinationOffset.z >> destinationMip);
		new Vector3Int(Mathf.Min(source.width, destination.width), Mathf.Min(source.height, destination.height), Mathf.Min(GetTextureDepth(source), GetTextureDepth(destination)));
		new Vector3Int(destination.width >> destinationMip, destination.height >> destinationMip, GetTextureDepth(destination) >> destinationMip);
		Vector3 one = Vector3.one;
		Vector3Int vector3Int2 = new Vector3Int(source.width >> sourceMip + 1, source.height >> sourceMip + 1, GetTextureDepth(source) >> sourceMip + 1);
		Vector3Int vector3Int3 = new Vector3Int(destination.width >> destinationMip, destination.height >> destinationMip, GetTextureDepth(destination) >> destinationMip);
		one = new Vector3(Mathf.Min((float)vector3Int3.x / (float)vector3Int2.x, 1f), Mathf.Min((float)vector3Int3.y / (float)vector3Int2.y, 1f), Mathf.Min((float)vector3Int3.z / (float)vector3Int2.z, 1f));
		cmd.SetComputeTextureParam(m_Texture3DAtlasCompute, m_GenerateMipKernel, ShaderPropertyID._Src3DTexture, source);
		cmd.SetComputeVectorParam(m_Texture3DAtlasCompute, ShaderPropertyID._SrcScale, one);
		cmd.SetComputeVectorParam(m_Texture3DAtlasCompute, ShaderPropertyID._SrcOffset, vector);
		cmd.SetComputeFloatParam(m_Texture3DAtlasCompute, ShaderPropertyID._SrcMip, sourceMip);
		cmd.SetComputeTextureParam(m_Texture3DAtlasCompute, m_GenerateMipKernel, ShaderPropertyID._Dst3DTexture, destination, destinationMip);
		cmd.SetComputeVectorParam(m_Texture3DAtlasCompute, ShaderPropertyID._DstOffset, (Vector3)vector3Int);
		int num = Mathf.Min(GetTextureDepth(source) >> sourceMip + 1, GetTextureDepth(destination) >> destinationMip);
		cmd.SetComputeIntParam(m_Texture3DAtlasCompute, ShaderPropertyID._SrcSize, num);
		bool flag = source is Texture3D texture3D && texture3D.format == TextureFormat.Alpha8;
		cmd.SetComputeFloatParam(m_Texture3DAtlasCompute, ShaderPropertyID._AlphaOnlyTexture, flag ? 1 : 0);
		cmd.DispatchCompute(m_Texture3DAtlasCompute, m_GenerateMipKernel, Mathf.Max(num / m_KernelGroupSize.x, 1), Mathf.Max(num / m_KernelGroupSize.y, 1), Mathf.Max(num / m_KernelGroupSize.z, 1));
	}

	public RenderTexture GetAtlas()
	{
		return m_Atlas;
	}

	public void Release()
	{
		ClearTextures();
		CoreUtils.Destroy(m_Atlas);
		CoreUtils.Destroy(m_MipMapGenerationTemp);
	}

	public static long GetApproxCacheSizeInByte(int elementSize, int elementCount, GraphicsFormat format, bool hasMipMaps)
	{
		int formatSizeInBytes = RenderingUtils.GetFormatSizeInBytes(format);
		return (long)((float)(elementSize * elementSize * elementSize * formatSizeInBytes) * (hasMipMaps ? 1.33f : 1f)) * elementCount;
	}

	public static int GetMaxElementCountForWeightInByte(long weight, int elementSize, int elementCount, GraphicsFormat format, bool hasMipMaps)
	{
		long num = (long)((float)((long)elementSize * (long)elementSize * elementSize * RenderingUtils.GetFormatSizeInBytes(format)) * (hasMipMaps ? 1.33f : 1f));
		return (int)Mathf.Clamp(weight / num, 1f, elementCount);
	}
}
