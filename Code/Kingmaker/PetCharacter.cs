using System.Collections;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Visual.CharacterSystem;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker;

public class PetCharacter : MonoBehaviour
{
	public Shader EquipmentColorizerShader;

	public Material PetOriginalMaterial;

	[Header("Pet Ramps from PET")]
	public CharacterColorsProfile CharacterColorsProfileFile;

	public RampColorPreset RampColorPresetFile;

	[SerializeField]
	private int m_PetRamp01Index = -1;

	[SerializeField]
	private int m_PetRamp02Index = -1;

	[Header("Pet Color Mask")]
	public Texture2D petColorMask;

	public int PetRamp01Index
	{
		get
		{
			return m_PetRamp01Index;
		}
		set
		{
			m_PetRamp01Index = value;
		}
	}

	public int PetRamp02Index
	{
		get
		{
			return m_PetRamp02Index;
		}
		set
		{
			m_PetRamp02Index = value;
		}
	}

	private IEnumerator ApplyDefaultRampsCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		PFLog.TechArt.Log("PetCharacter.ApplyDefaultRampsCoroutine: Applying default ramps (0,0) to: " + base.name);
		if (m_PetRamp01Index < 0 || m_PetRamp02Index < 0)
		{
			RampColorPreset.IndexSet indexSet = RampColorPresetFile.IndexPairs[0];
			m_PetRamp01Index = indexSet.PrimaryIndex;
			m_PetRamp02Index = indexSet.SecondaryIndex;
			ApplyRampsByIndicesFromOwnPresets(indexSet.PrimaryIndex, indexSet.SecondaryIndex);
		}
	}

	public Texture2D GetRampTextureByIndex(int rampIndex)
	{
		if (CharacterColorsProfileFile?.Ramps == null || CharacterColorsProfileFile.Ramps.Count == 0)
		{
			PFLog.TechArt.Warning("GetRampTextureByIndex: No ramps available in CharacterColorsProfileFile");
			return null;
		}
		if (rampIndex < 0 || rampIndex >= CharacterColorsProfileFile.Ramps.Count)
		{
			PFLog.TechArt.Warning($"GetRampTextureByIndex: Ramp index {rampIndex} out of range. Available ramps: {CharacterColorsProfileFile.Ramps.Count}");
			return null;
		}
		Texture2D texture2D = CharacterColorsProfileFile.Ramps[rampIndex];
		PFLog.TechArt.Log($"GetRampTextureByIndex: Retrieved ramp texture {texture2D?.name} for index {rampIndex}");
		return texture2D;
	}

	public void ApplyRampsByIndicesFromOwnPresets(int primaryIndex, int secondaryIndex)
	{
		if (RampColorPresetFile?.IndexPairs == null || RampColorPresetFile.IndexPairs.Count == 0)
		{
			PFLog.TechArt.Warning("ApplyRampsByIndicesFromOwnPresets: No presets available in RampColorPresetFile");
			return;
		}
		RampColorPreset.IndexSet indexSet = null;
		foreach (RampColorPreset.IndexSet indexPair in RampColorPresetFile.IndexPairs)
		{
			if (indexPair.PrimaryIndex == primaryIndex && indexPair.SecondaryIndex == secondaryIndex)
			{
				indexSet = indexPair;
				break;
			}
		}
		if (indexSet == null)
		{
			PFLog.TechArt.Warning($"ApplyRampsByIndicesFromOwnPresets: Preset with indices {primaryIndex}, {secondaryIndex} not found");
			return;
		}
		m_PetRamp01Index = primaryIndex;
		m_PetRamp02Index = secondaryIndex;
		ApplyRampsWithShader();
		PFLog.TechArt.Log($"ApplyRampsByIndicesFromOwnPresets: Applied preset '{indexSet.Name}' with indices {primaryIndex}, {secondaryIndex}");
	}

	public void RestoreOriginalMaterials()
	{
		Renderer componentInChildren = GetComponentInChildren<Renderer>();
		if (componentInChildren == null || PetOriginalMaterial == null)
		{
			PFLog.TechArt.Warning("RestoreOriginalMaterials: No renderer or original material found");
			return;
		}
		Material[] array = new Material[componentInChildren.sharedMaterials.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PetOriginalMaterial;
		}
		componentInChildren.sharedMaterials = array;
		PFLog.TechArt.Log("RestoreOriginalMaterials: Restored original materials for pet: " + base.name);
	}

	public void ApplyRampsWithShader()
	{
		Renderer componentInChildren = GetComponentInChildren<Renderer>();
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("ApplyRampsWithShader: No renderer in " + base.name);
			return;
		}
		if (EquipmentColorizerShader == null)
		{
			PFLog.TechArt.Error("ApplyRampsWithShader: EquipmentColorizerShader is null");
			return;
		}
		if (PetOriginalMaterial == null && componentInChildren.sharedMaterials.Length != 0)
		{
			PetOriginalMaterial = componentInChildren.sharedMaterials[0];
			PFLog.TechArt.Log("ApplyRampsWithShader: Saved original material: " + PetOriginalMaterial.name);
		}
		Texture2D rampTextureFromPresetByIndex = GetRampTextureFromPresetByIndex(m_PetRamp01Index);
		Texture2D rampTextureFromPresetByIndex2 = GetRampTextureFromPresetByIndex(m_PetRamp02Index);
		List<Material> list = new List<Material>();
		Material material = PetOriginalMaterial ?? componentInChildren.sharedMaterials[0];
		Material[] sharedMaterials = componentInChildren.sharedMaterials;
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			_ = sharedMaterials[i];
			Material material2 = new Material(EquipmentColorizerShader);
			material2.SetTexture(ShaderProps._MasksMap, material.GetTexture(ShaderProps._MasksMap));
			material2.SetTexture(ShaderProps._BaseMap, material.GetTexture(ShaderProps._BaseMap));
			material2.SetTexture(ShaderProps._BumpMap, material.GetTexture(ShaderProps._BumpMap));
			if (rampTextureFromPresetByIndex != null)
			{
				material2.SetTexture(ShaderProps._Ramp1, rampTextureFromPresetByIndex);
				PFLog.TechArt.Log($"ApplyRampsWithShader: Applied ramp01: {rampTextureFromPresetByIndex.name} (index {m_PetRamp01Index})");
			}
			if (rampTextureFromPresetByIndex2 != null)
			{
				material2.SetTexture(ShaderProps._Ramp2, rampTextureFromPresetByIndex2);
				PFLog.TechArt.Log($"ApplyRampsWithShader: Applied ramp02: {rampTextureFromPresetByIndex2.name} (index {m_PetRamp02Index})");
			}
			if (petColorMask != null)
			{
				material2.SetTexture(ShaderProps._ColorMask, petColorMask);
				PFLog.TechArt.Log("ApplyColorMask: Applied ColorMask: " + petColorMask.name);
			}
			material2.name = base.name + $"_material_ramp_{m_PetRamp01Index}_{m_PetRamp02Index}";
			list.Add(material2);
		}
		if (list.Count > 0)
		{
			componentInChildren.sharedMaterials = list.ToArray();
			PFLog.TechArt.Log($"ApplyRampsWithShader: Applied {list.Count} new materials with ramps to pet: {base.name}");
		}
	}

	private Texture2D GetRampTextureFromPresetByIndex(int rampIndex)
	{
		if (CharacterColorsProfileFile?.Ramps == null || rampIndex < 0)
		{
			return null;
		}
		if (rampIndex >= CharacterColorsProfileFile.Ramps.Count)
		{
			PFLog.TechArt.Warning($"GetRampTextureFromPresetByIndex: Ramp index {rampIndex} out of range. Available ramps: {CharacterColorsProfileFile.Ramps.Count}");
			return null;
		}
		return CharacterColorsProfileFile.Ramps[rampIndex];
	}

	public void SaveRampIndicesToDoll(BaseUnitEntity petUnit)
	{
		if (petUnit?.ViewSettings?.Doll == null)
		{
			PFLog.TechArt.Warning("SaveRampIndicesToDoll: Pet unit or doll data is null");
			return;
		}
		petUnit.ViewSettings.Doll.PetRamp01Index = m_PetRamp01Index;
		petUnit.ViewSettings.Doll.PetRamp02Index = m_PetRamp02Index;
		PFLog.TechArt.Log($"SaveRampIndicesToDoll: Saved ramp indices {m_PetRamp01Index}, {m_PetRamp02Index} to pet doll data");
	}

	public void LoadRampIndicesFromDoll(BaseUnitEntity petUnit)
	{
		if (petUnit?.ViewSettings?.Doll == null)
		{
			PFLog.TechArt.Warning("LoadRampIndicesFromDoll: Pet unit or doll data is null");
			return;
		}
		int petRamp01Index = petUnit.ViewSettings.Doll.PetRamp01Index;
		int petRamp02Index = petUnit.ViewSettings.Doll.PetRamp02Index;
		if (petRamp01Index >= 0 || petRamp02Index >= 0)
		{
			ApplyRampsByIndicesFromOwnPresets(petRamp01Index, petRamp02Index);
			PFLog.TechArt.Log($"LoadRampIndicesFromDoll: Loaded and applied ramp indices {petRamp01Index}, {petRamp02Index} from pet doll data");
		}
	}

	public void HandlePetInitialized(BlueprintPet pet)
	{
		PFLog.TechArt.Log("PetCharacter.HandlePetInitialized: Pet initialized, applying default ramps to: " + base.name);
		StartCoroutine(ApplyDefaultRampsCoroutine());
	}
}
