using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.DxtCompressor;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterAtlas
{
	public bool Destroyed;

	private List<CharacterTextureDescription> m_SortedTextures;

	private Dictionary<Texture, HashSet<BodyPartType>> m_TexturesTypesMap;

	private Dictionary<CharacterTextureDescription, Texture> m_PrimaryTextureMap;

	private CharacterAtlasData m_CharacterAtlasDataLocal;

	private Vector2 m_Size;

	private bool m_Baked;

	private Material m_BakeMaterial;

	private Material m_ShadowBakeMaterial;

	private Material m_DiffuseBakeMaterial;

	private Material m_RoughnessLightenBlend;

	private RenderTexture m_UncompressedTexture;

	private Action<CharacterAtlas, Texture2D> m_OnTextureCompressed;

	private Action<CharacterAtlas> m_OnTextureNotCompressed;

	private static RenderTexture m_TempLinearTexture;

	private static RenderTexture m_TempSRGBTexture;

	private readonly float ScaleFactor;

	public static bool MakeAtlasPowerOfTwo { get; set; }

	public Dictionary<Texture, Rect> Rects { get; private set; }

	public AtlasTexture AtlasTexture { get; private set; }

	public CharacterTextureChannel Channel { get; private set; }

	public void RefreshData()
	{
		m_TexturesTypesMap = new Dictionary<Texture, HashSet<BodyPartType>>();
		m_PrimaryTextureMap = new Dictionary<CharacterTextureDescription, Texture>();
		m_SortedTextures = new List<CharacterTextureDescription>();
	}

	public CharacterAtlas(int size, CharacterTextureChannel channel, CharacterAtlasData atlasData)
	{
		m_Size = new Vector2(size, size);
		m_CharacterAtlasDataLocal = atlasData;
		Rects = new Dictionary<Texture, Rect>();
		Channel = channel;
		m_BakeMaterial = new Material(Shader.Find("Hidden/CharacterAtlasGenerator"));
		m_ShadowBakeMaterial = new Material(Shader.Find("Hidden/CharacterShadowBake"));
		m_DiffuseBakeMaterial = new Material(Shader.Find("Hidden/CharacterDiffuseBake"));
		m_RoughnessLightenBlend = new Material(Shader.Find("Hidden/RoughnessLightenBlend"));
		ScaleFactor = (int)m_CharacterAtlasDataLocal.targetResolution / size;
	}

	public void AddPrimaryTexture(CharacterTextureDescription textureDesc, BodyPartType bodyPartType, bool shadow = false)
	{
		if (textureDesc == null)
		{
			PFLog.Default.Warning($"TextureDesc is null {bodyPartType}");
			return;
		}
		if (textureDesc.GetSourceTexture() == null)
		{
			PFLog.Default.Warning($"Character Source texture is null {bodyPartType}");
		}
		if (shadow && null == textureDesc.RampShadowTexture)
		{
			PFLog.Default.Warning($"RampShadow texture is null {bodyPartType}!");
			return;
		}
		if (Channel != 0)
		{
			PFLog.Default.Warning($"Can't add NonDiffuse texture as primary {bodyPartType} {textureDesc.GetMainTextureName()}");
		}
		if (textureDesc.Channel != Channel)
		{
			PFLog.Default.Warning($"Wrong texture channel {bodyPartType} {textureDesc.GetMainTextureName()}!");
			return;
		}
		HashSet<BodyPartType> value;
		bool flag = m_TexturesTypesMap.TryGetValue(textureDesc.GetSourceTexture(), out value);
		if (flag && value.Contains(bodyPartType))
		{
			m_TexturesTypesMap.Remove(textureDesc.GetSourceTexture());
			flag = false;
		}
		if (!flag)
		{
			value = new HashSet<BodyPartType>();
			m_TexturesTypesMap[textureDesc.GetSourceTexture()] = value;
			m_SortedTextures.Add(textureDesc);
		}
		value.Add(bodyPartType);
	}

	public void AddSecondaryTexture(CharacterTextureDescription textureDesc, Texture primaryTexture, BodyPartType bodyPartType, Material material)
	{
		if (!textureDesc.IsEmpty && (textureDesc.GetSourceTexture().width != primaryTexture.width || textureDesc.GetSourceTexture().height != primaryTexture.height))
		{
			PFLog.Default.Error("Textures must be the same size " + textureDesc.GetMainTextureName() + " - " + primaryTexture.name);
		}
		textureDesc.Material = material;
		if (!m_TexturesTypesMap.TryGetValue(primaryTexture, out var value))
		{
			value = new HashSet<BodyPartType>();
			m_TexturesTypesMap[primaryTexture] = value;
			m_SortedTextures.Add(textureDesc);
		}
		value.Add(bodyPartType);
		if (!m_PrimaryTextureMap.ContainsKey(textureDesc))
		{
			m_PrimaryTextureMap.Add(textureDesc, primaryTexture);
		}
	}

	public void AddSecondaryTextureEmpty(Texture primaryTexture, BodyPartType bodyPartType, Material material)
	{
		CharacterTextureDescription characterTextureDescription = new CharacterTextureDescription(Channel, null);
		characterTextureDescription.IsEmpty = true;
		AddSecondaryTexture(characterTextureDescription, primaryTexture, bodyPartType, material);
	}

	private void CreateAtlasTexture()
	{
		if (AtlasTexture == null || AtlasTexture.Destroyed || !AtlasTexture.CompressionComplete || AtlasTexture.Texture == null || !AtlasTexture.Texture.isReadable)
		{
			if (AtlasTexture != null)
			{
				if (AtlasTexture.CompressionComplete && AtlasTexture.Texture != null && !AtlasTexture.Texture.isReadable)
				{
					UnityEngine.Object.Destroy(AtlasTexture.Texture);
				}
				else
				{
					AtlasTexture.Destroyed = true;
				}
			}
			bool linear = Channel != CharacterTextureChannel.Diffuse;
			Texture2D texture = new Texture2D((int)m_Size.x, (int)m_Size.y, TextureFormat.DXT5, mipChain: true, linear)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Repeat,
				anisoLevel = 1,
				name = $"{Channel}_2D"
			};
			AtlasTexture = new AtlasTexture
			{
				Texture = texture
			};
		}
		else
		{
			AtlasTexture.CompressionComplete = false;
		}
	}

	public void Build(EquipmentEntity.PaintedTextures paintedTextures, Material material, bool cleanAtlas, bool delayTextureCreation)
	{
		CalculateRects();
		if (!delayTextureCreation)
		{
			CreateAtlasTexture();
		}
		RenderTexture renderTexture = new RenderTexture((int)m_Size.x, (int)m_Size.y, 0, RenderTextureFormat.ARGB32, (Channel != 0) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
		renderTexture.filterMode = FilterMode.Bilinear;
		renderTexture.wrapMode = TextureWrapMode.Repeat;
		renderTexture.anisoLevel = 1;
		renderTexture.useMipMap = true;
		renderTexture.autoGenerateMips = true;
		renderTexture.name = $"{Channel}_RT";
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		if (!m_Baked || cleanAtlas)
		{
			Color backgroundColor = new Color(0f, 0f, 0f, 0f);
			if (Channel == CharacterTextureChannel.Masks)
			{
				backgroundColor = new Color(0.85f, 0f, 0f, 0f);
			}
			if (Channel == CharacterTextureChannel.Normal)
			{
				backgroundColor = new Color(0.5f, 0.5f, 1f, 1f);
			}
			GL.Clear(clearDepth: false, clearColor: true, backgroundColor);
		}
		RenderTexture renderTexture2 = null;
		if (Channel == CharacterTextureChannel.Diffuse)
		{
			if (m_TempSRGBTexture != null && (m_TempSRGBTexture.width != (int)m_Size.x || m_TempSRGBTexture.height != (int)m_Size.y))
			{
				m_TempSRGBTexture.Release();
				m_TempSRGBTexture = null;
			}
			if (m_TempSRGBTexture == null)
			{
				m_TempSRGBTexture = new RenderTexture((int)m_Size.x, (int)m_Size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			}
			m_TempSRGBTexture.DiscardContents();
			renderTexture2 = m_TempSRGBTexture;
		}
		else
		{
			if (m_TempLinearTexture != null && (m_TempLinearTexture.width != (int)m_Size.x || m_TempLinearTexture.height != (int)m_Size.y))
			{
				m_TempLinearTexture.Release();
				m_TempLinearTexture = null;
			}
			if (m_TempLinearTexture == null)
			{
				m_TempLinearTexture = new RenderTexture((int)m_Size.x, (int)m_Size.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			}
			m_TempLinearTexture.DiscardContents();
			renderTexture2 = m_TempLinearTexture;
		}
		renderTexture2.filterMode = FilterMode.Bilinear;
		renderTexture2.wrapMode = TextureWrapMode.Repeat;
		renderTexture2.anisoLevel = 1;
		if (!renderTexture2.useMipMap)
		{
			if (renderTexture2.IsCreated())
			{
				renderTexture2.Release();
			}
			renderTexture2.useMipMap = true;
		}
		renderTexture2.autoGenerateMips = true;
		renderTexture2.name = $"{Channel}_prevRT";
		if (!renderTexture2.IsCreated())
		{
			renderTexture2.Create();
		}
		m_BakeMaterial.SetTexture(ShaderProps._PreviousTex, renderTexture2);
		foreach (CharacterTextureDescription sortedTexture in m_SortedTextures)
		{
			Rect rect = ((Channel != 0) ? Rects[m_PrimaryTextureMap[sortedTexture]] : Rects[sortedTexture.GetSourceTexture()]);
			float num = (float)sortedTexture.ActiveTexture.width / ScaleFactor;
			float num2 = (float)sortedTexture.ActiveTexture.height / ScaleFactor;
			m_BakeMaterial.SetVector(ShaderProps._SrcRect, new Vector4(0f, 0f, rect.width / num, rect.height / num2));
			m_ShadowBakeMaterial.SetVector(ShaderProps._SrcRect, new Vector4(0f, 0f, rect.width / num, rect.height / num2));
			m_DiffuseBakeMaterial.SetVector(ShaderProps._SrcRect, new Vector4(0f, 0f, rect.width / num, rect.height / num2));
			m_RoughnessLightenBlend.SetVector(ShaderProps._SrcRect, new Vector4(0f, 0f, rect.width / num, rect.height / num2));
			m_BakeMaterial.SetVector(ShaderProps._DstRect, new Vector4(rect.x / m_Size.x, rect.y / m_Size.y, rect.width / m_Size.x, rect.height / m_Size.y));
			m_ShadowBakeMaterial.SetVector(ShaderProps._DstRect, new Vector4(rect.x / m_Size.x, rect.y / m_Size.y, rect.width / m_Size.x, rect.height / m_Size.y));
			m_DiffuseBakeMaterial.SetVector(ShaderProps._DstRect, new Vector4(rect.x / m_Size.x, rect.y / m_Size.y, rect.width / m_Size.x, rect.height / m_Size.y));
			m_RoughnessLightenBlend.SetVector(ShaderProps._DstRect, new Vector4(rect.x / m_Size.x, rect.y / m_Size.y, rect.width / m_Size.x, rect.height / m_Size.y));
			m_BakeMaterial.SetInt(ShaderProps._IsEmpty, sortedTexture.IsEmpty ? 1 : 0);
			if (sortedTexture.Material != null)
			{
				m_BakeMaterial.SetFloat(ShaderProps._Roughness, sortedTexture.Material.GetFloat(ShaderProps._Roughness));
				m_BakeMaterial.SetFloat(ShaderProps._Emission, sortedTexture.Material.GetFloat(ShaderProps._Emission));
				m_BakeMaterial.SetFloat(ShaderProps._Metallic, sortedTexture.Material.GetFloat(ShaderProps._Metallic));
			}
			else
			{
				m_BakeMaterial.SetFloat(ShaderProps._Roughness, 1f);
				m_BakeMaterial.SetFloat(ShaderProps._Emission, 1f);
				m_BakeMaterial.SetFloat(ShaderProps._Metallic, 1f);
			}
			m_BakeMaterial.SetTexture(ShaderProps._AlphaMask, sortedTexture.DiffuseTexture);
			if (Channel != 0)
			{
				m_BakeMaterial.EnableKeyword("ALPHA_MASK_ON");
				if (Channel == CharacterTextureChannel.Normal)
				{
					m_BakeMaterial.EnableKeyword("NORMAL_MAP_ON");
					material.SetFloat(ShaderProps._UseNormalMapAtlas, 1f);
				}
			}
			else
			{
				m_BakeMaterial.DisableKeyword("ALPHA_MASK_ON");
				m_BakeMaterial.DisableKeyword("NORMAL_MAP_ON");
				material.SetFloat(ShaderProps._UseNormalMapAtlas, 0f);
			}
			Texture source = (Texture)(((object)paintedTextures.Get(sortedTexture)) ?? ((object)sortedTexture.ActiveTexture));
			if (Channel == CharacterTextureChannel.Diffuse)
			{
				if (sortedTexture.UseShadowMask)
				{
					m_ShadowBakeMaterial.SetTexture(ShaderProps._Mask, sortedTexture.RampShadowTexture);
					Graphics.Blit(source, renderTexture, m_ShadowBakeMaterial);
					if (!sortedTexture.ActiveTexture.name.EndsWith("_D", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					Graphics.Blit(source, renderTexture, m_DiffuseBakeMaterial);
					Graphics.Blit(renderTexture, renderTexture2);
				}
				source = (Texture)(((object)paintedTextures.Get(sortedTexture)) ?? ((object)sortedTexture.ActiveTexture));
			}
			else if (Channel == CharacterTextureChannel.Normal)
			{
				source = sortedTexture.NormalTexture;
			}
			else if (Channel == CharacterTextureChannel.Masks)
			{
				source = sortedTexture.MaskTexture;
				if (sortedTexture.UseShadowMask && null != sortedTexture.RampShadowTexture)
				{
					RenderTexture temporary = RenderTexture.GetTemporary(sortedTexture.RampShadowTexture.width, sortedTexture.RampShadowTexture.height, 0, RenderTextureFormat.ARGB32);
					Graphics.Blit(sortedTexture.RampShadowTexture, temporary);
					Graphics.Blit(temporary, renderTexture, m_RoughnessLightenBlend);
					Graphics.Blit(renderTexture, renderTexture2);
					RenderTexture.ReleaseTemporary(temporary);
				}
			}
			Graphics.Blit(source, renderTexture, m_BakeMaterial);
			Graphics.Blit(renderTexture, renderTexture2);
		}
		UpdateMaterial(material, renderTexture);
		RenderTexture.active = active;
		m_Baked = true;
		if (m_UncompressedTexture != null)
		{
			m_UncompressedTexture.Release();
			UnityEngine.Object.Destroy(m_UncompressedTexture);
		}
		m_UncompressedTexture = renderTexture;
	}

	public void CompressAsync(DxtCompressorService dxtCompressorService, Action<CharacterAtlas, Texture2D> onTextureCompressed, Action<CharacterAtlas> onTextureNotCompressed)
	{
		if (m_UncompressedTexture == null)
		{
			onTextureNotCompressed(this);
			return;
		}
		CreateAtlasTexture();
		m_OnTextureCompressed = onTextureCompressed;
		m_OnTextureNotCompressed = onTextureNotCompressed;
		if (dxtCompressorService != null)
		{
			dxtCompressorService.CompressTexture(m_UncompressedTexture, AtlasTexture.Texture, DxtCompressorService.Compression.Dxt5, 0).OnDone += HandleCompressionDone;
		}
	}

	private void HandleCompressionDone(DxtCompressorService.Request request)
	{
		try
		{
			if (request.TextureOut == null || AtlasTexture.Destroyed || m_UncompressedTexture != request.TextureIn || !request.TextureOut.isReadable)
			{
				m_OnTextureNotCompressed?.Invoke(this);
				if (request.TextureOut != null)
				{
					UnityEngine.Object.Destroy(request.TextureOut);
				}
				return;
			}
			AtlasTexture.CompressionComplete = true;
			if (request.HasError)
			{
				PFLog.Default.Error("Failed to compress atlas to DXT: " + request.ErrorText);
				m_OnTextureNotCompressed?.Invoke(this);
			}
			else
			{
				request.TextureOut.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				m_OnTextureCompressed?.Invoke(this, request.TextureOut);
			}
		}
		finally
		{
			request.OnDone -= HandleCompressionDone;
			if (m_UncompressedTexture == request.TextureIn)
			{
				ClearTempValues();
			}
			else if (request.TextureIn != null)
			{
				((RenderTexture)request.TextureIn).Release();
				UnityEngine.Object.Destroy(request.TextureIn);
			}
		}
	}

	public void ClearTempValues()
	{
		if (m_UncompressedTexture != null)
		{
			m_UncompressedTexture.Release();
			UnityEngine.Object.Destroy(m_UncompressedTexture);
			m_UncompressedTexture = null;
		}
		m_OnTextureCompressed = null;
		m_OnTextureNotCompressed = null;
	}

	public void UpdateMaterial(Material material, Texture tex)
	{
		switch (Channel)
		{
		case CharacterTextureChannel.Diffuse:
			material.SetTexture(ShaderProps._BaseMap, tex);
			break;
		case CharacterTextureChannel.Normal:
			material.EnableKeyword("_NORMALMAP");
			material.SetTexture(ShaderProps._BumpMap, tex);
			break;
		case CharacterTextureChannel.Masks:
			material.EnableKeyword("_MASKSMAP");
			material.SetTexture(ShaderProps._MasksMap, tex);
			break;
		}
	}

	private void CalculateRects()
	{
		Rects.Clear();
		Dictionary<BodyPartType, Rect> dictionary = new Dictionary<BodyPartType, Rect>();
		foreach (KeyValuePair<Texture, HashSet<BodyPartType>> item in m_TexturesTypesMap)
		{
			bool flag = false;
			Rect value = new Rect(0f, 0f, 0f, 0f);
			foreach (BodyPartType item2 in item.Value)
			{
				flag = dictionary.TryGetValue(item2, out value);
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				foreach (BodyPartType item3 in item.Value)
				{
					value = (dictionary[item3] = GetMappedRect(item3));
				}
			}
			Rects[item.Key] = value;
		}
	}

	public Rect GetMappedRect(BodyPartType type)
	{
		Rect result = Rect.zero;
		foreach (CharacterAtlasData.BodyPartCoords bodyPartsCoord in m_CharacterAtlasDataLocal.BodyPartsCoords)
		{
			if (bodyPartsCoord.bodyPart == (long)type)
			{
				result = new Rect(bodyPartsCoord.gpuCoords.x, bodyPartsCoord.gpuCoords.y, bodyPartsCoord.textureRectCoords.width, bodyPartsCoord.textureRectCoords.height);
			}
		}
		result.x /= ScaleFactor;
		result.y /= ScaleFactor;
		result.width /= ScaleFactor;
		result.height /= ScaleFactor;
		return result;
	}
}
