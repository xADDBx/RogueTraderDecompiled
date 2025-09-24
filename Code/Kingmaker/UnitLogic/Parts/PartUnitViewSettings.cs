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
			PartPetPolymorphed optional = base.Owner.GetOptional<PartPetPolymorphed>();
			if (optional?.Component != null)
			{
				UnitEntityView unitEntityView = optional.Component.GetPrefab(base.Owner).Load();
				if (unitEntityView != null)
				{
					Quaternion rotation = (unitEntityView.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
					UnitEntityView component = Object.Instantiate(unitEntityView, base.Owner.Position, rotation).GetComponent<UnitEntityView>();
					optional.ViewReplacement = component.gameObject;
					component.DisableSizeScaling = true;
					return component;
				}
			}
			PartPolymorphed optional2 = base.Owner.GetOptional<PartPolymorphed>();
			UnitEntityView unitEntityView2 = optional2?.Component.GetPrefab(base.Owner).Load();
			if (unitEntityView2 != null)
			{
				Quaternion rotation2 = (unitEntityView2.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
				UnitEntityView component2 = Object.Instantiate(unitEntityView2, base.Owner.Position, rotation2).GetComponent<UnitEntityView>();
				optional2.ViewReplacement = component2.gameObject;
				component2.DisableSizeScaling = true;
				return component2;
			}
		}
		if (Doll?.RacePreset != null)
		{
			UnitEntityView unitEntityView3 = Doll.CreateUnitView();
			unitEntityView3.ViewTransform.position = base.Owner.Position;
			unitEntityView3.ViewTransform.rotation = Quaternion.Euler(0f, base.Owner.Orientation, 0f);
			return unitEntityView3;
		}
		UnitEntityView unitEntityView4 = ResourcesLibrary.TryGetResource<UnitEntityView>(PrefabGuid);
		if (unitEntityView4 == null)
		{
			PFLog.TechArt.Error(base.Owner.Blueprint, "Cannot find prefab for unit: " + PrefabGuid);
			return null;
		}
		Quaternion rotation3 = (unitEntityView4.ForbidRotation ? Quaternion.identity : Quaternion.Euler(0f, base.Owner.Orientation, 0f));
		UnitEntityView unitEntityView5 = Object.Instantiate(unitEntityView4, base.Owner.Position, rotation3);
		if (base.Owner is BaseUnitEntity { IsPet: not false } baseUnitEntity)
		{
			if (Doll == null)
			{
				SetDoll(new DollData());
				PFLog.TechArt.Log("PartUnitViewSettings.Instantiate: Created DollData for pet: " + baseUnitEntity.CharacterName);
			}
			PFLog.TechArt.Log("PartUnitViewSettings.Instantiate: Applying saved pet ramps for pet: " + baseUnitEntity.CharacterName);
			Doll.ApplyPetRamps(unitEntityView5);
		}
		return unitEntityView5;
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
