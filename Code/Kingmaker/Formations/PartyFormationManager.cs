using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Formations;

public sealed class PartyFormationManager : EntityPart<Player>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IUnitBuffHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IAreaActivationHandler, IPartyHandler, IUnitCompleteLevelUpHandler, IHashable
{
	[JsonProperty]
	private int m_CurrentFormationIndex;

	[JsonProperty]
	private readonly Dictionary<BlueprintPartyFormation, PartyFormationCustom> m_CustomFormations = new Dictionary<BlueprintPartyFormation, PartyFormationCustom>();

	[JsonProperty]
	private readonly Dictionary<BlueprintPartyFormation, bool> m_PreserveFormation = new Dictionary<BlueprintPartyFormation, bool>();

	private BlueprintPartyFormation m_SelectedFormation;

	private PartyFormationAuto m_AutoFormation;

	private BlueprintPartyFormation SelectedFormation
	{
		get
		{
			if (m_SelectedFormation == null)
			{
				UpdateSelectedFormation();
			}
			return m_SelectedFormation;
		}
	}

	public bool IsCustomFormation => SelectedFormation.Type == PartyFormationType.Custom;

	public bool InvalidTankInAutoFormation
	{
		get
		{
			SureAutoFormation();
			return m_AutoFormation.InvalidTank;
		}
	}

	public int CurrentFormationIndex
	{
		get
		{
			return m_CurrentFormationIndex;
		}
		set
		{
			if (m_CurrentFormationIndex != value)
			{
				m_CurrentFormationIndex = value;
				UpdateSelectedFormation();
				EventBus.RaiseEvent(delegate(IFormationUIHandlers h)
				{
					h.CurrentFormationChanged(m_CurrentFormationIndex);
				});
			}
		}
	}

	[NotNull]
	public IPartyFormation CurrentFormation => GetPartyFormation(SelectedFormation);

	private IPartyFormation GetPartyFormation(BlueprintPartyFormation formation)
	{
		if (formation.Type == PartyFormationType.Custom)
		{
			return SureCustomFormation(formation);
		}
		if (formation.Type == PartyFormationType.Auto)
		{
			return SureAutoFormation();
		}
		return formation;
	}

	protected override void OnAttach()
	{
		UpdateSelectedFormation();
	}

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		ReferenceArrayProxy<BlueprintPartyFormation> predefinedFormations = BlueprintRoot.Instance.Formations.PredefinedFormations;
		int i = 0;
		for (int length = predefinedFormations.Length; i < length; i++)
		{
			GetPartyFormation(predefinedFormations[i]);
		}
	}

	private void UpdateSelectedFormation()
	{
		FormationsRoot formations = BlueprintRoot.Instance.Formations;
		if (formations.PredefinedFormations.Length > 0)
		{
			m_SelectedFormation = formations.PredefinedFormations[m_CurrentFormationIndex];
		}
	}

	private PartyFormationCustom SureCustomFormation(BlueprintPartyFormation blueprint)
	{
		if (!m_CustomFormations.TryGetValue(blueprint, out var value))
		{
			value = new PartyFormationCustom(blueprint.Positions.ToArray());
			m_CustomFormations[blueprint] = value;
		}
		return value;
	}

	private PartyFormationAuto SureAutoFormation()
	{
		if (m_AutoFormation == null)
		{
			m_AutoFormation = new PartyFormationAuto();
			PartyAutoFormationHelper.Setup(m_AutoFormation);
		}
		if (m_AutoFormation.Dirty && !Game.Instance.Player.IsInCombat)
		{
			UpdateAutoFormation();
		}
		return m_AutoFormation;
	}

	public void UpdateAutoFormation()
	{
		if (m_AutoFormation == null)
		{
			m_AutoFormation = new PartyFormationAuto();
		}
		PartyAutoFormationHelper.Setup(m_AutoFormation);
		m_AutoFormation.Dirty = false;
	}

	private void MarkAutoFormationDirty([CanBeNull] BaseUnitEntity unit = null)
	{
		if (m_AutoFormation != null && (unit == null || unit.IsInPlayerParty))
		{
			m_AutoFormation.Dirty = true;
		}
	}

	public void ResetCustomFormation(int formationIndex)
	{
		ReferenceArrayProxy<BlueprintPartyFormation> predefinedFormations = BlueprintRoot.Instance.Formations.PredefinedFormations;
		if (formationIndex < 0 || predefinedFormations.Length <= formationIndex)
		{
			return;
		}
		BlueprintPartyFormation blueprintPartyFormation = predefinedFormations[formationIndex];
		if (blueprintPartyFormation.Type == PartyFormationType.Custom)
		{
			m_CustomFormations.Remove(blueprintPartyFormation);
			SureCustomFormation(blueprintPartyFormation);
			EventBus.RaiseEvent(delegate(IFormationUIHandlers h)
			{
				h.CurrentFormationChanged(m_CurrentFormationIndex);
			});
		}
	}

	public void SetPreserveFormation(bool value)
	{
		if (SelectedFormation.Type != PartyFormationType.Auto)
		{
			m_PreserveFormation[SelectedFormation] = value;
		}
	}

	public bool GetPreserveFormation()
	{
		if (SelectedFormation.Type != PartyFormationType.Auto)
		{
			return m_PreserveFormation.Get(SelectedFormation, defaultValue: true);
		}
		return true;
	}

	public void SetOffset(int formationIndex, int index, BaseUnitEntity unit, Vector2 vector)
	{
		ReferenceArrayProxy<BlueprintPartyFormation> predefinedFormations = BlueprintRoot.Instance.Formations.PredefinedFormations;
		if (formationIndex >= 0 && predefinedFormations.Length > formationIndex)
		{
			BlueprintPartyFormation formation = predefinedFormations[formationIndex];
			GetPartyFormation(formation).SetOffset(index, unit, vector);
			EventBus.RaiseEvent(delegate(IFormationUIHandlers h)
			{
				h.CurrentFormationChanged(m_CurrentFormationIndex);
			});
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		MarkAutoFormationDirty(slot.Owner as BaseUnitEntity);
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		MarkAutoFormationDirty(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleBuffDidAdded(Buff buff)
	{
		MarkAutoFormationDirty(buff.Owner);
	}

	public void HandleBuffRankIncreased(Buff buff)
	{
	}

	public void HandleBuffRankDecreased(Buff buff)
	{
	}

	public void HandleBuffDidRemoved(Buff buff)
	{
		MarkAutoFormationDirty(buff.Owner);
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null)
		{
			if (prevLifeState == UnitLifeState.Conscious && baseUnitEntity.LifeState.State != 0)
			{
				MarkAutoFormationDirty(baseUnitEntity);
			}
			else if (prevLifeState != 0 && baseUnitEntity.LifeState.State == UnitLifeState.Conscious)
			{
				MarkAutoFormationDirty(baseUnitEntity);
			}
		}
	}

	public void HandleUnitCompleteLevelup()
	{
		MarkAutoFormationDirty(EventInvokerExtensions.BaseUnitEntity);
	}

	public void OnAreaActivated()
	{
		MarkAutoFormationDirty();
	}

	public void HandleAddCompanion()
	{
		MarkAutoFormationDirty();
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		MarkAutoFormationDirty();
	}

	public void HandleCompanionActivated()
	{
		MarkAutoFormationDirty();
	}

	public void HandleCapitalModeChanged()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_CurrentFormationIndex);
		Dictionary<BlueprintPartyFormation, PartyFormationCustom> customFormations = m_CustomFormations;
		if (customFormations != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintPartyFormation, PartyFormationCustom> item in customFormations)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<PartyFormationCustom>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		Dictionary<BlueprintPartyFormation, bool> preserveFormation = m_PreserveFormation;
		if (preserveFormation != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<BlueprintPartyFormation, bool> item2 in preserveFormation)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val6);
				bool obj = item2.Value;
				Hash128 val7 = UnmanagedHasher<bool>.GetHash128(ref obj);
				hash2.Append(ref val7);
				val5 ^= hash2.GetHashCode();
			}
			result.Append(ref val5);
		}
		return result;
	}
}
