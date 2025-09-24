using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.UnitLogic.Buffs;

[Serializable]
[TypeId("72edc5c3225c64449ad4766f9c759091")]
public class PetPolymorphRTBuff : UnitBuffComponentDelegate, IUnitSpawnHandler<EntitySubscriber>, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitSpawnHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[Tooltip("Префаб для Servitor пета")]
	private UnitViewLink m_ServitorPrefab;

	[SerializeField]
	[Tooltip("Префаб для Mastiff пета")]
	private UnitViewLink m_MastiffPrefab;

	[SerializeField]
	[Tooltip("Префаб для Eagle пета")]
	private UnitViewLink m_EaglePrefab;

	[SerializeField]
	[Tooltip("Префаб для ServoskullSwarm пета")]
	private UnitViewLink m_ServoskullSwarmPrefab;

	[SerializeField]
	[Tooltip("Префаб для Raven пета")]
	private UnitViewLink m_RavenPrefab;

	[SerializeField]
	[HideInInspector]
	[Tooltip("Маркер для идентификации PetPolymorphRTBuff баффов")]
	private bool m_IsPetPolymorphRTBuff = true;

	public UnitViewLink ServitorPrefab => m_ServitorPrefab;

	public UnitViewLink MastiffPrefab => m_MastiffPrefab;

	public UnitViewLink EaglePrefab => m_EaglePrefab;

	public UnitViewLink ServoskullSwarmPrefab => m_ServoskullSwarmPrefab;

	public UnitViewLink RavenPrefab => m_RavenPrefab;

	public bool IsPetPolymorphRTBuff => m_IsPetPolymorphRTBuff;

	public UnitViewLink GetPrefab(AbstractUnitEntity unit)
	{
		PetType petType = PetType.Servitor;
		if (unit is BaseUnitEntity { IsPet: not false, Master: not null } baseUnitEntity)
		{
			UnitPartPetOwner optional = baseUnitEntity.Master.GetOptional<UnitPartPetOwner>();
			if (optional != null)
			{
				petType = optional.PetType;
				PFLog.TechArt.Log($"PetPolymorphRTBuff.GetPrefab: Определили тип пета {petType} для {unit?.CharacterName}");
			}
			else
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.GetPrefab: Не найден UnitPartPetOwner у мастера " + baseUnitEntity.Master.CharacterName + " для пета " + unit?.CharacterName);
			}
		}
		else
		{
			PFLog.TechArt.Warning("PetPolymorphRTBuff.GetPrefab: Юнит " + unit?.CharacterName + " не является петом или нет мастера");
		}
		UnitViewLink unitViewLink = null;
		switch (petType)
		{
		case PetType.Servitor:
			unitViewLink = m_ServitorPrefab;
			break;
		case PetType.Mastiff:
			unitViewLink = m_MastiffPrefab;
			break;
		case PetType.Eagle:
			unitViewLink = m_EaglePrefab;
			break;
		case PetType.ServoskullSwarm:
			unitViewLink = m_ServoskullSwarmPrefab;
			break;
		case PetType.Raven:
			unitViewLink = m_RavenPrefab;
			break;
		default:
			PFLog.TechArt.Warning($"PetPolymorphRTBuff.GetPrefab: Неизвестный тип пета {petType}, используем Servitor");
			unitViewLink = m_ServitorPrefab;
			break;
		}
		if (unitViewLink == null || !unitViewLink.Exists())
		{
			PFLog.TechArt.Warning($"PetPolymorphRTBuff.GetPrefab: Префаб для типа {petType} не задан или недоступен для {unit?.CharacterName}, используем оригинальный префаб");
			return unit?.Blueprint?.Prefab;
		}
		PFLog.TechArt.Log($"PetPolymorphRTBuff.GetPrefab: Выбран префаб для типа {petType} для {unit?.CharacterName}");
		return unitViewLink;
	}

	protected override void OnActivate()
	{
		if (IsInDollRoomMode())
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Skipping activation as we are in DollRoom mode");
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		BaseUnitEntity owner = base.Owner;
		if (owner != null && owner.IsPet)
		{
			baseUnitEntity = owner;
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Owner " + base.Owner?.CharacterName + " is a pet, will transform it directly");
		}
		else
		{
			UnitPartPetOwner optional = base.Owner.GetOptional<UnitPartPetOwner>();
			if (optional == null || optional.PetType < PetType.Servitor || optional.PetType > PetType.Raven || optional.PetUnit == null)
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.OnActivate: Owner " + base.Owner?.CharacterName + " is not a pet and has no pet, skipping transformation");
				return;
			}
			baseUnitEntity = optional.PetUnit;
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Owner " + base.Owner?.CharacterName + " has pet " + baseUnitEntity?.CharacterName + ", will transform the pet");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Starting for unit " + baseUnitEntity?.CharacterName + " (ID: " + baseUnitEntity?.UniqueId + ")");
		if (baseUnitEntity.GetOptional<PartPetPolymorphed>() != null)
		{
			PFLog.TechArt.Error("PetPolymorphRTBuff: Can't apply pet polymorph over existing polymorph effect on " + baseUnitEntity?.CharacterName);
			return;
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff: No existing PartPetPolymorphed found on " + baseUnitEntity?.CharacterName);
		PartPetPolymorphed optional2 = baseUnitEntity.GetOptional<PartPetPolymorphed>();
		if (optional2 != null && optional2.Component != null && optional2.Component != this)
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: Found existing PartPetPolymorphed on " + baseUnitEntity?.CharacterName + ", checking for buffs to remove");
			List<Buff> list = new List<Buff>();
			foreach (Buff buff in baseUnitEntity.Buffs)
			{
				PFLog.TechArt.Log($"PetPolymorphRTBuff: Checking buff {buff.Blueprint.name} with {buff.Components.Count()} components");
				foreach (EntityFactComponent component in buff.Components)
				{
					PFLog.TechArt.Log("PetPolymorphRTBuff: Component type: " + component.GetType().Name);
					if (component is UnitBuffComponentRuntime unitBuffComponentRuntime)
					{
						PFLog.TechArt.Log("PetPolymorphRTBuff: Found UnitBuffComponentRuntime, checking SourceBlueprintComponent type: " + unitBuffComponentRuntime.SourceBlueprintComponent?.GetType().Name);
						if (unitBuffComponentRuntime.SourceBlueprintComponent is PetPolymorphRTBuff { IsPetPolymorphRTBuff: not false } petPolymorphRTBuff && petPolymorphRTBuff != this)
						{
							PFLog.TechArt.Log("PetPolymorphRTBuff: Found PetPolymorphRTBuff component with marker to remove in buff " + buff.Blueprint.name);
							list.Add(buff);
							break;
						}
					}
				}
			}
			PFLog.TechArt.Log($"PetPolymorphRTBuff: Will remove {list.Count} existing pet polymorph buffs");
			foreach (Buff item in list)
			{
				PFLog.TechArt.Log("PetPolymorphRTBuff: Removing existing pet polymorph buff: " + item.Blueprint.name);
				baseUnitEntity.Buffs.Remove(item);
			}
		}
		else
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: No existing PartPetPolymorphed found on " + baseUnitEntity?.CharacterName + ", or it's the same component");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff: Creating new PartPetPolymorphed for " + baseUnitEntity?.CharacterName);
		baseUnitEntity.GetOrCreate<PartPetPolymorphed>().Setup(this);
		PFLog.TechArt.Log("PetPolymorphRTBuff: PartPetPolymorphed setup completed for " + baseUnitEntity?.CharacterName);
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Owner " + baseUnitEntity?.CharacterName + " HoldingState is " + ((baseUnitEntity?.HoldingState != null) ? "not null" : "null"));
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Owner " + baseUnitEntity?.CharacterName + " View is " + ((baseUnitEntity?.View != null) ? "not null" : "null"));
		LogChannel techArt = PFLog.TechArt;
		string obj = baseUnitEntity?.CharacterName;
		object obj2;
		if (baseUnitEntity != null)
		{
			BaseUnitEntity baseUnitEntity2 = baseUnitEntity;
			if (baseUnitEntity2.IsPet)
			{
				obj2 = "true";
				goto IL_04dc;
			}
		}
		obj2 = "false";
		goto IL_04dc;
		IL_04dc:
		techArt.Log("PetPolymorphRTBuff.OnActivate: Owner " + obj + " IsPet is " + (string)obj2);
		if (baseUnitEntity.HoldingState != null)
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: Owner has HoldingState, calling TryReplaceView for " + baseUnitEntity?.CharacterName);
			TryReplaceView();
		}
		else if (baseUnitEntity.View != null)
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: Owner has View but no HoldingState, calling TryReplaceView for " + baseUnitEntity?.CharacterName);
			TryReplaceView();
		}
		else
		{
			PFLog.TechArt.Warning("PetPolymorphRTBuff: Owner " + baseUnitEntity?.CharacterName + " has no HoldingState and no View, skipping view replacement - will wait for HandleUnitSpawned");
			EventBus.Subscribe(this);
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivate: Completed for unit " + baseUnitEntity?.CharacterName);
	}

	protected override void OnActivateOrPostLoad()
	{
		if (IsInDollRoomMode())
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Skipping activation as we are in DollRoom mode");
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		BaseUnitEntity owner = base.Owner;
		if (owner != null && owner.IsPet)
		{
			baseUnitEntity = owner;
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Owner " + base.Owner?.CharacterName + " is a pet, will transform it directly");
		}
		else
		{
			UnitPartPetOwner optional = base.Owner.GetOptional<UnitPartPetOwner>();
			if (optional == null || optional.PetType < PetType.Servitor || optional.PetType > PetType.Raven || optional.PetUnit == null)
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.OnActivateOrPostLoad: Owner " + base.Owner?.CharacterName + " is not a pet and has no pet, skipping transformation");
				return;
			}
			baseUnitEntity = optional.PetUnit;
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Owner " + base.Owner?.CharacterName + " has pet " + baseUnitEntity?.CharacterName + ", will transform the pet");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Starting for unit " + baseUnitEntity?.CharacterName + " (ID: " + baseUnitEntity?.UniqueId + ")");
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Owner " + baseUnitEntity?.CharacterName + " HoldingState is " + ((baseUnitEntity?.HoldingState != null) ? "not null" : "null"));
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Owner " + baseUnitEntity?.CharacterName + " View is " + ((baseUnitEntity?.View != null) ? "not null" : "null"));
		LogChannel techArt = PFLog.TechArt;
		string obj = baseUnitEntity?.CharacterName;
		object obj2;
		if (baseUnitEntity != null)
		{
			BaseUnitEntity baseUnitEntity2 = baseUnitEntity;
			if (baseUnitEntity2.IsPet)
			{
				obj2 = "true";
				goto IL_0216;
			}
		}
		obj2 = "false";
		goto IL_0216;
		IL_0216:
		techArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Owner " + obj + " IsPet is " + (string)obj2);
		PartPetPolymorphed optional2 = baseUnitEntity.GetOptional<PartPetPolymorphed>();
		if (optional2 != null && optional2.Component != null)
		{
			PFLog.TechArt.Error("PetPolymorphRTBuff: Can't apply pet polymorph over existing polymorph effect on " + baseUnitEntity?.CharacterName);
			return;
		}
		PartPetPolymorphed optional3 = baseUnitEntity.GetOptional<PartPetPolymorphed>();
		if (optional3 != null && optional3.Component != null && optional3.Component != this)
		{
			PFLog.TechArt.Error("PetPolymorphRTBuff: Can't apply two pet polymorph effects to one character " + baseUnitEntity?.CharacterName);
			return;
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff: Setting up PartPetPolymorphed for " + baseUnitEntity?.CharacterName);
		optional3 = baseUnitEntity.GetOrCreate<PartPetPolymorphed>();
		optional3.Setup(this);
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnActivateOrPostLoad: Completed for unit " + baseUnitEntity?.CharacterName);
	}

	protected override void OnDeactivate()
	{
		if (IsInDollRoomMode())
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnDeactivate: Skipping deactivation as we are in DollRoom mode");
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		BaseUnitEntity owner = base.Owner;
		if (owner != null && owner.IsPet)
		{
			baseUnitEntity = owner;
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnDeactivate: Owner " + base.Owner?.CharacterName + " is a pet, will restore it directly");
		}
		else
		{
			UnitPartPetOwner optional = base.Owner.GetOptional<UnitPartPetOwner>();
			if (optional == null || optional.PetType < PetType.Servitor || optional.PetType > PetType.Raven || optional.PetUnit == null)
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.OnDeactivate: Owner " + base.Owner?.CharacterName + " is not a pet and has no pet, skipping restoration");
				return;
			}
			baseUnitEntity = optional.PetUnit;
			PFLog.TechArt.Log("PetPolymorphRTBuff.OnDeactivate: Owner " + base.Owner?.CharacterName + " has pet " + baseUnitEntity?.CharacterName + ", will restore the pet");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnDeactivate: Starting for unit " + baseUnitEntity?.CharacterName + " (ID: " + baseUnitEntity?.UniqueId + ")");
		if (baseUnitEntity.GetOptional<PartPetPolymorphed>()?.ViewReplacement != null)
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: Restoring view for " + baseUnitEntity?.CharacterName);
			RestoreView();
		}
		else
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: No ViewReplacement found for " + baseUnitEntity?.CharacterName + ", skipping RestoreView");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff: Removing PartPetPolymorphed from " + baseUnitEntity?.CharacterName);
		baseUnitEntity.Remove<PartPetPolymorphed>();
		PFLog.TechArt.Log("PetPolymorphRTBuff.OnDeactivate: Completed for unit " + baseUnitEntity?.CharacterName);
	}

	public void HandleUnitSpawned()
	{
		if (IsInDollRoomMode())
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Skipping handling as we are in DollRoom mode");
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		BaseUnitEntity owner = base.Owner;
		if (owner != null && owner.IsPet)
		{
			baseUnitEntity = owner;
			PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Owner " + base.Owner?.CharacterName + " is a pet, will handle it directly");
		}
		else
		{
			UnitPartPetOwner optional = base.Owner.GetOptional<UnitPartPetOwner>();
			if (optional == null || optional.PetType < PetType.Servitor || optional.PetType > PetType.Raven || optional.PetUnit == null)
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.HandleUnitSpawned: Owner " + base.Owner?.CharacterName + " is not a pet and has no pet, skipping handling");
				return;
			}
			baseUnitEntity = optional.PetUnit;
			PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Owner " + base.Owner?.CharacterName + " has pet " + baseUnitEntity?.CharacterName + ", will handle the pet");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Processing unit " + baseUnitEntity?.CharacterName);
		PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Owner " + baseUnitEntity?.CharacterName + " HoldingState is " + ((baseUnitEntity?.HoldingState != null) ? "not null" : "null"));
		PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Owner " + baseUnitEntity?.CharacterName + " View is " + ((baseUnitEntity?.View != null) ? "not null" : "null"));
		LogChannel techArt = PFLog.TechArt;
		string obj = baseUnitEntity?.CharacterName;
		object obj2;
		if (baseUnitEntity != null)
		{
			BaseUnitEntity baseUnitEntity2 = baseUnitEntity;
			if (baseUnitEntity2.IsPet)
			{
				obj2 = "true";
				goto IL_01eb;
			}
		}
		obj2 = "false";
		goto IL_01eb;
		IL_01eb:
		techArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Owner " + obj + " IsPet is " + (string)obj2);
		PartPetPolymorphed optional2 = baseUnitEntity.GetOptional<PartPetPolymorphed>();
		if (optional2 != null && baseUnitEntity.View != null)
		{
			if (optional2.ViewReplacement == null)
			{
				optional2.ViewReplacement = baseUnitEntity.View.gameObject;
			}
		}
		else if (baseUnitEntity.HoldingState != null)
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Calling TryReplaceView for " + baseUnitEntity?.CharacterName + " as HoldingState is not null");
			TryReplaceView();
		}
		else
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.HandleUnitSpawned: Skipping TryReplaceView for " + baseUnitEntity?.CharacterName + " as HoldingState is null");
		}
	}

	private void TryReplaceView()
	{
		if (IsInDollRoomMode())
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.TryReplaceView: Skipping view replacement as we are in DollRoom mode");
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		BaseUnitEntity owner = base.Owner;
		if (owner != null && owner.IsPet)
		{
			baseUnitEntity = owner;
			PFLog.TechArt.Log("PetPolymorphRTBuff.TryReplaceView: Owner " + base.Owner?.CharacterName + " is a pet, will transform it directly");
		}
		else
		{
			UnitPartPetOwner optional = base.Owner.GetOptional<UnitPartPetOwner>();
			if (optional == null || optional.PetType < PetType.Servitor || optional.PetType > PetType.Raven || optional.PetUnit == null)
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.TryReplaceView: Owner " + base.Owner?.CharacterName + " is not a pet and has no pet, skipping transformation");
				return;
			}
			baseUnitEntity = optional.PetUnit;
			PFLog.TechArt.Log("PetPolymorphRTBuff.TryReplaceView: Owner " + base.Owner?.CharacterName + " has pet " + baseUnitEntity?.CharacterName + ", will transform the pet");
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff.TryReplaceView: Starting for unit " + baseUnitEntity?.CharacterName);
		PartPetPolymorphed optional2 = baseUnitEntity.GetOptional<PartPetPolymorphed>();
		if (optional2 == null)
		{
			PFLog.TechArt.Warning("PetPolymorphRTBuff: No PartPetPolymorphed found for " + baseUnitEntity?.CharacterName);
			return;
		}
		if (!baseUnitEntity.View)
		{
			PFLog.TechArt.Warning("PetPolymorphRTBuff: No View found for " + baseUnitEntity?.CharacterName);
			return;
		}
		UnitEntityView unitEntityView = GetPrefab(baseUnitEntity).Load();
		if (unitEntityView == null)
		{
			PFLog.TechArt.Warning("PetPolymorphRTBuff: Failed to load prefab for " + baseUnitEntity?.CharacterName);
			return;
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff: Successfully loaded prefab " + unitEntityView.name + " for " + baseUnitEntity?.CharacterName);
		int num = -1;
		int num2 = -1;
		PetCharacter componentInChildren = baseUnitEntity.View.GetComponentInChildren<PetCharacter>();
		if (componentInChildren != null)
		{
			num = componentInChildren.PetRamp01Index;
			num2 = componentInChildren.PetRamp02Index;
			PFLog.TechArt.Log($"PetPolymorphRTBuff: Saved color ramp indices from original pet: {num}, {num2} for {baseUnitEntity?.CharacterName}");
		}
		else
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: No PetCharacter component found on original view for " + baseUnitEntity?.CharacterName);
		}
		if ((bool)optional2.ViewReplacement)
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: Destroying existing ViewReplacement for " + baseUnitEntity?.CharacterName);
			UnityEngine.Object.Destroy(optional2.ViewReplacement);
			optional2.ViewReplacement = null;
		}
		PFLog.TechArt.Log("PetPolymorphRTBuff: Clearing particle effects for " + baseUnitEntity?.CharacterName);
		foreach (Buff buff in baseUnitEntity.Buffs)
		{
			buff.ClearParticleEffect();
		}
		UnitEntityView view = baseUnitEntity.View;
		PFLog.TechArt.Log("PetPolymorphRTBuff: Creating new view from prefab " + unitEntityView.name + " for " + baseUnitEntity?.CharacterName);
		UnitEntityView component = UnityEngine.Object.Instantiate(unitEntityView).GetComponent<UnitEntityView>();
		component.UniqueId = baseUnitEntity.UniqueId;
		UnityEngine.SceneManagement.Scene scene = view.gameObject.scene;
		PFLog.TechArt.Log("PetPolymorphRTBuff: Moving new view to scene " + scene.name + " for " + baseUnitEntity?.CharacterName);
		SceneManager.MoveGameObjectToScene(component.gameObject, scene);
		component.ViewTransform.position = view.ViewTransform.position;
		component.ViewTransform.rotation = view.ViewTransform.rotation;
		component.DisableSizeScaling = true;
		component.Blueprint = baseUnitEntity.Blueprint;
		optional2.ViewReplacement = component.gameObject;
		baseUnitEntity.AttachToViewOnLoad(component);
		if (num >= 0 || num2 >= 0)
		{
			PetCharacter componentInChildren2 = component.GetComponentInChildren<PetCharacter>();
			if (componentInChildren2 != null)
			{
				PFLog.TechArt.Log($"PetPolymorphRTBuff: Applying saved color ramp indices {num}, {num2} to new pet {baseUnitEntity?.CharacterName}");
				componentInChildren2.ApplyRampsByIndicesFromOwnPresets(num, num2);
				componentInChildren2.SaveRampIndicesToDoll(baseUnitEntity);
				PFLog.TechArt.Log("PetPolymorphRTBuff: Successfully applied and saved color ramps to new pet " + baseUnitEntity?.CharacterName);
			}
			else
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff: No PetCharacter component found on new view for " + baseUnitEntity?.CharacterName + ", cannot apply color ramps");
			}
		}
		else
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff: No valid color ramp indices to transfer for " + baseUnitEntity?.CharacterName);
		}
		baseUnitEntity.Commands.InterruptAll((AbstractUnitCommand cmd) => !(cmd is UnitMoveTo));
		UIAccess.SelectionManager?.ForceCreateMarks();
		UnityEngine.Object.Destroy(view.gameObject);
		Game.Instance.SelectionCharacter.ReselectCurrentUnit();
		PFLog.TechArt.Log("PetPolymorphRTBuff: View replacement completed for " + baseUnitEntity?.CharacterName);
	}

	private void RestoreView()
	{
		if (IsInDollRoomMode())
		{
			PFLog.TechArt.Log("PetPolymorphRTBuff.RestoreView: Skipping view restoration as we are in DollRoom mode");
			return;
		}
		BaseUnitEntity baseUnitEntity = null;
		BaseUnitEntity owner = base.Owner;
		if (owner != null && owner.IsPet)
		{
			baseUnitEntity = owner;
			PFLog.TechArt.Log("PetPolymorphRTBuff.RestoreView: Owner " + base.Owner?.CharacterName + " is a pet, will restore it directly");
		}
		else
		{
			UnitPartPetOwner optional = base.Owner.GetOptional<UnitPartPetOwner>();
			if (optional == null || optional.PetType < PetType.Servitor || optional.PetType > PetType.Raven || optional.PetUnit == null)
			{
				PFLog.TechArt.Warning("PetPolymorphRTBuff.RestoreView: Owner " + base.Owner?.CharacterName + " is not a pet and has no pet, skipping restoration");
				return;
			}
			baseUnitEntity = optional.PetUnit;
			PFLog.TechArt.Log("PetPolymorphRTBuff.RestoreView: Owner " + base.Owner?.CharacterName + " has pet " + baseUnitEntity?.CharacterName + ", will restore the pet");
		}
		PartPetPolymorphed optional2 = baseUnitEntity.GetOptional<PartPetPolymorphed>();
		if (optional2?.ViewReplacement == null)
		{
			return;
		}
		foreach (Buff buff in baseUnitEntity.Buffs)
		{
			buff.ClearParticleEffect();
		}
		UnitEntityView view = baseUnitEntity.View;
		UnitEntityView unitEntityView = baseUnitEntity.ViewSettings.Instantiate(ignorePolymorph: true);
		if (unitEntityView != null)
		{
			baseUnitEntity.AttachView(unitEntityView);
			UnityEngine.SceneManagement.Scene scene = view.ViewTransform.gameObject.scene;
			SceneManager.MoveGameObjectToScene(baseUnitEntity.View.ViewTransform.gameObject, scene);
			baseUnitEntity.View.ViewTransform.position = view.ViewTransform.position;
			baseUnitEntity.View.ViewTransform.rotation = view.ViewTransform.rotation;
			baseUnitEntity.Commands.InterruptAll((AbstractUnitCommand cmd) => !(cmd is UnitMoveTo));
			UIAccess.SelectionManager?.ForceCreateMarks();
		}
		UnityEngine.Object.Destroy(optional2.ViewReplacement);
		optional2.ViewReplacement = null;
		Game.Instance.SelectionCharacter.ReselectCurrentUnit();
	}

	private bool IsInDollRoomMode()
	{
		UIDollRooms instance = UIDollRooms.Instance;
		if ((object)instance != null && instance.CharacterDollRoom?.IsVisible == true)
		{
			return true;
		}
		UIDollRooms instance2 = UIDollRooms.Instance;
		if ((object)instance2 != null && instance2.CharGenDollRoom?.IsVisible == true)
		{
			return true;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
