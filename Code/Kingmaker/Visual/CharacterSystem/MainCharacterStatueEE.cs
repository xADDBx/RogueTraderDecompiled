using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharactersRigidbody;
using RootMotion.FinalIK;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class MainCharacterStatueEE : MonoBehaviour, IAreaHandler, ISubscriber
{
	[Header("Statue Settings")]
	[SerializeField]
	private bool m_CreateOnStart = true;

	[SerializeField]
	private Material m_StoneMaterial;

	[SerializeField]
	private RuntimeAnimatorController m_StatueAnimatorController;

	[Header("Fallback Statue")]
	[SerializeField]
	[Tooltip("GameObject that will be shown if MainCharacterEntity is not available (e.g., when PlayerCharacter is empty)")]
	private GameObject m_DefaultStatueGO;

	[Header("Debug Settings")]
	[SerializeField]
	private bool m_ShowDetailedConsole = true;

	[Header("Debug Info")]
	[SerializeField]
	private bool m_StatueCreationAttempted;

	private BaseUnitEntity m_StatueUnit;

	private bool m_IsInitialized;

	public bool IsStatueCreated
	{
		get
		{
			if (m_StatueUnit == null || !IsStatueVisuallyReady())
			{
				if (m_DefaultStatueGO != null)
				{
					return m_DefaultStatueGO.activeInHierarchy;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsDynamicStatueCreated
	{
		get
		{
			if (m_StatueUnit != null)
			{
				return IsStatueVisuallyReady();
			}
			return false;
		}
	}

	public bool IsDefaultStatueShown
	{
		get
		{
			if (m_DefaultStatueGO != null)
			{
				return m_DefaultStatueGO.activeInHierarchy;
			}
			return false;
		}
	}

	public bool StatueCreationAttempted => m_StatueCreationAttempted;

	private void LogDetailed(string message)
	{
		if (m_ShowDetailedConsole)
		{
			PFLog.TechArt.Log(message);
		}
	}

	private void LogDetailedWarning(string message)
	{
		if (m_ShowDetailedConsole)
		{
			PFLog.TechArt.Warning(message);
		}
	}

	private void LogDetailedError(string message)
	{
		if (m_ShowDetailedConsole)
		{
			PFLog.TechArt.Error(message);
		}
	}

	private void LogCriticalError(string message)
	{
		PFLog.TechArt.Error(message);
	}

	private void LogCriticalException(Exception e)
	{
		PFLog.TechArt.Exception(e);
	}

	private bool IsStatueVisuallyReady()
	{
		if (m_StatueUnit?.View?.CharacterAvatar == null)
		{
			return false;
		}
		Character characterAvatar = m_StatueUnit.View.CharacterAvatar;
		SkinnedMeshRenderer[] componentsInChildren = characterAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		int num = 0;
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (skinnedMeshRenderer.sharedMesh != null && skinnedMeshRenderer.enabled && skinnedMeshRenderer.gameObject.activeInHierarchy)
			{
				num++;
			}
		}
		bool flag = num > 0;
		bool flag2 = !characterAvatar.IsDirty;
		LogDetailed($"IsStatueVisuallyReady: ValidRenderers={num}, IsDirty={characterAvatar.IsDirty}, OverlaysMerged={characterAvatar.OverlaysMerged}, Result={flag && flag2}");
		return flag && flag2;
	}

	private void OnEnable()
	{
		LogDetailed("=== MainCharacterStatueEE.OnEnable ===");
		LogDetailed($"IsInitialized={m_IsInitialized}");
		LogDetailed($"CreateOnStart={m_CreateOnStart}");
		LogDetailed($"StatueCreationAttempted={m_StatueCreationAttempted}");
		LogDetailed($"IsDynamicStatueCreated={IsDynamicStatueCreated}");
		LogDetailed($"IsDefaultStatueShown={IsDefaultStatueShown}");
		if (m_IsInitialized)
		{
			LogDetailed("OnEnable: Already initialized, performing quick check...");
			PerformQuickStatueCheck();
			return;
		}
		LogDetailed("OnEnable: First initialization...");
		m_IsInitialized = true;
		LogDetailed($"DefaultStatueGO assigned: {m_DefaultStatueGO != null}");
		if (m_DefaultStatueGO != null)
		{
			LogDetailed("DefaultStatueGO name: " + m_DefaultStatueGO.name);
			LogDetailed($"DefaultStatueGO active: {m_DefaultStatueGO.activeInHierarchy}");
		}
		LogDetailed($"Game.Instance: {Game.Instance != null}");
		LogDetailed($"Game.Instance.Player: {Game.Instance?.Player != null}");
		LogDetailed($"MainCharacterEntity: {Game.Instance?.Player?.MainCharacterEntity != null}");
		if (m_StatueCreationAttempted && !IsDynamicStatueCreated)
		{
			LogDetailed("OnEnable: Statue creation was attempted but dynamic statue doesn't exist, resetting flag...");
			m_StatueCreationAttempted = false;
		}
		if (m_CreateOnStart && !m_StatueCreationAttempted)
		{
			LogDetailed("OnEnable: Starting statue creation process...");
			EventBus.Subscribe(this);
			StartCoroutine(TryCreateStatueWhenReady());
		}
		else
		{
			LogDetailed($"OnEnable: Statue creation skipped - CreateOnStart={m_CreateOnStart}, StatueCreationAttempted={m_StatueCreationAttempted}");
		}
	}

	private void PerformQuickStatueCheck()
	{
		if (!IsDynamicStatueCreated && m_CreateOnStart && !m_StatueCreationAttempted)
		{
			BaseUnitEntity mainCharacter = GetMainCharacter();
			if (IsMainCharacterFullyReady(mainCharacter))
			{
				LogDetailed("PerformQuickStatueCheck: MainCharacter ready, creating statue...");
				CreateStatue();
			}
			else if (mainCharacter == null)
			{
				LogDetailed("PerformQuickStatueCheck: MainCharacter not available, showing default statue...");
				ShowDefaultStatue();
			}
		}
	}

	private void OnDisable()
	{
		LogDetailed("OnDisable: Component disabled");
		EventBus.Unsubscribe(this);
	}

	private IEnumerator TryCreateStatueWhenReady()
	{
		LogDetailed("TryCreateStatueWhenReady: Starting periodic check for main character....");
		int attempts = 0;
		while (!m_StatueCreationAttempted && !IsDynamicStatueCreated && attempts < 300)
		{
			attempts++;
			BaseUnitEntity mainCharacter = GetMainCharacter();
			LogDetailed(string.Format("TryCreateStatueWhenReady: Attempt {0}, MainCharacter: {1}, DynamicStatue: {2}, DefaultStatue: {3}", attempts, (mainCharacter != null) ? mainCharacter.CharacterName : "NULL", IsDynamicStatueCreated, IsDefaultStatueShown));
			if (IsMainCharacterFullyReady(mainCharacter))
			{
				LogDetailed("TryCreateStatueWhenReady: Main character fully ready: " + mainCharacter.CharacterName);
				CreateStatue();
				LogDetailed("TryCreateStatueWhenReady: Statue creation attempted.");
				yield break;
			}
			yield return new WaitForSeconds(0.1f);
		}
		if (attempts >= 300)
		{
			LogCriticalError("TryCreateStatueWhenReady: TIMEOUT - Failed to create statue after 30 seconds");
		}
	}

	private bool IsMainCharacterFullyReady(BaseUnitEntity mainCharacter)
	{
		if (mainCharacter == null)
		{
			return false;
		}
		LogDetailed("IsMainCharacterFullyReady: Checking " + mainCharacter.CharacterName + "...");
		if (mainCharacter.Progression == null)
		{
			LogDetailed("IsMainCharacterFullyReady: No Progression found");
			return false;
		}
		if (mainCharacter.Description == null)
		{
			LogDetailed("IsMainCharacterFullyReady: No Description found");
			return false;
		}
		try
		{
			List<EquipmentEntityLink> currentEquipmentEntities = GetCurrentEquipmentEntities();
			if (currentEquipmentEntities.Count == 0)
			{
				LogDetailed("IsMainCharacterFullyReady: No current Equipment Entities found");
				return false;
			}
			List<EquipmentEntityLink> occupationEquipmentEntities = GetOccupationEquipmentEntities();
			LogDetailed($"IsMainCharacterFullyReady: Found {currentEquipmentEntities.Count} current EE and {occupationEquipmentEntities.Count} occupation EE");
			List<EquipmentEntityLink> equipmentLinks = CombineEquipmentEntities(currentEquipmentEntities, occupationEquipmentEntities);
			List<EquipmentEntityLink> list = FilterValidEquipmentEntities(equipmentLinks);
			if (list.Count == 0)
			{
				LogDetailed("IsMainCharacterFullyReady: No valid Equipment Entities found");
				return false;
			}
			LogDetailed($"IsMainCharacterFullyReady: SUCCESS - Found {list.Count} valid Equipment Entities");
			return true;
		}
		catch (Exception ex)
		{
			LogDetailedError("IsMainCharacterFullyReady: Exception checking Equipment Entities: " + ex.Message);
			return false;
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		LogDetailed($"OnAreaDidLoad: Area loaded - CreateOnStart={m_CreateOnStart}, StatueCreationAttempted={m_StatueCreationAttempted}, DynamicStatue={IsDynamicStatueCreated}, DefaultStatue={IsDefaultStatueShown}");
		if (m_StatueCreationAttempted && !IsDynamicStatueCreated)
		{
			LogDetailed("OnAreaDidLoad: Statue creation was attempted but dynamic statue doesn't exist, resetting flag...");
			m_StatueCreationAttempted = false;
		}
		if (m_CreateOnStart && !m_StatueCreationAttempted && !IsDynamicStatueCreated)
		{
			LogDetailed("OnAreaDidLoad: Area loaded, trying to create dynamic statue...");
			StartCoroutine(TryCreateStatueAfterAreaLoad());
		}
		else
		{
			LogDetailed($"OnAreaDidLoad: Skipping statue creation - CreateOnStart={m_CreateOnStart}, StatueCreationAttempted={m_StatueCreationAttempted}, DynamicStatue={IsDynamicStatueCreated}");
		}
	}

	private IEnumerator TryCreateStatueAfterAreaLoad()
	{
		LogDetailed("TryCreateStatueAfterAreaLoad: Starting...");
		for (int i = 0; i < 5; i++)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (IsMainCharacterFullyReady(mainCharacter))
		{
			LogDetailed("TryCreateStatueAfterAreaLoad: Main character ready immediately: " + mainCharacter.CharacterName);
			CreateStatue();
			LogDetailed("TryCreateStatueAfterAreaLoad: Statue creation attempted.");
		}
		else
		{
			LogDetailed("TryCreateStatueAfterAreaLoad: Main character not ready, starting periodic check...");
			StartCoroutine(TryCreateStatueWhenReady());
		}
	}

	public void CreateStatue()
	{
		if (m_StatueCreationAttempted)
		{
			LogDetailedWarning("CreateStatue: Statue creation already attempted. Use ResetCreationFlag() to try again.");
			return;
		}
		if (IsDynamicStatueCreated)
		{
			LogDetailedWarning("CreateStatue: Dynamic statue already exists. Destroying old statue first.");
			DestroyStatue();
		}
		LogDetailed("CreateStatue: Starting statue creation process...");
		m_StatueCreationAttempted = true;
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (mainCharacter == null)
		{
			LogDetailedWarning("Main character not found - trying to show default statue");
			ShowDefaultStatue();
			return;
		}
		LogDetailed("Creating statue from main character: " + mainCharacter.CharacterName);
		LogOccupationInfo();
		try
		{
			LogDetailed("=== STARTING STATUE CREATION ===");
			HideDefaultStatue();
			LogDetailed("Step 1: Getting current Equipment Entities from main character...");
			List<EquipmentEntityLink> currentEquipmentEntities = GetCurrentEquipmentEntities();
			LogDetailed($"Step 1 RESULT: Found {currentEquipmentEntities.Count} current Equipment Entity Links");
			LogDetailed("Step 2: Getting occupation Equipment Entities...");
			List<EquipmentEntityLink> occupationEquipmentEntities = GetOccupationEquipmentEntities();
			LogDetailed($"Step 2 RESULT: Found {occupationEquipmentEntities.Count} occupation Equipment Entity Links");
			LogDetailed("Step 3: Combining Equipment Entity lists...");
			List<EquipmentEntityLink> list = CombineEquipmentEntities(currentEquipmentEntities, occupationEquipmentEntities);
			LogDetailed($"Step 3 RESULT: Combined {list.Count} Equipment Entity Links");
			LogDetailed("Step 4: Filtering valid Equipment Entity Links...");
			List<EquipmentEntityLink> list2 = FilterValidEquipmentEntities(list);
			LogDetailed($"Step 4 RESULT: {list2.Count} valid Equipment Entity Links");
			LogDetailed("Step 5: Creating statue unit...");
			m_StatueUnit = CreateStatueUnit(mainCharacter, list2);
			if (m_StatueUnit == null)
			{
				LogCriticalError("Step 5 FAILED: Could not create statue unit");
				return;
			}
			LogDetailed("Step 5 SUCCESS: Statue unit created: " + m_StatueUnit.CharacterName);
			LogDetailed("Step 6: Setting up statue...");
			SetupStatue(m_StatueUnit);
			LogDetailed("Step 6 SUCCESS: Statue setup completed");
			LogDetailed("=== STATUE CREATION COMPLETED ===");
		}
		catch (Exception ex)
		{
			LogCriticalError("Failed to create statue: " + ex.Message);
			LogCriticalException(ex);
		}
	}

	public void ShowDefaultStatue()
	{
		LogDetailed("ShowDefaultStatue: Attempting to show default statue...");
		if (m_DefaultStatueGO == null)
		{
			LogCriticalError("ShowDefaultStatue: DefaultStatueGO is not assigned! Cannot show fallback statue.");
			return;
		}
		HideDynamicStatue();
		m_DefaultStatueGO.SetActive(value: true);
		LogDetailed("ShowDefaultStatue: Default statue '" + m_DefaultStatueGO.name + "' is now visible");
	}

	public void HideDefaultStatue()
	{
		if (m_DefaultStatueGO != null && m_DefaultStatueGO.activeInHierarchy)
		{
			m_DefaultStatueGO.SetActive(value: false);
			LogDetailed("HideDefaultStatue: Default statue '" + m_DefaultStatueGO.name + "' is now hidden");
		}
	}

	private void HideDynamicStatue()
	{
		if (m_StatueUnit?.View?.ViewTransform != null)
		{
			m_StatueUnit.View.ViewTransform.gameObject.SetActive(value: false);
			LogDetailed("HideDynamicStatue: Dynamic statue is now hidden");
		}
	}

	public void ResetCreationFlag()
	{
		m_StatueCreationAttempted = false;
		LogDetailed("Statue creation flag reset");
	}

	public void ResetComponent()
	{
		m_StatueCreationAttempted = false;
		m_IsInitialized = false;
		EventBus.Unsubscribe(this);
		LogDetailed("Component fully reset - will reinitialize on next OnEnable");
	}

	private BaseUnitEntity GetMainCharacter()
	{
		BaseUnitEntity baseUnitEntity = Game.Instance?.Player?.MainCharacterEntity;
		if (baseUnitEntity != null)
		{
			bool flag = IsUsingDefaultPlayerCharacter(baseUnitEntity);
			LogDetailed($"GetMainCharacter: Found character '{baseUnitEntity.CharacterName}' (IsDefault: {flag})");
		}
		else
		{
			LogDetailed("GetMainCharacter: MainCharacterEntity is NULL");
		}
		return baseUnitEntity;
	}

	private bool IsUsingDefaultPlayerCharacter(BaseUnitEntity character)
	{
		if (character?.Blueprint == null)
		{
			return false;
		}
		BlueprintUnit blueprintUnit = BlueprintRoot.Instance?.DefaultPlayerCharacter;
		if (blueprintUnit != null)
		{
			return character.Blueprint == blueprintUnit;
		}
		return false;
	}

	private BlueprintFeature GetMainCharacterOccupation()
	{
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (mainCharacter?.Progression == null)
		{
			return null;
		}
		BlueprintOriginPath unitOriginPath = CharGenUtility.GetUnitOriginPath(mainCharacter);
		if (unitOriginPath == null)
		{
			return null;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = CharGenUtility.GetFeatureSelectionsByGroup(unitOriginPath, FeatureGroup.ChargenOccupation).FirstOrDefault();
		if (blueprintSelectionFeature == null)
		{
			return null;
		}
		return mainCharacter.Progression.GetSelectedFeature(unitOriginPath, 0, blueprintSelectionFeature)?.Feature;
	}

	private List<EquipmentEntityLink> GetCurrentEquipmentEntities()
	{
		LogDetailed("  GetCurrentEquipmentEntities: Starting...");
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (mainCharacter == null)
		{
			LogDetailedError("  GetCurrentEquipmentEntities: Main character is null!");
			return new List<EquipmentEntityLink>();
		}
		LogDetailed("  GetCurrentEquipmentEntities: Main character found: " + mainCharacter.CharacterName);
		Gender gender = mainCharacter.Description?.Gender ?? Gender.Male;
		Race valueOrDefault = (mainCharacter.Progression?.Race?.RaceId).GetValueOrDefault();
		LogDetailed($"  GetCurrentEquipmentEntities: Gender={gender}, Race={valueOrDefault}");
		LogDetailed("  GetCurrentEquipmentEntities: Calling CharGenUtility.GetUnitEquipmentEntities...");
		IEnumerable<KingmakerEquipmentEntity> unitEquipmentEntities = CharGenUtility.GetUnitEquipmentEntities(mainCharacter);
		if (unitEquipmentEntities == null)
		{
			LogDetailedError("  GetCurrentEquipmentEntities: equipmentEntities is null!");
			return new List<EquipmentEntityLink>();
		}
		LogDetailed($"  GetCurrentEquipmentEntities: Found {unitEquipmentEntities.Count()} equipment entities");
		LogDetailed("  GetCurrentEquipmentEntities: Calling CharGenUtility.GetClothes...");
		IEnumerable<EquipmentEntityLink> clothes = CharGenUtility.GetClothes(unitEquipmentEntities, gender, valueOrDefault);
		if (clothes == null)
		{
			LogDetailedError("  GetCurrentEquipmentEntities: clothesLinks is null!");
			return new List<EquipmentEntityLink>();
		}
		List<EquipmentEntityLink> list = clothes.ToList();
		LogDetailed($"  GetCurrentEquipmentEntities: Returning {list.Count} clothes links");
		return list;
	}

	private List<EquipmentEntityLink> CombineEquipmentEntities(List<EquipmentEntityLink> currentEE, List<EquipmentEntityLink> occupationEE)
	{
		LogDetailed("  CombineEquipmentEntities: Starting...");
		LogDetailed($"  CombineEquipmentEntities: Current EE count: {currentEE.Count}");
		LogDetailed($"  CombineEquipmentEntities: Occupation EE count: {occupationEE.Count}");
		List<EquipmentEntityLink> list = new List<EquipmentEntityLink>();
		LogDetailed("  CombineEquipmentEntities: Adding current Equipment Entities...");
		foreach (EquipmentEntityLink item in currentEE)
		{
			if (item != null && !string.IsNullOrEmpty(item.AssetId))
			{
				list.Add(item);
				LogDetailed("  CombineEquipmentEntities: Added current EE: " + item.AssetId);
			}
		}
		LogDetailed("  CombineEquipmentEntities: Adding occupation Equipment Entities...");
		foreach (EquipmentEntityLink item2 in occupationEE)
		{
			if (item2 != null && !string.IsNullOrEmpty(item2.AssetId))
			{
				list.Add(item2);
				LogDetailed("  CombineEquipmentEntities: Added occupation EE: " + item2.AssetId);
			}
		}
		LogDetailed($"  CombineEquipmentEntities: Total combined before deduplication: {list.Count}");
		List<EquipmentEntityLink> list2 = (from link in list
			group link by link.AssetId).Select(delegate(IGrouping<string, EquipmentEntityLink> group)
		{
			if (group.Count() > 1)
			{
				LogDetailed("  CombineEquipmentEntities: Found duplicate AssetId " + group.Key + ", keeping first occurrence");
			}
			return group.First();
		}).ToList();
		LogDetailed($"  CombineEquipmentEntities: Final unique Equipment Entities: {list2.Count}");
		return list2;
	}

	private List<EquipmentEntityLink> FilterValidEquipmentEntities(List<EquipmentEntityLink> equipmentLinks)
	{
		LogDetailed($"  FilterValidEquipmentEntities: Starting with {equipmentLinks.Count} links...");
		List<EquipmentEntityLink> list = new List<EquipmentEntityLink>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < equipmentLinks.Count; i++)
		{
			EquipmentEntityLink equipmentEntityLink = equipmentLinks[i];
			LogDetailed($"  FilterValidEquipmentEntities: Processing link {i + 1}/{equipmentLinks.Count}");
			if (equipmentEntityLink == null)
			{
				num++;
				LogDetailedWarning($"  FilterValidEquipmentEntities: Link {i + 1} is null");
				continue;
			}
			if (string.IsNullOrEmpty(equipmentEntityLink.AssetId))
			{
				num2++;
				LogDetailedWarning($"  FilterValidEquipmentEntities: Link {i + 1} has empty AssetId");
				continue;
			}
			LogDetailed($"  FilterValidEquipmentEntities: Link {i + 1} AssetId: {equipmentEntityLink.AssetId}");
			try
			{
				LogDetailed($"  FilterValidEquipmentEntities: Attempting to load link {i + 1}...");
				if (equipmentEntityLink.Load() != null)
				{
					list.Add(equipmentEntityLink);
					LogDetailed($"  FilterValidEquipmentEntities: Link {i + 1} loaded successfully");
				}
				else
				{
					num3++;
					LogDetailedWarning($"  FilterValidEquipmentEntities: Link {i + 1} with AssetId {equipmentEntityLink.AssetId} failed to load (returned null)");
				}
			}
			catch (Exception ex)
			{
				num4++;
				LogDetailedWarning($"  FilterValidEquipmentEntities: Exception loading link {i + 1} with AssetId {equipmentEntityLink.AssetId}: {ex.Message}");
			}
		}
		LogDetailed($"  FilterValidEquipmentEntities: SUMMARY - Valid: {list.Count}, Null: {num}, Empty AssetId: {num2}, Load Failed: {num3}, Exceptions: {num4}");
		return list;
	}

	private List<EquipmentEntityLink> GetOccupationEquipmentEntities()
	{
		LogDetailed("  GetOccupationEquipmentEntities: Starting...");
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (mainCharacter == null)
		{
			LogDetailedError("  GetOccupationEquipmentEntities: Main character is null");
			return new List<EquipmentEntityLink>();
		}
		BlueprintFeature mainCharacterOccupation = GetMainCharacterOccupation();
		if (mainCharacterOccupation == null)
		{
			LogDetailed("  GetOccupationEquipmentEntities: No occupation found - this is normal for DefaultPlayerCharacter");
			return new List<EquipmentEntityLink>();
		}
		LogDetailed("  GetOccupationEquipmentEntities: Found occupation: " + mainCharacterOccupation.Name);
		AddKingmakerEquipmentEntity[] array = mainCharacterOccupation.GetComponents<AddKingmakerEquipmentEntity>().ToArray();
		if (array.Length == 0)
		{
			LogDetailed("  GetOccupationEquipmentEntities: No AddKingmakerEquipmentEntity components found in occupation");
			return new List<EquipmentEntityLink>();
		}
		Gender gender = mainCharacter.Description?.Gender ?? Gender.Male;
		Race valueOrDefault = (mainCharacter.Progression?.Race?.RaceId).GetValueOrDefault();
		LogDetailed($"  GetOccupationEquipmentEntities: Using Gender={gender}, Race={valueOrDefault}");
		List<EquipmentEntityLink> list = new List<EquipmentEntityLink>();
		AddKingmakerEquipmentEntity[] array2 = array;
		foreach (AddKingmakerEquipmentEntity addKingmakerEquipmentEntity in array2)
		{
			if (addKingmakerEquipmentEntity.EquipmentEntity != null)
			{
				EquipmentEntityLink[] links = addKingmakerEquipmentEntity.EquipmentEntity.GetLinks(gender, valueOrDefault);
				list.AddRange(links);
				LogDetailed($"  GetOccupationEquipmentEntities: Added {links.Count()} links from {addKingmakerEquipmentEntity.EquipmentEntity.name}");
			}
		}
		LogDetailed($"  GetOccupationEquipmentEntities: Total occupation EE links: {list.Count}");
		return list;
	}

	private BaseUnitEntity CreateStatueUnit(BaseUnitEntity mainCharacter, List<EquipmentEntityLink> equipmentLinks)
	{
		LogDetailed("  CreateStatueUnit: Starting...");
		if (mainCharacter == null)
		{
			LogDetailedError("  CreateStatueUnit: Main character is null!");
			return null;
		}
		LogDetailed("  CreateStatueUnit: Main character: " + mainCharacter.CharacterName);
		try
		{
			LogDetailed("  CreateStatueUnit: Copying main character without pets (createView=true, preview=true)...");
			BaseUnitEntity baseUnitEntity = CopyForStatue(mainCharacter, createView: true, preview: true);
			if (baseUnitEntity == null)
			{
				LogDetailedError("  CreateStatueUnit: Failed to copy main character - returned null");
				return null;
			}
			LogDetailed("  CreateStatueUnit: Statue unit created successfully: " + baseUnitEntity.CharacterName);
			LogDetailed("  CreateStatueUnit: Statue unit blueprint: " + baseUnitEntity.Blueprint?.name);
			LogDetailed("  CreateStatueUnit: Disconnecting from game systems...");
			DisconnectFromGameSystems(baseUnitEntity);
			UnitEntityView view = baseUnitEntity.View;
			if (view == null)
			{
				LogDetailedError("  CreateStatueUnit: Statue unit view is null!");
				baseUnitEntity.Dispose();
				return null;
			}
			LogDetailed("  CreateStatueUnit: View found: " + view.GetType().Name);
			if (view.CharacterAvatar == null)
			{
				LogDetailedError("  CreateStatueUnit: view.CharacterAvatar is null!");
				return baseUnitEntity;
			}
			LogDetailed("  CreateStatueUnit: CharacterAvatar found: " + view.CharacterAvatar.GetType().Name);
			if (equipmentLinks.Count > 0)
			{
				LogDetailed($"  CreateStatueUnit: Adding {equipmentLinks.Count} equipment entities to CharacterAvatar...");
				for (int i = 0; i < equipmentLinks.Count; i++)
				{
					LogDetailed($"  CreateStatueUnit: Adding EE {i + 1}/{equipmentLinks.Count}: {equipmentLinks[i].AssetId}");
				}
				view.CharacterAvatar.AddEquipmentEntities(equipmentLinks);
				LogDetailed("  CreateStatueUnit: Equipment entities added successfully");
				LogDetailed("  CreateStatueUnit: Setting CharacterAvatar.makeTextures = false...");
				view.CharacterAvatar.makeTextures = false;
				LogDetailed("  CreateStatueUnit: Texture creation disabled for statue");
				LogDetailed("  CreateStatueUnit: Activating ViewTransform for Character update...");
				bool activeSelf = view.ViewTransform.gameObject.activeSelf;
				view.ViewTransform.gameObject.SetActive(value: true);
				LogDetailed($"  CreateStatueUnit: ViewTransform activated (was: {activeSelf})");
				LogDetailed("  CreateStatueUnit: Setting CharacterAvatar.IsDirty = true...");
				view.CharacterAvatar.IsDirty = true;
				LogDetailed("  CreateStatueUnit: CharacterAvatar marked as dirty");
				LogDetailed("  CreateStatueUnit: Temporarily enabling Character updates...");
				bool? preventUpdate = view.CharacterAvatar.PreventUpdate;
				view.CharacterAvatar.PreventUpdate = false;
				LogDetailed("  CreateStatueUnit: Calling CharacterAvatar.DoUpdate()...");
				view.CharacterAvatar.DoUpdate();
				LogDetailed("  CreateStatueUnit: CharacterAvatar.DoUpdate() completed");
				view.CharacterAvatar.PreventUpdate = preventUpdate;
				LogDetailed("  CreateStatueUnit: Character update settings restored");
				LogDetailed("  CreateStatueUnit: Finalizing Character mesh building...");
				FinalizeCharacterMeshBuilding(view.CharacterAvatar);
				LogDetailed("  CreateStatueUnit: Character mesh building finalized");
			}
			else
			{
				LogDetailedWarning("  CreateStatueUnit: No equipment links to add");
			}
			LogDetailed("  CreateStatueUnit: Statue unit creation completed successfully");
			return baseUnitEntity;
		}
		catch (Exception ex)
		{
			LogDetailedError("  CreateStatueUnit: Exception occurred: " + ex.Message);
			LogCriticalException(ex);
			return null;
		}
	}

	private void DisconnectFromGameSystems(BaseUnitEntity statueUnit)
	{
		LogDetailed("  DisconnectFromGameSystems: Starting disconnection...");
		if (statueUnit == null)
		{
			LogDetailedError("  DisconnectFromGameSystems: Statue unit is null!");
			return;
		}
		try
		{
			LogDetailed($"  DisconnectFromGameSystems: Setting IsInGame = false (was: {statueUnit.IsInGame})");
			statueUnit.IsInGame = false;
			LogDetailed("  DisconnectFromGameSystems: Unsubscribing from EventBus...");
			statueUnit.Unsubscribe();
			if (statueUnit.IsInCombat)
			{
				LogDetailed("  DisconnectFromGameSystems: Leaving combat...");
				statueUnit.CombatState.LeaveCombat();
			}
			LogDetailed("  DisconnectFromGameSystems: Interrupting all commands...");
			statueUnit.Commands.InterruptAllInterruptible();
			LogDetailed("  DisconnectFromGameSystems: Successfully disconnected from game systems");
		}
		catch (Exception ex)
		{
			LogDetailedError("  DisconnectFromGameSystems: Exception occurred: " + ex.Message);
			LogCriticalException(ex);
		}
	}

	private void SetupStatue(BaseUnitEntity statueUnit)
	{
		LogDetailed("  SetupStatue: Starting...");
		if (statueUnit?.View == null)
		{
			LogDetailedError("  SetupStatue: Statue unit or view is null!");
			return;
		}
		try
		{
			LogDetailed("  SetupStatue: Ensuring ViewTransform is active...");
			if (!statueUnit.View.ViewTransform.gameObject.activeSelf)
			{
				statueUnit.View.ViewTransform.gameObject.SetActive(value: true);
				LogDetailed("  SetupStatue: ViewTransform activated");
			}
			LogDetailed("  SetupStatue: Positioning statue...");
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			Vector3 localScale = base.transform.localScale;
			statueUnit.Position = position;
			statueUnit.SetOrientation(rotation.eulerAngles.y);
			if (statueUnit.View.ViewTransform != null)
			{
				statueUnit.View.ViewTransform.position = position;
				statueUnit.View.ViewTransform.rotation = rotation;
				statueUnit.View.ViewTransform.localScale = localScale;
				statueUnit.View.ViewTransform.SetParent(base.transform);
				statueUnit.View.ViewTransform.localScale = Vector3.one;
				LogDetailed($"  SetupStatue: Positioned at {position}, parented to {base.transform.name}, unit scale reset to (1,1,1)");
			}
			LogDetailed("  SetupStatue: Applying stone material...");
			ApplyStatueMaterial(statueUnit);
			LogDetailed("  SetupStatue: Setting up animator...");
			SetupStatueAnimator(statueUnit);
			LogDetailed("  SetupStatue: Verifying final statue state...");
			VerifyStatueVisibility(statueUnit);
			LogDetailed("  SetupStatue: Disabling gameplay components...");
			DisableGameplayComponents(statueUnit);
			LogDetailed("  SetupStatue: Statue setup completed successfully");
		}
		catch (Exception ex)
		{
			LogDetailedError("  SetupStatue: Exception occurred: " + ex.Message);
			LogCriticalException(ex);
		}
	}

	private void FinalizeCharacterMeshBuilding(Character character)
	{
		LogDetailed("  FinalizeCharacterMeshBuilding: Starting finalization...");
		if (character == null)
		{
			LogDetailedError("  FinalizeCharacterMeshBuilding: Character is null!");
			return;
		}
		LogDetailed("  FinalizeCharacterMeshBuilding: Forcing Character updates...");
		for (int i = 0; i < 20; i++)
		{
			if (!character.gameObject.activeSelf)
			{
				LogDetailedWarning($"  FinalizeCharacterMeshBuilding: GameObject is inactive on iteration {i + 1}");
				character.gameObject.SetActive(value: true);
			}
			bool? preventUpdate = character.PreventUpdate;
			character.PreventUpdate = false;
			character.IsDirty = true;
			character.DoUpdate();
			character.PreventUpdate = preventUpdate;
			try
			{
				FieldInfo field = typeof(Character).GetField("m_OverlaysMerged", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null)
				{
					field.SetValue(character, true);
					LogDetailed("  FinalizeCharacterMeshBuilding: Forced OverlaysMerged = true");
				}
				MethodInfo method = typeof(Character).GetMethod("UpdateCharacter", BindingFlags.Instance | BindingFlags.NonPublic);
				if (method != null)
				{
					LogDetailed("  FinalizeCharacterMeshBuilding: Calling UpdateCharacter directly via reflection...");
					method.Invoke(character, null);
					LogDetailed("  FinalizeCharacterMeshBuilding: UpdateCharacter called successfully");
				}
			}
			catch (Exception ex)
			{
				LogDetailedWarning("  FinalizeCharacterMeshBuilding: Could not force character update: " + ex.Message);
			}
			LogDetailed($"  FinalizeCharacterMeshBuilding: Update {i + 1}/20 - IsDirty={character.IsDirty}, OverlaysMerged={character.OverlaysMerged}");
			SkinnedMeshRenderer[] componentsInChildren = character.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			LogDetailed($"  FinalizeCharacterMeshBuilding: Current SkinnedMeshRenderer count: {componentsInChildren.Length}");
			if (!character.IsDirty && character.OverlaysMerged && componentsInChildren.Length != 0)
			{
				LogDetailed($"  FinalizeCharacterMeshBuilding: Character ready after {i + 1} updates with {componentsInChildren.Length} renderers");
				break;
			}
			Thread.Sleep(50);
		}
		SkinnedMeshRenderer[] componentsInChildren2 = character.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		int num = 0;
		SkinnedMeshRenderer[] array = componentsInChildren2;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			if (skinnedMeshRenderer.sharedMesh != null && skinnedMeshRenderer.enabled && skinnedMeshRenderer.gameObject.activeInHierarchy)
			{
				num++;
				LogDetailed("  FinalizeCharacterMeshBuilding: Valid renderer on " + skinnedMeshRenderer.gameObject.name + " with mesh " + skinnedMeshRenderer.sharedMesh.name);
			}
		}
		LogDetailed($"  FinalizeCharacterMeshBuilding: Final state - IsDirty={character.IsDirty}, OverlaysMerged={character.OverlaysMerged}, Valid renderers: {num}/{componentsInChildren2.Length}");
		if (num > 0)
		{
			LogDetailed($"  FinalizeCharacterMeshBuilding: SUCCESS - Character finalized with {num} valid renderers");
		}
		else
		{
			LogDetailedWarning("  FinalizeCharacterMeshBuilding: WARNING - No valid renderers found after finalization");
		}
		if (!character.OverlaysMerged && num > 0)
		{
			LogDetailed("  FinalizeCharacterMeshBuilding: OverlaysMerged is false but renderers are valid - accepting as ready");
		}
	}

	private void VerifyStatueVisibility(BaseUnitEntity statueUnit)
	{
		LogDetailed("  VerifyStatueVisibility: Starting verification...");
		if (statueUnit?.View?.ViewTransform == null)
		{
			LogDetailedError("  VerifyStatueVisibility: Statue unit or view transform is null!");
			return;
		}
		Transform viewTransform = statueUnit.View.ViewTransform;
		LogDetailed($"  VerifyStatueVisibility: ViewTransform position: {viewTransform.position}");
		LogDetailed($"  VerifyStatueVisibility: ViewTransform active: {viewTransform.gameObject.activeInHierarchy}");
		LogDetailed("  VerifyStatueVisibility: ViewTransform parent: " + (viewTransform.parent ? viewTransform.parent.name : "NULL"));
		Renderer[] componentsInChildren = statueUnit.View.GetComponentsInChildren<Renderer>(includeInactive: true);
		LogDetailed($"  VerifyStatueVisibility: Found {componentsInChildren.Length} renderers");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			bool activeInHierarchy = renderer.gameObject.activeInHierarchy;
			bool flag = renderer.enabled;
			bool flag2 = false;
			if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				flag2 = skinnedMeshRenderer.sharedMesh != null;
				LogDetailed(string.Format("  VerifyStatueVisibility: SkinnedMeshRenderer on {0}: active={1}, enabled={2}, mesh={3}", renderer.gameObject.name, activeInHierarchy, flag, skinnedMeshRenderer.sharedMesh?.name ?? "NULL"));
			}
			else if (renderer is MeshRenderer)
			{
				MeshFilter component = renderer.GetComponent<MeshFilter>();
				flag2 = component?.sharedMesh != null;
				LogDetailed(string.Format("  VerifyStatueVisibility: MeshRenderer on {0}: active={1}, enabled={2}, mesh={3}", renderer.gameObject.name, activeInHierarchy, flag, component?.sharedMesh?.name ?? "NULL"));
			}
			if (activeInHierarchy)
			{
				num++;
			}
			if (flag)
			{
				num2++;
			}
			if (flag2)
			{
				num3++;
			}
		}
		LogDetailed($"  VerifyStatueVisibility: SUMMARY - Total: {componentsInChildren.Length}, Active: {num}, Enabled: {num2}, With Mesh: {num3}");
		Character characterAvatar = statueUnit.View.CharacterAvatar;
		if (characterAvatar != null)
		{
			LogDetailed($"  VerifyStatueVisibility: Character state - IsDirty: {characterAvatar.IsDirty}, OverlaysMerged: {characterAvatar.OverlaysMerged}");
		}
		else
		{
			LogDetailedWarning("  VerifyStatueVisibility: No Character component found!");
		}
		LogDetailed("  VerifyStatueVisibility: Verification completed");
	}

	private void DisableGameplayComponents(BaseUnitEntity statueUnit)
	{
		LogDetailed("  DisableGameplayComponents: Starting...");
		UnitEntityView view = statueUnit.View;
		IK[] componentsInChildren = view.GetComponentsInChildren<IK>();
		foreach (IK iK in componentsInChildren)
		{
			iK.enabled = false;
			LogDetailed("  DisableGameplayComponents: IK component disabled: " + iK.GetType().Name);
		}
		IKController component = view.GetComponent<IKController>();
		if (component != null)
		{
			component.enabled = false;
		}
		GrounderIK component2 = view.GetComponent<GrounderIK>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
		UnitAnimationManager componentInChildren = view.GetComponentInChildren<UnitAnimationManager>();
		if (componentInChildren != null)
		{
			LogDetailed("  DisableGameplayComponents: Destroying UnitAnimationManager component...");
			UnityEngine.Object.DestroyImmediate(componentInChildren);
			LogDetailed("  DisableGameplayComponents: UnitAnimationManager destroyed");
		}
		UnitAnimationCallbackReceiver componentInChildren2 = view.GetComponentInChildren<UnitAnimationCallbackReceiver>();
		if (componentInChildren2 != null)
		{
			LogDetailed("  DisableGameplayComponents: Destroying UnitAnimationCallbackReceiverComp component...");
			UnityEngine.Object.DestroyImmediate(componentInChildren2);
			LogDetailed("  DisableGameplayComponents: UnitAnimationCallbackReceiverComp destroyed");
		}
		RigidbodyCreatureController componentInChildren3 = view.GetComponentInChildren<RigidbodyCreatureController>();
		if (componentInChildren3 != null)
		{
			LogDetailed("  DisableGameplayComponents: Destroying RigidbodyCreatureController component...");
			UnityEngine.Object.DestroyImmediate(componentInChildren3);
			LogDetailed("  DisableGameplayComponents: RigidbodyCreatureController destroyed");
		}
		UnitMovementAgentBase componentInChildren4 = view.GetComponentInChildren<UnitMovementAgentBase>();
		if (componentInChildren4 != null)
		{
			LogDetailed("  DisableGameplayComponents: Destroying UnitMovementAgentBase component...");
			UnityEngine.Object.DestroyImmediate(componentInChildren4);
			LogDetailed("  DisableGameplayComponents: UnitMovementAgentBase destroyed");
		}
		UnitEntityView component3 = view.GetComponent<UnitEntityView>();
		if (component3 != null)
		{
			UnityEngine.Object.DestroyImmediate(component3);
			LogDetailed("  DisableGameplayComponents: UnitEntityView destroyed");
		}
		LogDetailed("  DisableGameplayComponents: Completed");
	}

	private void ApplyStatueMaterial(BaseUnitEntity statueUnit)
	{
		if (m_StoneMaterial == null)
		{
			LogDetailedWarning("  ApplyStatueMaterial: No stone material assigned");
			return;
		}
		Renderer[] componentsInChildren = statueUnit.View.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] array2 = new Material[renderer.materials.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = m_StoneMaterial;
			}
			renderer.materials = array2;
		}
		LogDetailed($"  ApplyStatueMaterial: Applied stone material to {componentsInChildren.Length} renderers");
	}

	private void SetupStatueAnimator(BaseUnitEntity statueUnit)
	{
		Animator componentInChildren = statueUnit.View.GetComponentInChildren<Animator>();
		if (componentInChildren == null)
		{
			LogDetailedWarning("  SetupStatueAnimator: No animator found");
			return;
		}
		componentInChildren.enabled = true;
		if (m_StatueAnimatorController != null)
		{
			componentInChildren.runtimeAnimatorController = m_StatueAnimatorController;
			LogDetailed("  SetupStatueAnimator: Statue animator controller applied: " + m_StatueAnimatorController.name);
		}
		else
		{
			componentInChildren.speed = 0f;
			LogDetailed("  SetupStatueAnimator: No statue controller assigned, animator speed set to 0 (static pose)");
		}
	}

	public void LogOccupationInfo()
	{
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (mainCharacter == null)
		{
			LogDetailedWarning("Main character not found - cannot determine occupation");
			return;
		}
		BlueprintFeature mainCharacterOccupation = GetMainCharacterOccupation();
		if (mainCharacterOccupation != null)
		{
			LogDetailed("=== Main Character Occupation Info ===");
			LogDetailed("Character: " + mainCharacter.CharacterName);
			LogDetailed($"Gender: {mainCharacter.Description?.Gender ?? Gender.Male}");
			LogDetailed(string.Format("Race: {0} ({1})", (mainCharacter.Progression?.Race?.RaceId).GetValueOrDefault(), mainCharacter.Progression?.Race?.Name ?? "Unknown"));
			LogDetailed("Occupation Feature Name: " + mainCharacterOccupation.Name);
			LogDetailed("Occupation Blueprint Name: " + mainCharacterOccupation.name);
			LogDetailed("Occupation Asset GUID: " + mainCharacterOccupation.AssetGuid);
			LogDetailed("=====================================");
		}
		else
		{
			LogDetailedWarning("No occupation found for character: " + mainCharacter.CharacterName);
		}
	}

	public void DestroyStatue()
	{
		LogDetailed("DestroyStatue: Starting statue destruction...");
		if (m_StatueUnit != null)
		{
			try
			{
				LogDetailed("DestroyStatue: Destroying dynamic statue unit...");
				if (m_StatueUnit.View != null)
				{
					m_StatueUnit.DetachView();
				}
				m_StatueUnit.Dispose();
				m_StatueUnit = null;
				LogDetailed("DestroyStatue: Dynamic statue destroyed");
			}
			catch (Exception ex)
			{
				LogCriticalError("DestroyStatue: Exception destroying dynamic statue: " + ex.Message);
				LogCriticalException(ex);
			}
		}
		HideDefaultStatue();
		m_StatueCreationAttempted = false;
		LogDetailed("DestroyStatue: All statues destroyed and creation flag reset");
	}

	private void OnDestroy()
	{
		LogDetailed("OnDestroy: Component destroyed");
		EventBus.Unsubscribe(this);
		DestroyStatue();
	}

	public string GetStatueInfo()
	{
		string text = "=== Main Character Statue EE Info ===\n";
		BaseUnitEntity mainCharacter = GetMainCharacter();
		if (mainCharacter != null)
		{
			text = text + "Main Character: " + mainCharacter.CharacterName + "\n";
			text = text + "Blueprint: " + mainCharacter.Blueprint?.name + "\n";
			text += $"Gender: {mainCharacter.Description?.Gender}\n";
			BlueprintFeature mainCharacterOccupation = GetMainCharacterOccupation();
			text = ((mainCharacterOccupation == null) ? (text + "Occupation: Not found\n") : (text + "Occupation: " + mainCharacterOccupation.Name + " (" + mainCharacterOccupation.name + ")\n"));
			List<EquipmentEntityLink> currentEquipmentEntities = GetCurrentEquipmentEntities();
			List<EquipmentEntityLink> occupationEquipmentEntities = GetOccupationEquipmentEntities();
			List<EquipmentEntityLink> list = CombineEquipmentEntities(currentEquipmentEntities, occupationEquipmentEntities);
			List<EquipmentEntityLink> list2 = FilterValidEquipmentEntities(list);
			text += $"Current EE Count: {currentEquipmentEntities.Count}\n";
			text += $"Occupation EE Count: {occupationEquipmentEntities.Count}\n";
			text += $"Combined EE Count: {list.Count}\n";
			text += $"Valid EE Count: {list2.Count}\n";
		}
		else
		{
			text += "Main Character: Not found\n";
		}
		text += $"Statue Created: {IsStatueCreated}\n";
		text += $"Creation Attempted: {m_StatueCreationAttempted}\n";
		text += $"Auto Create On Start: {m_CreateOnStart}\n";
		text = ((m_StatueUnit != null && IsStatueVisuallyReady()) ? (text + "Statue Type: Dynamic (from MainCharacter)\n") : ((!(m_DefaultStatueGO != null) || !m_DefaultStatueGO.activeInHierarchy) ? (text + "Statue Type: None\n") : (text + "Statue Type: Fallback (DefaultStatueGO: " + m_DefaultStatueGO.name + ")\n")));
		text = text + "Stone Material: " + (m_StoneMaterial ? m_StoneMaterial.name : "None") + "\n";
		text = text + "Animator Controller: " + (m_StatueAnimatorController ? m_StatueAnimatorController.name : "None") + "\n";
		text = text + "Default Statue GO: " + (m_DefaultStatueGO ? m_DefaultStatueGO.name : "None") + "\n";
		return text + "Default Statue Active: " + (m_DefaultStatueGO ? m_DefaultStatueGO.activeInHierarchy.ToString() : "N/A") + "\n";
	}

	private static BaseUnitEntity CopyForStatue(BaseUnitEntity unit, bool createView, bool preview)
	{
		PFLog.TechArt.Log("CopyForStatue: Creating copy without pet-related facts...");
		BaseUnitEntity baseUnitEntity;
		using (ContextData<DisableStatefulRandomContext>.RequestIf(preview))
		{
			using (ContextData<UnitHelper.PreviewUnit>.RequestIf(preview))
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
				{
					using (ContextData<AddClassLevels.DoNotCreatePlan>.RequestIf(preview))
					{
						baseUnitEntity = unit.OriginalBlueprint.CreateEntity();
					}
				}
			}
		}
		baseUnitEntity.CopyOf = unit;
		baseUnitEntity.Unsubscribe();
		baseUnitEntity.Description.SetName(unit.Description.CustomName);
		baseUnitEntity.UISettings.SetPortrait(unit.Portrait);
		baseUnitEntity.ViewSettings.SetDoll(unit.ViewSettings.Doll);
		baseUnitEntity.Inventory.EnsureOwn();
		if (preview)
		{
			baseUnitEntity.Facts.EnsureFactProcessor<BuffCollection>().SetupPreview(baseUnitEntity);
		}
		baseUnitEntity.Progression.CopyFrom(unit.Progression);
		CopyFactsExcludingPets(unit, baseUnitEntity);
		if (createView)
		{
			UnitEntityView view = baseUnitEntity.CreateView();
			baseUnitEntity.AttachView(view);
		}
		baseUnitEntity.Subscribe();
		PFLog.TechArt.Log("CopyForStatue: Copy created successfully without pets");
		return baseUnitEntity;
	}

	private static void CopyFactsExcludingPets(BaseUnitEntity original, BaseUnitEntity target)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (EntityFact item in original.Facts.List)
		{
			num++;
			if (IsPetRelatedFact(item))
			{
				num2++;
				PFLog.TechArt.Log("CopyFactsExcludingPets: Skipping pet-related fact: " + item.Blueprint?.name);
			}
			else
			{
				if (item.MaybeContext?.Root.AssociatedBlueprint is BlueprintItemEnchantment)
				{
					continue;
				}
				UnitFact unitFact = item as UnitFact;
				if (item.SourceItem != null)
				{
					continue;
				}
				Feature feature = unitFact as Feature;
				Feature feature2 = ((feature != null) ? target.Progression.Features.Get(feature.Blueprint) : null);
				if ((feature != null && (feature2 == null || feature2.GetRank() < feature.GetRank())) || !target.Facts.Contains(item.Blueprint))
				{
					MechanicsContext mechanicsContext = item.MaybeContext?.ParentContext;
					MechanicsContext mechanicsContext2 = mechanicsContext?.CloneFor(mechanicsContext.AssociatedBlueprint, target);
					mechanicsContext2?.UnlinkParent();
					EntityFact entityFact = feature2 ?? ((unitFact != null) ? unitFact.Blueprint.CreateFact(mechanicsContext2, target, null) : new EntityFact(item.Blueprint));
					if (entityFact is Feature feature3 && feature != null)
					{
						feature3.SetSamePathSource(feature);
						feature3.Param = feature.Param;
					}
					if (entityFact is IFactWithRanks factWithRanks && unitFact is IFactWithRanks factWithRanks2)
					{
						int count = factWithRanks2.Rank - entityFact.GetRank();
						factWithRanks.AddRank(count);
					}
					if (entityFact != feature2)
					{
						target.Facts.Add(entityFact);
						num3++;
					}
				}
			}
		}
		PFLog.TechArt.Log($"CopyFactsExcludingPets: Total facts: {num}, Pet facts skipped: {num2}, Facts copied: {num3}");
	}

	private static bool IsPetRelatedFact(EntityFact fact)
	{
		if (fact?.Blueprint == null)
		{
			return false;
		}
		BlueprintComponent[] componentsArray = fact.Blueprint.ComponentsArray;
		foreach (BlueprintComponent blueprintComponent in componentsArray)
		{
			if (blueprintComponent is PetOwner)
			{
				return true;
			}
			if (blueprintComponent is AddFactsToPet)
			{
				return true;
			}
			string text = blueprintComponent.GetType().Name;
			if (text.Contains("Pet"))
			{
				return true;
			}
			if (text.Contains("Follow"))
			{
				return true;
			}
		}
		string text2 = fact.Blueprint.name;
		if (text2 != null)
		{
			if (text2.Contains("Pet"))
			{
				return true;
			}
			if (text2.Contains("Follow"))
			{
				return true;
			}
			if (text2.Contains("Companion") && text2.Contains("Animal"))
			{
				return true;
			}
		}
		return false;
	}
}
