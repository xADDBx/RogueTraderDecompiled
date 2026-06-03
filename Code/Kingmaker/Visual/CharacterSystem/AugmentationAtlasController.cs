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

	private readonly Dictionary<BodyPartType, RectInt> _slotRects = new Dictionary<BodyPartType, RectInt>();

	private readonly Dictionary<BodyPartType, Texture2D> _currentDiffuse = new Dictionary<BodyPartType, Texture2D>();

	private readonly Dictionary<BodyPartType, Texture2D> _currentNormal = new Dictionary<BodyPartType, Texture2D>();

	private readonly Dictionary<BodyPartType, Texture2D> _currentMasks = new Dictionary<BodyPartType, Texture2D>();

	private bool _isInitialized;

	private const int DownscaledAtlasSize = 1024;

	public Texture2D DiffuseAtlas => _diffuseAtlas;

	public Texture2D NormalAtlas => _normalAtlas;

	public Texture2D MasksAtlas => _masksAtlas;

	public bool IsInitialized => _isInitialized;

	public static bool ShouldDownscale
	{
		get
		{
			if (Application.platform != RuntimePlatform.WindowsPlayer)
			{
				return Application.isEditor;
			}
			return true;
		}
	}

	public void Initialize(CharacterAtlasData atlasData)
	{
		if (atlasData == null)
		{
			PFLog.TechArt.Error("[AugmentationAtlas] atlasData is null, cannot initialize");
			return;
		}
		_atlasData = atlasData;
		_atlasSize = (int)atlasData.targetResolution;
		_atlasSize = _atlasSize / 4 * 4;
		_slotRects.Clear();
		foreach (CharacterAtlasData.BodyPartCoords bodyPartsCoord in atlasData.BodyPartsCoords)
		{
			BodyPartType bodyPart = (BodyPartType)bodyPartsCoord.bodyPart;
			RectInt textureRectCoords = bodyPartsCoord.textureRectCoords;
			int xMin = textureRectCoords.x / 4 * 4;
			int width = textureRectCoords.width / 4 * 4;
			int height = textureRectCoords.height / 4 * 4;
			int yMin = (_atlasSize - textureRectCoords.y - textureRectCoords.height) / 4 * 4;
			_slotRects[bodyPart] = new RectInt(xMin, yMin, width, height);
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

	public void DownscaleAtlases(int targetSize = 1024)
	{
		if (!_isInitialized)
		{
			return;
		}
		targetSize = targetSize / 4 * 4;
		if (targetSize > 0 && targetSize < _atlasSize)
		{
			DxtCompressorServiceNew instance = Services.GetInstance<DxtCompressorServiceNew>();
			if (instance == null)
			{
				PFLog.TechArt.Error("[AugmentationAtlas] DxtCompressorServiceNew unavailable, skipping downscale");
				return;
			}
			_diffuseAtlas = ReplaceWithDownscaled(instance, _diffuseAtlas, targetSize, "AugmentationAtlas_Diffuse");
			_normalAtlas = ReplaceWithDownscaled(instance, _normalAtlas, targetSize, "AugmentationAtlas_Normal");
			_masksAtlas = ReplaceWithDownscaled(instance, _masksAtlas, targetSize, "AugmentationAtlas_Masks");
			_atlasSize = targetSize;
			PFLog.TechArt.Log($"[AugmentationAtlas] Downscaled atlases to {targetSize}x{targetSize}");
		}
	}

	private static Texture2D ReplaceWithDownscaled(DxtCompressorServiceNew compressor, Texture2D source, int targetSize, string name)
	{
		if (source == null)
		{
			return null;
		}
		Texture2D texture2D = compressor.CompressTextureGPUSync(source, targetSize, targetSize, name);
		if (texture2D == null)
		{
			PFLog.TechArt.Error("[AugmentationAtlas] Downscale failed for " + name + ", keeping original");
			return source;
		}
		texture2D.name = name;
		texture2D.filterMode = source.filterMode;
		texture2D.wrapMode = source.wrapMode;
		UnityEngine.Object.Destroy(source);
		return texture2D;
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
