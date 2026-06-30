using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class AugmentationAtlasController : IDisposable
{
	private Texture2D _diffuseAtlas;

	private Texture2D _normalAtlas;

	private Texture2D _masksAtlas;

	private CharacterAtlasData _atlasData;

	private int _atlasSize;

	private int _fullAtlasSize;

	private DxtCompressorServiceNew _compressor;

	private readonly Dictionary<BodyPartType, RectInt> _slotRects = new Dictionary<BodyPartType, RectInt>();

	private readonly Dictionary<BodyPartType, Texture2D> _currentDiffuse = new Dictionary<BodyPartType, Texture2D>();

	private readonly Dictionary<BodyPartType, Texture2D> _currentNormal = new Dictionary<BodyPartType, Texture2D>();

	private readonly Dictionary<BodyPartType, Texture2D> _currentMasks = new Dictionary<BodyPartType, Texture2D>();

	private bool _isInitialized;

	private const int DownscaledAtlasSize = 1024;

	public static bool ShouldDownscale = true;

	public Texture2D DiffuseAtlas => _diffuseAtlas;

	public Texture2D NormalAtlas => _normalAtlas;

	public Texture2D MasksAtlas => _masksAtlas;

	public bool IsInitialized => _isInitialized;

	public void Initialize(CharacterAtlasData atlasData)
	{
		if (atlasData == null)
		{
			PFLog.TechArt.Error("[AugmentationAtlas] atlasData is null, cannot initialize");
			return;
		}
		_atlasData = atlasData;
		_fullAtlasSize = (int)atlasData.targetResolution / 4 * 4;
		int num = ((ShouldDownscale && GetCompressor() != null) ? Mathf.Min(_fullAtlasSize, 1024) : _fullAtlasSize);
		_atlasSize = num / 4 * 4;
		_slotRects.Clear();
		foreach (CharacterAtlasData.BodyPartCoords bodyPartsCoord in atlasData.BodyPartsCoords)
		{
			BodyPartType bodyPart = (BodyPartType)bodyPartsCoord.bodyPart;
			RectInt textureRectCoords = bodyPartsCoord.textureRectCoords;
			int xMin = ScaleToAtlas(textureRectCoords.x);
			int width = ScaleToAtlas(textureRectCoords.width);
			int num2 = ScaleToAtlas(textureRectCoords.height);
			int yMin = (_atlasSize - ScaleToAtlas(textureRectCoords.y) - num2) / 4 * 4;
			_slotRects[bodyPart] = new RectInt(xMin, yMin, width, num2);
		}
		_diffuseAtlas = CreateBlackDXT5Texture(_atlasSize, _atlasSize, linear: false, "AugmentationAtlas_Diffuse");
		_normalAtlas = CreateBlackDXT5Texture(_atlasSize, _atlasSize, linear: true, "AugmentationAtlas_Normal");
		_masksAtlas = CreateBlackDXT5Texture(_atlasSize, _atlasSize, linear: true, "AugmentationAtlas_Masks");
		_isInitialized = true;
		PFLog.TechArt.Log($"[AugmentationAtlas] Initialized {_atlasSize}x{_atlasSize}, {_slotRects.Count} slots");
	}

	public bool UpdateSlot(BodyPartType slot, Texture2D diffuse, Texture2D normal = null, Texture2D masks = null)
	{
		if (!_isInitialized)
		{
			PFLog.TechArt.Error("[AugmentationAtlas] Not initialized");
			return false;
		}
		if (!_slotRects.TryGetValue(slot, out var value))
		{
			PFLog.TechArt.Error($"[AugmentationAtlas] No slot rect for {slot}");
			return false;
		}
		if (diffuse != null)
		{
			if (!CopyTextureToAtlas(diffuse, _diffuseAtlas, value, "diffuse", slot))
			{
				return false;
			}
			_currentDiffuse[slot] = diffuse;
		}
		if (normal != null)
		{
			if (!CopyTextureToAtlas(normal, _normalAtlas, value, "normal", slot))
			{
				return false;
			}
			_currentNormal[slot] = normal;
		}
		if (masks != null)
		{
			if (!CopyTextureToAtlas(masks, _masksAtlas, value, "masks", slot))
			{
				return false;
			}
			_currentMasks[slot] = masks;
		}
		return true;
	}

	public void ClearSlot(BodyPartType slot)
	{
		_currentDiffuse.Remove(slot);
		_currentNormal.Remove(slot);
		_currentMasks.Remove(slot);
	}

	public bool HasSlot(BodyPartType slot)
	{
		return _slotRects.ContainsKey(slot);
	}

	public RectInt GetSlotRect(BodyPartType slot)
	{
		if (!_slotRects.TryGetValue(slot, out var value))
		{
			return default(RectInt);
		}
		return value;
	}

	public void ApplyToMaterial(Material material)
	{
		if (_isInitialized && !(material == null))
		{
			material.SetTexture("_AugmentDiffuse", _diffuseAtlas);
			material.SetTexture("_AugmentNormal", _normalAtlas);
			material.SetTexture("_AugmentMasks", _masksAtlas);
		}
	}

	private int ScaleToAtlas(int fullValue)
	{
		return (int)((long)fullValue * (long)_atlasSize / _fullAtlasSize / 4) * 4;
	}

	private DxtCompressorServiceNew GetCompressor()
	{
		if (_compressor == null)
		{
			_compressor = Services.GetInstance<DxtCompressorServiceNew>();
		}
		return _compressor;
	}

	private static Texture2D CreateBlackDXT5Texture(int width, int height, bool linear, string name)
	{
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.DXT5, mipChain: false, linear);
		texture2D.name = name;
		texture2D.filterMode = FilterMode.Bilinear;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		int num = width / 4 * (height / 4);
		byte[] array = new byte[num * 16];
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 16;
			array[num2] = byte.MaxValue;
			array[num2 + 1] = byte.MaxValue;
		}
		texture2D.LoadRawTextureData(array);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		return texture2D;
	}

	private bool CopyTextureToAtlas(Texture2D source, Texture2D atlas, RectInt rect, string channel, BodyPartType slot)
	{
		if (source.format != TextureFormat.DXT5)
		{
			PFLog.TechArt.Error($"[AugmentationAtlas] {channel} texture for {slot} is {source.format}, expected DXT5");
			return false;
		}
		Texture2D source2 = source;
		Texture2D texture2D = null;
		if (_atlasSize != _fullAtlasSize)
		{
			int targetWidth = Mathf.Max(4, ScaleToAtlas(source.width));
			int targetHeight = Mathf.Max(4, ScaleToAtlas(source.height));
			DxtCompressorServiceNew compressor = GetCompressor();
			if (compressor == null)
			{
				PFLog.TechArt.Error("[AugmentationAtlas] DxtCompressorServiceNew unavailable, cannot downscale slot texture");
				return false;
			}
			texture2D = compressor.CompressTextureGPUSync(source, targetWidth, targetHeight, channel);
			if (texture2D == null)
			{
				PFLog.TechArt.Error($"[AugmentationAtlas] {channel} downscale failed for {slot}");
				return false;
			}
			source2 = texture2D;
		}
		bool result = CopyBlocks(source2, atlas, rect, channel, slot);
		if (texture2D != null)
		{
			UnityEngine.Object.Destroy(texture2D);
		}
		return result;
	}

	private static bool CopyBlocks(Texture2D source, Texture2D atlas, RectInt rect, string channel, BodyPartType slot)
	{
		if (source.width > rect.width || source.height > rect.height)
		{
			PFLog.TechArt.Error($"[AugmentationAtlas] {channel} texture for {slot} ({source.width}x{source.height}) exceeds slot ({rect.width}x{rect.height})");
			return false;
		}
		if (rect.x % 4 != 0 || rect.y % 4 != 0 || source.width % 4 != 0 || source.height % 4 != 0)
		{
			PFLog.TechArt.Error($"[AugmentationAtlas] {channel} for {slot}: coordinates or dimensions not aligned to 4");
			return false;
		}
		Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, atlas, 0, 0, rect.x, rect.y);
		return true;
	}

	public void Dispose()
	{
		if (_diffuseAtlas != null)
		{
			UnityEngine.Object.Destroy(_diffuseAtlas);
			_diffuseAtlas = null;
		}
		if (_normalAtlas != null)
		{
			UnityEngine.Object.Destroy(_normalAtlas);
			_normalAtlas = null;
		}
		if (_masksAtlas != null)
		{
			UnityEngine.Object.Destroy(_masksAtlas);
			_masksAtlas = null;
		}
		_slotRects.Clear();
		_currentDiffuse.Clear();
		_currentNormal.Clear();
		_currentMasks.Clear();
		_isInitialized = false;
		PFLog.TechArt.Log("[AugmentationAtlas] Disposed");
	}
}
