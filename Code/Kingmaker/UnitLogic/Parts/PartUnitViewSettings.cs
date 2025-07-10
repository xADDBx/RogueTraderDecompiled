using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartUnitViewSettings : MechanicEntityPart<AbstractUnitEntity>, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitViewSettings>, IEntityPartOwner
	{
		PartUnitViewSettings ViewSettings { get; }
	}

	[JsonProperty]
	[CanBeNull]
	private string m_CustomPrefabGuid;

	[JsonProperty]
	[CanBeNull]
	public DollData Doll { get; private set; }

	[CanBeNull]
	public string PrefabGuid
	{
		get
		{
			if (Doll?.RacePreset != null)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(m_CustomPrefabGuid))
			{
				return m_CustomPrefabGuid;
			}
			return base.Owner.Blueprint.Prefab.AssetId;
		}
	}

	protected override void OnAttach()
	{
		SpawningData current = ContextData<SpawningData>.Current;
		if (current != null)
		{
			m_CustomPrefabGuid = current.PrefabGuid;
		}
	}

	public void SetDoll(DollData doll)
	{
		Doll = doll;
	}

	public void SetCustomPrefabGuid(string guid)
	{
		m_CustomPrefabGuid = guid;
	}

	public UnitEntityView Instantiate(bool ignorePolymorph = false)
	{
		if (!ignorePolymorph)
		{
			PartPolymorphed optional = base.Owner.GetOptional<PartPolymorphed>();
			UnitEntityView unitEntityView = optional?.Component.GetPrefab(base.Owner).Load();
			if (unitEntityView != null)
			{
				Quaternion rotation = (unitEntityView.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
				UnitEntityView component = Object.Instantiate(unitEntityView, base.Owner.Position, rotation).GetComponent<UnitEntityView>();
				optional.ViewReplacement = component.gameObject;
				component.DisableSizeScaling = true;
				return component;
			}
		}
		if (Doll?.RacePreset != null)
		{
			UnitEntityView unitEntityView2 = Doll.CreateUnitView();
			unitEntityView2.ViewTransform.position = base.Owner.Position;
			unitEntityView2.ViewTransform.rotation = Quaternion.Euler(0f, base.Owner.Orientation, 0f);
			return unitEntityView2;
		}
		UnitEntityView unitEntityView3 = ResourcesLibrary.TryGetResource<UnitEntityView>(PrefabGuid);
		if (unitEntityView3 == null)
		{
			PFLog.Default.Error(base.Owner.Blueprint, "Cannot find prefab for unit: " + PrefabGuid);
			return null;
		}
		Quaternion rotation2 = (unitEntityView3.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
		UnitEntityView unitEntityView4 = Object.Instantiate(unitEntityView3, base.Owner.Position, rotation2);
		if (base.Owner is BaseUnitEntity { IsPet: not false } baseUnitEntity)
		{
			if (Doll == null)
			{
				SetDoll(new DollData());
				PFLog.TechArt.Log("PartUnitViewSettings.Instantiate: Created DollData for pet: " + baseUnitEntity.CharacterName);
			}
			PFLog.TechArt.Log("PartUnitViewSettings.Instantiate: Applying saved pet ramps for pet: " + baseUnitEntity.CharacterName);
			Doll.ApplyPetRamps(unitEntityView4);
		}
		return unitEntityView4;
	}

	public void PreloadResources()
	{
		if (Doll != null)
		{
			Doll.PreloadEquipmentEntities();
		}
		else if (!string.IsNullOrEmpty(PrefabGuid))
		{
			ResourcesLibrary.PreloadResource<GameObject>(PrefabGuid);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_CustomPrefabGuid);
		Hash128 val2 = ClassHasher<DollData>.GetHash128(Doll);
		result.Append(ref val2);
		return result;
	}
}
