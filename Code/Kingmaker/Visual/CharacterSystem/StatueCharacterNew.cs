using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class StatueCharacterNew : Character
{
	[Header("Statue Settings")]
	[SerializeField]
	private Material m_StoneMaterial;

	[SerializeField]
	private RuntimeAnimatorController m_StatueAnimatorController;

	[Header("Fallback Statue")]
	[SerializeField]
	[Tooltip("GameObject that will be shown if MainCharacterEntity is not available or fails to create")]
	private GameObject m_DefaultStatueGO;

	[Header("Character Setup")]
	[SerializeField]
	private Skeleton m_CharacterSkeleton;

	[SerializeField]
	private Animator m_CharacterAnimatorPrefab;

	[Header("Debug Info")]
	[SerializeField]
	private List<EquipmentEntity> m_CurrentEquipmentEntities = new List<EquipmentEntity>();

	[SerializeField]
	private List<EquipmentEntity> m_OccupationEquipmentEntities = new List<EquipmentEntity>();

	[SerializeField]
	private List<EquipmentEntity> m_CombinedEquipmentEntities = new List<EquipmentEntity>();

	private bool m_StatueCreated;

	private bool m_IsCreatingStatue;

	private void OnEnable()
	{
		if (!m_IsCreatingStatue)
		{
			StartCoroutine(CreateStatueWhenReady());
		}
	}

	private IEnumerator CreateStatueWhenReady()
	{
		m_IsCreatingStatue = true;
		PFLog.TechArt.Log("[StatueCharacter] Waiting for MainCharacter to be ready...");
		BaseUnitEntity mainCharacter = null;
		int attempts = 0;
		while (attempts < 100)
		{
			mainCharacter = Game.Instance?.Player?.MainCharacterEntity;
			if (IsMainCharacterReady(mainCharacter))
			{
				PFLog.TechArt.Log($"[StatueCharacter] MainCharacter ready after {attempts + 1} attempts: {mainCharacter.CharacterName}");
				break;
			}
			attempts++;
			yield return new WaitForSeconds(0.1f);
		}
		if (mainCharacter == null || !IsMainCharacterReady(mainCharacter))
		{
			PFLog.TechArt.Warning("[StatueCharacter] Failed to get ready MainCharacter after maximum attempts - showing default statue");
			ShowDefaultStatue();
		}
		else
		{
			CreateStatue(mainCharacter);
		}
	}

	private bool IsMainCharacterReady(BaseUnitEntity character)
	{
		if (character?.View?.CharacterAvatar?.EquipmentEntities == null)
		{
			return false;
		}
		return character.View.CharacterAvatar.EquipmentEntities.Count > 0;
	}

	private void CreateStatue(BaseUnitEntity mainCharacter)
	{
		PFLog.TechArt.Log("[StatueCharacter] === CREATING STATUE ===");
		HideDefaultStatue();
		SetupCharacterForStatue(mainCharacter);
		List<EquipmentEntity> list = (m_CurrentEquipmentEntities = GetCurrentEquipmentEntities(mainCharacter));
		PFLog.TechArt.Log($"[StatueCharacter] Current Equipment Entities: {list.Count}");
		List<EquipmentEntity> list2 = (m_OccupationEquipmentEntities = GetOccupationEquipmentEntities());
		PFLog.TechArt.Log($"[StatueCharacter] Occupation Equipment Entities: {list2.Count}");
		List<EquipmentEntity> list3 = new List<EquipmentEntity>();
		list3.AddRange(list);
		list3.AddRange(list2);
		m_CombinedEquipmentEntities = list3;
		PFLog.TechArt.Log($"[StatueCharacter] Combined Equipment Entities: {list3.Count} (MainCharacter: {list.Count} + Occupation: {list2.Count})");
		for (int i = 0; i < list3.Count; i++)
		{
			EquipmentEntity equipmentEntity = list3[i];
			string arg = ((i < list.Count) ? "MainCharacter" : "Occupation");
			PFLog.TechArt.Log(string.Format("[StatueCharacter] Combined EE [{0}] ({1}): {2}", i, arg, equipmentEntity?.name ?? "null"));
		}
		if (list3.Count > 0)
		{
			AddEquipmentEntities(list3);
			PFLog.TechArt.Log($"[StatueCharacter] Added {list3.Count} combined Equipment Entities to Character");
		}
		else
		{
			PFLog.TechArt.Warning("[StatueCharacter] No Equipment Entities to add!");
		}
		PFLog.TechArt.Log($"[StatueCharacter] Total Equipment Entities after adding: {base.EquipmentEntities?.Count ?? 0}");
		OnStart();
		StartCoroutine(ForceCharacterUpdate());
		PFLog.TechArt.Log("[StatueCharacter] Character assembly started");
	}

	private void SetupCharacterForStatue(BaseUnitEntity mainCharacter)
	{
		Character characterAvatar = mainCharacter.View.CharacterAvatar;
		if (m_CharacterSkeleton != null)
		{
			base.Skeleton = m_CharacterSkeleton;
			PFLog.TechArt.Log("[StatueCharacter] Using assigned Skeleton");
		}
		else if (characterAvatar.Skeleton != null)
		{
			base.Skeleton = characterAvatar.Skeleton;
			PFLog.TechArt.Log("[StatueCharacter] Copied Skeleton from MainCharacter");
		}
		if (m_CharacterAnimatorPrefab != null)
		{
			AnimatorPrefab = m_CharacterAnimatorPrefab;
			PFLog.TechArt.Log("[StatueCharacter] Using assigned AnimatorPrefab");
		}
		else if (characterAvatar.AnimatorPrefab != null)
		{
			AnimatorPrefab = characterAvatar.AnimatorPrefab;
			PFLog.TechArt.Log("[StatueCharacter] Copied AnimatorPrefab from MainCharacter");
		}
		makeTextures = false;
		IsInDollRoom = true;
		PreventUpdate = false;
		PFLog.TechArt.Log("[StatueCharacter] Character setup: Skeleton=" + base.Skeleton?.name + ", AnimatorPrefab=" + AnimatorPrefab?.name);
	}

	private List<EquipmentEntity> GetCurrentEquipmentEntities(BaseUnitEntity character)
	{
		PFLog.TechArt.Log("[StatueCharacter] GetCurrentEquipmentEntities: Starting...");
		if (character?.View?.CharacterAvatar == null)
		{
			PFLog.TechArt.Error("[StatueCharacter] GetCurrentEquipmentEntities: Character, View or CharacterAvatar is null!");
			return new List<EquipmentEntity>();
		}
		List<EquipmentEntity> list = new List<EquipmentEntity>();
		List<EquipmentEntityLink> savedEquipmentEntities = character.View.CharacterAvatar.SavedEquipmentEntities;
		if (savedEquipmentEntities != null && savedEquipmentEntities.Count > 0)
		{
			PFLog.TechArt.Log($"[StatueCharacter] GetCurrentEquipmentEntities: Priority 1 - Found {savedEquipmentEntities.Count} saved Equipment Entity Links");
			list = LoadEquipmentEntitiesFromLinks(savedEquipmentEntities, "saved");
			if (list.Count > 0)
			{
				PFLog.TechArt.Log($"[StatueCharacter] GetCurrentEquipmentEntities: Priority 1 SUCCESS - {list.Count} Equipment Entities from SavedEquipmentEntities");
				return list;
			}
		}
		PFLog.TechArt.Log("[StatueCharacter] GetCurrentEquipmentEntities: Priority 2 - Using DollState.SetupFromUnit...");
		list = GetEquipmentEntitiesFromDollState(character);
		if (list.Count > 0)
		{
			PFLog.TechArt.Log($"[StatueCharacter] GetCurrentEquipmentEntities: Priority 2 SUCCESS - {list.Count} Equipment Entities from DollState");
			return list;
		}
		PFLog.TechArt.Log("[StatueCharacter] GetCurrentEquipmentEntities: Priority 3 - Fallback to progression method...");
		list = GetEquipmentEntitiesFromProgression(character);
		PFLog.TechArt.Log($"[StatueCharacter] GetCurrentEquipmentEntities: Final result: {list.Count} Equipment Entities");
		return list;
	}

	private List<EquipmentEntity> LoadEquipmentEntitiesFromLinks(IEnumerable<EquipmentEntityLink> links, string source)
	{
		List<EquipmentEntity> list = new List<EquipmentEntity>();
		foreach (EquipmentEntityLink link in links)
		{
			try
			{
				EquipmentEntity equipmentEntity = link.Load();
				if (equipmentEntity != null)
				{
					list.Add(equipmentEntity);
					PFLog.TechArt.Log("[StatueCharacter] LoadEquipmentEntitiesFromLinks: Loaded " + source + " EE: " + equipmentEntity.name);
				}
				else
				{
					PFLog.TechArt.Warning("[StatueCharacter] LoadEquipmentEntitiesFromLinks: Failed to load " + source + " EE from link " + link.AssetId);
				}
			}
			catch (Exception ex)
			{
				PFLog.TechArt.Error("[StatueCharacter] LoadEquipmentEntitiesFromLinks: Exception loading " + source + " EE " + link.AssetId + ": " + ex.Message);
			}
		}
		return list;
	}

	private List<EquipmentEntity> GetEquipmentEntitiesFromDollState(BaseUnitEntity character)
	{
		try
		{
			DollState dollState = new DollState();
			dollState.SetupFromUnit(character);
			List<EquipmentEntityLink> list = dollState.CollectEntities();
			PFLog.TechArt.Log($"[StatueCharacter] GetEquipmentEntitiesFromDollState: DollState.CollectEntities() returned {list.Count} links");
			return LoadEquipmentEntitiesFromLinks(list, "DollState");
		}
		catch (Exception ex)
		{
			PFLog.TechArt.Error("[StatueCharacter] GetEquipmentEntitiesFromDollState: Exception: " + ex.Message);
			return new List<EquipmentEntity>();
		}
	}

	private List<EquipmentEntity> GetEquipmentEntitiesFromProgression(BaseUnitEntity character)
	{
		Gender gender = character.Description?.Gender ?? Gender.Male;
		Race valueOrDefault = (character.Progression?.Race?.RaceId).GetValueOrDefault();
		PFLog.TechArt.Log($"[StatueCharacter] GetEquipmentEntitiesFromProgression: Using Gender={gender}, Race={valueOrDefault}");
		try
		{
			IEnumerable<KingmakerEquipmentEntity> unitEquipmentEntities = CharGenUtility.GetUnitEquipmentEntities(character);
			if (unitEquipmentEntities != null)
			{
				PFLog.TechArt.Log($"[StatueCharacter] GetEquipmentEntitiesFromProgression: Found {unitEquipmentEntities.Count()} equipment entities from progression");
				IEnumerable<EquipmentEntityLink> clothes = CharGenUtility.GetClothes(unitEquipmentEntities, gender, valueOrDefault);
				if (clothes != null)
				{
					return LoadEquipmentEntitiesFromLinks(clothes, "progression");
				}
				PFLog.TechArt.Warning("[StatueCharacter] GetEquipmentEntitiesFromProgression: clothesLinks is null from progression");
			}
			else
			{
				PFLog.TechArt.Warning("[StatueCharacter] GetEquipmentEntitiesFromProgression: equipmentEntities is null from progression");
			}
		}
		catch (Exception ex)
		{
			PFLog.TechArt.Error("[StatueCharacter] GetEquipmentEntitiesFromProgression: Exception: " + ex.Message);
		}
		return new List<EquipmentEntity>();
	}

	private List<EquipmentEntity> GetOccupationEquipmentEntities()
	{
		PFLog.TechArt.Log("[StatueCharacter] GetOccupationEquipmentEntities: Starting...");
		BaseUnitEntity baseUnitEntity = Game.Instance?.Player?.MainCharacterEntity;
		if (baseUnitEntity == null)
		{
			PFLog.TechArt.Error("[StatueCharacter] GetOccupationEquipmentEntities: Main character is null");
			return new List<EquipmentEntity>();
		}
		BlueprintFeature mainCharacterOccupation = GetMainCharacterOccupation();
		if (mainCharacterOccupation == null)
		{
			PFLog.TechArt.Log("[StatueCharacter] GetOccupationEquipmentEntities: No occupation found - this is normal for DefaultPlayerCharacter");
			return new List<EquipmentEntity>();
		}
		PFLog.TechArt.Log("[StatueCharacter] GetOccupationEquipmentEntities: Found occupation: " + mainCharacterOccupation.Name);
		AddKingmakerEquipmentEntity[] array = mainCharacterOccupation.GetComponents<AddKingmakerEquipmentEntity>().ToArray();
		if (array.Length == 0)
		{
			PFLog.TechArt.Log("[StatueCharacter] GetOccupationEquipmentEntities: No AddKingmakerEquipmentEntity components found in occupation");
			return new List<EquipmentEntity>();
		}
		Gender gender = baseUnitEntity.Description?.Gender ?? Gender.Male;
		Race valueOrDefault = (baseUnitEntity.Progression?.Race?.RaceId).GetValueOrDefault();
		PFLog.TechArt.Log($"[StatueCharacter] GetOccupationEquipmentEntities: Using Gender={gender}, Race={valueOrDefault}");
		List<EquipmentEntity> list = new List<EquipmentEntity>();
		AddKingmakerEquipmentEntity[] array2 = array;
		foreach (AddKingmakerEquipmentEntity addKingmakerEquipmentEntity in array2)
		{
			if (addKingmakerEquipmentEntity.EquipmentEntity == null)
			{
				continue;
			}
			EquipmentEntityLink[] links = addKingmakerEquipmentEntity.EquipmentEntity.GetLinks(gender, valueOrDefault);
			EquipmentEntityLink[] array3 = links;
			foreach (EquipmentEntityLink equipmentEntityLink in array3)
			{
				try
				{
					EquipmentEntity equipmentEntity = equipmentEntityLink.Load();
					if (equipmentEntity != null)
					{
						list.Add(equipmentEntity);
					}
				}
				catch (Exception ex)
				{
					PFLog.TechArt.Error("[StatueCharacter] GetOccupationEquipmentEntities: Failed to load EE " + equipmentEntityLink.AssetId + ": " + ex.Message);
				}
			}
			PFLog.TechArt.Log($"[StatueCharacter] GetOccupationEquipmentEntities: Added {links.Count()} links from {addKingmakerEquipmentEntity.EquipmentEntity.name}");
		}
		PFLog.TechArt.Log($"[StatueCharacter] GetOccupationEquipmentEntities: Total occupation EE: {list.Count}");
		return list;
	}

	private BlueprintFeature GetMainCharacterOccupation()
	{
		BaseUnitEntity baseUnitEntity = Game.Instance?.Player?.MainCharacterEntity;
		if (baseUnitEntity?.Progression == null)
		{
			return null;
		}
		BlueprintOriginPath unitOriginPath = CharGenUtility.GetUnitOriginPath(baseUnitEntity);
		if (unitOriginPath == null)
		{
			return null;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = CharGenUtility.GetFeatureSelectionsByGroup(unitOriginPath, FeatureGroup.ChargenOccupation).FirstOrDefault();
		if (blueprintSelectionFeature == null)
		{
			return null;
		}
		return baseUnitEntity.Progression.GetSelectedFeature(unitOriginPath, 0, blueprintSelectionFeature)?.Feature;
	}

	private IEnumerator ForceCharacterUpdate()
	{
		PFLog.TechArt.Log("[StatueCharacter] Starting forced Character update...");
		IsDirty = true;
		int attempts = 0;
		while (attempts < 100)
		{
			DoUpdate();
			PFLog.TechArt.Log($"[StatueCharacter] Force update attempt {attempts + 1}: IsDirty={IsDirty}, OverlaysMerged={base.OverlaysMerged}, EquipmentEntities={base.EquipmentEntities?.Count ?? 0}");
			if (!IsDirty && base.OverlaysMerged && base.EquipmentEntities != null && base.EquipmentEntities.Count > 0)
			{
				PFLog.TechArt.Log("[StatueCharacter] Character assembly completed, converting to statue...");
				ConvertToStatue();
				yield break;
			}
			attempts++;
			yield return new WaitForSeconds(0.1f);
		}
		PFLog.TechArt.Error($"[StatueCharacter] Failed to complete Character assembly after {100} attempts - showing default statue");
		ShowDefaultStatue();
	}

	private void ConvertToStatue()
	{
		PFLog.TechArt.Log("[StatueCharacter] Converting to statue...");
		ApplyStoneMaterial();
		SetupStatueAnimator();
		RemoveUnnecessaryComponents();
		m_StatueCreated = true;
		PFLog.TechArt.Log("[StatueCharacter] Statue conversion complete!");
	}

	private void ApplyStoneMaterial()
	{
		if (m_StoneMaterial == null)
		{
			PFLog.TechArt.Warning("[StatueCharacter] Stone material is not assigned");
			return;
		}
		SkinnedMeshRenderer[] componentsInChildren = GetComponentsInChildren<SkinnedMeshRenderer>();
		PFLog.TechArt.Log($"[StatueCharacter] Applying stone material to {componentsInChildren.Length} SkinnedMeshRenderers");
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			Material[] array2 = new Material[skinnedMeshRenderer.materials.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = m_StoneMaterial;
			}
			skinnedMeshRenderer.materials = array2;
		}
		MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>();
		PFLog.TechArt.Log($"[StatueCharacter] Applying stone material to {componentsInChildren2.Length} MeshRenderers");
		MeshRenderer[] array3 = componentsInChildren2;
		foreach (MeshRenderer meshRenderer in array3)
		{
			Material[] array4 = new Material[meshRenderer.materials.Length];
			for (int k = 0; k < array4.Length; k++)
			{
				array4[k] = m_StoneMaterial;
			}
			meshRenderer.materials = array4;
		}
	}

	private void SetupStatueAnimator()
	{
		Animator componentInChildren = GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			if (m_StatueAnimatorController != null)
			{
				componentInChildren.runtimeAnimatorController = m_StatueAnimatorController;
				PFLog.TechArt.Log("[StatueCharacter] Statue animator controller applied");
			}
			else
			{
				PFLog.TechArt.Warning("[StatueCharacter] StatueAnimatorController is not assigned");
			}
		}
		else
		{
			PFLog.TechArt.Warning("[StatueCharacter] Animator not found");
		}
	}

	private void RemoveUnnecessaryComponents()
	{
		UnitAnimationManager componentInChildren = GetComponentInChildren<UnitAnimationManager>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.DestroyImmediate(componentInChildren);
			PFLog.TechArt.Log("[StatueCharacter] Removed UnitAnimationManager");
		}
		PFLog.TechArt.Log("[StatueCharacter] Unnecessary components removed");
	}

	public void ShowDefaultStatue()
	{
		PFLog.TechArt.Log("[StatueCharacter] ShowDefaultStatue: Attempting to show default statue...");
		if (m_DefaultStatueGO == null)
		{
			PFLog.TechArt.Error("[StatueCharacter] ShowDefaultStatue: DefaultStatueGO is not assigned! Cannot show fallback statue.");
			return;
		}
		m_DefaultStatueGO.SetActive(value: true);
		PFLog.TechArt.Log("[StatueCharacter] ShowDefaultStatue: Default statue '" + m_DefaultStatueGO.name + "' is now visible");
	}

	public void HideDefaultStatue()
	{
		if (m_DefaultStatueGO != null && m_DefaultStatueGO.activeInHierarchy)
		{
			m_DefaultStatueGO.SetActive(value: false);
			PFLog.TechArt.Log("[StatueCharacter] HideDefaultStatue: Default statue '" + m_DefaultStatueGO.name + "' is now hidden");
		}
	}
}
