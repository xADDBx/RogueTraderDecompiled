using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartCultAmbush : BaseUnitPart, IHashable
{
	public enum VisibilityStatuses
	{
		NotVisible,
		FirstShow,
		Visible
	}

	public class VisibilityData : IHashable
	{
		[JsonProperty]
		public VisibilityStatuses VisibilityStatus { get; private set; }

		public void UpdateVisibilityStatus(VisibilityStatuses visibilityStatus)
		{
			VisibilityStatus = visibilityStatus;
		}

		protected VisibilityData(VisibilityStatuses visibilityStatus)
		{
			VisibilityStatus = visibilityStatus;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			VisibilityStatuses val = VisibilityStatus;
			result.Append(ref val);
			return result;
		}
	}

	public class VisibilityAbilityData : VisibilityData, IHashable
	{
		[JsonProperty]
		public BlueprintAbilityReference AbilityReference { get; private set; }

		public VisibilityAbilityData(VisibilityStatuses visibilityStatus, BlueprintAbility blueprintAbility)
			: base(visibilityStatus)
		{
			AbilityReference = blueprintAbility.ToReference<BlueprintAbilityReference>();
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(AbilityReference);
			result.Append(ref val2);
			return result;
		}
	}

	public class VisibilityFeatureData : VisibilityData, IHashable
	{
		[JsonProperty]
		public BlueprintFeatureReference FeatureReference { get; private set; }

		public VisibilityFeatureData(VisibilityStatuses visibilityStatus, BlueprintFeature blueprintFeature)
			: base(visibilityStatus)
		{
			FeatureReference = blueprintFeature.ToReference<BlueprintFeatureReference>();
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(FeatureReference);
			result.Append(ref val2);
			return result;
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private bool m_IsAllVisibility;

	[JsonProperty]
	private List<VisibilityData> m_VisibilityData = new List<VisibilityData>();

	[JsonIgnore]
	private readonly Dictionary<string, int> m_VisibilityDataHash = new Dictionary<string, int>();

	[JsonIgnore]
	public bool IsAllVisibility => m_IsAllVisibility;

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		m_VisibilityDataHash.Clear();
		for (int num = m_VisibilityData.Count - 1; num >= 0; num--)
		{
			VisibilityData visibilityData = m_VisibilityData[num];
			string assetGuid;
			if (!(visibilityData is VisibilityAbilityData visibilityAbilityData))
			{
				if (!(visibilityData is VisibilityFeatureData visibilityFeatureData) || visibilityFeatureData.FeatureReference.IsEmpty())
				{
					continue;
				}
				assetGuid = visibilityFeatureData.FeatureReference.Get().AssetGuid;
			}
			else
			{
				if (visibilityAbilityData.AbilityReference.IsEmpty())
				{
					continue;
				}
				assetGuid = visibilityAbilityData.AbilityReference.Get().AssetGuid;
			}
			m_VisibilityDataHash.Add(assetGuid, num);
		}
	}

	public VisibilityStatuses Visibility(Ability ability, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || ability == null || ability.Owner != base.Owner || ability.Data.Weapon != null)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(ability.Blueprint.AssetGuid, isFirstShow);
	}

	public VisibilityStatuses Visibility(Feature feature, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || feature == null || feature.Owner != base.Owner)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(feature.Blueprint.AssetGuid, isFirstShow);
	}

	public VisibilityStatuses Visibility(BaseUnitEntity owner, BlueprintAbility ability, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || ability == null || owner != base.Owner)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(ability.AssetGuid, isFirstShow);
	}

	public VisibilityStatuses Visibility(BaseUnitEntity owner, BlueprintFeature feature, bool isFirstShow = false)
	{
		if (m_IsAllVisibility || feature == null || owner != base.Owner)
		{
			return VisibilityStatuses.Visible;
		}
		return Visibility(feature.AssetGuid, isFirstShow);
	}

	private VisibilityStatuses Visibility(string guid, bool isFirstShow)
	{
		if (m_VisibilityDataHash.TryGetValue(guid, out var value) && value >= 0 && value < m_VisibilityData.Count)
		{
			VisibilityData visibilityData = m_VisibilityData[value];
			VisibilityStatuses visibilityStatus = visibilityData.VisibilityStatus;
			if (visibilityStatus == VisibilityStatuses.FirstShow && isFirstShow)
			{
				visibilityData.UpdateVisibilityStatus(VisibilityStatuses.Visible);
			}
			return visibilityStatus;
		}
		return VisibilityStatuses.NotVisible;
	}

	public void ActivateCultAmbushAbilityFact(BlueprintFeature feature)
	{
		if (!m_IsAllVisibility && feature != null && !m_VisibilityDataHash.ContainsKey(feature.AssetGuid))
		{
			m_VisibilityData.Add(new VisibilityFeatureData(VisibilityStatuses.FirstShow, feature));
			m_VisibilityDataHash.Add(feature.AssetGuid, m_VisibilityData.Count - 1);
		}
	}

	public void Use(BlueprintAbility ability, bool isWeapon)
	{
		if (!(m_IsAllVisibility || isWeapon) && ability != null && !m_VisibilityDataHash.ContainsKey(ability.AssetGuid) && base.Owner.IsInCombat)
		{
			m_VisibilityData.Add(new VisibilityAbilityData(VisibilityStatuses.FirstShow, ability));
			m_VisibilityDataHash.Add(ability.AssetGuid, m_VisibilityData.Count - 1);
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUICultAmbushVisibilityChangeHandler>)delegate(IUICultAmbushVisibilityChangeHandler h)
			{
				h.HandleCultAmbushVisibilityChange();
			}, isCheckRuntime: true);
		}
	}

	public void Use(BlueprintFeature feature)
	{
		if (!m_IsAllVisibility && feature != null && !m_VisibilityDataHash.ContainsKey(feature.AssetGuid) && base.Owner.IsInCombat)
		{
			m_VisibilityData.Add(new VisibilityFeatureData(VisibilityStatuses.FirstShow, feature));
			m_VisibilityDataHash.Add(feature.AssetGuid, m_VisibilityData.Count - 1);
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUICultAmbushVisibilityChangeHandler>)delegate(IUICultAmbushVisibilityChangeHandler h)
			{
				h.HandleCultAmbushVisibilityChange();
			}, isCheckRuntime: true);
		}
	}

	public void MarkAllAsVisibility(bool isCombatPreparation)
	{
		if (isCombatPreparation)
		{
			MarkAllAsVisibilityImpl();
		}
		else if (base.Owner.IsInCombat)
		{
			MarkAllAsVisibilityImpl();
		}
	}

	private void MarkAllAsVisibilityImpl()
	{
		m_IsAllVisibility = true;
		m_VisibilityData.Clear();
		m_VisibilityDataHash.Clear();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUICultAmbushVisibilityChangeHandler>)delegate(IUICultAmbushVisibilityChangeHandler h)
		{
			h.HandleCultAmbushVisibilityChange();
		}, isCheckRuntime: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_IsAllVisibility);
		List<VisibilityData> visibilityData = m_VisibilityData;
		if (visibilityData != null)
		{
			for (int i = 0; i < visibilityData.Count; i++)
			{
				Hash128 val2 = ClassHasher<VisibilityData>.GetHash128(visibilityData[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
