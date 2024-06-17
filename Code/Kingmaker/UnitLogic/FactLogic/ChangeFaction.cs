using System;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("1fbc22235f60369428389f7ee134267f")]
public class ChangeFaction : UnitFactComponentDelegate, IHashable
{
	public class ComponentData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public BlueprintFaction OriginalFaction;

		[JsonProperty]
		public string OriginalGroupId;

		[JsonProperty]
		public List<BlueprintFaction> OriginalAttackFactions;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(OriginalFaction);
			result.Append(ref val2);
			result.Append(OriginalGroupId);
			List<BlueprintFaction> originalAttackFactions = OriginalAttackFactions;
			if (originalAttackFactions != null)
			{
				for (int i = 0; i < originalAttackFactions.Count; i++)
				{
					Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(originalAttackFactions[i]);
					result.Append(ref val3);
				}
			}
			return result;
		}
	}

	private enum ChangeType
	{
		ToNeutrals,
		ToCaster,
		ToCustom
	}

	[SerializeField]
	[UsedImplicitly]
	private ChangeType m_Type;

	[SerializeField]
	[UsedImplicitly]
	[ShowIf("ToCustom")]
	private BlueprintFactionReference m_Faction;

	[SerializeField]
	[UsedImplicitly]
	private bool m_AllowDirectControl;

	private bool ToCustom => m_Type == ChangeType.ToCustom;

	protected override void OnActivate()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		componentData.OriginalFaction = base.Owner.Faction.Blueprint;
		componentData.OriginalGroupId = base.Owner.CombatGroup.Id;
		componentData.OriginalAttackFactions = base.Owner.Faction.AttackFactions.ToList();
		BlueprintFaction faction;
		string id;
		switch (m_Type)
		{
		case ChangeType.ToNeutrals:
			faction = BlueprintRoot.Instance.SystemMechanics.FactionNeutrals;
			id = base.Owner.UniqueId;
			break;
		case ChangeType.ToCaster:
		{
			MechanicEntity mechanicEntity = base.Fact.MaybeContext?.MaybeCaster;
			if (mechanicEntity == null)
			{
				PFLog.Default.Error("Caster is missing");
				return;
			}
			BlueprintFaction blueprintFaction = mechanicEntity.GetFactionOptional()?.Blueprint;
			if (blueprintFaction != null)
			{
				string text = mechanicEntity.GetCombatGroupOptional()?.Id;
				if (text != null)
				{
					faction = blueprintFaction;
					id = text;
					break;
				}
			}
			PFLog.Default.Error("Caster's faction or combat group is missing");
			return;
		}
		case ChangeType.ToCustom:
			faction = m_Faction.Get();
			id = base.Owner.UniqueId;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.Owner.CombatGroup.Id = id;
		base.Owner.Faction.Set(faction);
		base.Owner.CombatGroup.ResetFactionSet();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitFactionHandler>)delegate(IUnitFactionHandler h)
		{
			h.HandleFactionChanged();
		}, isCheckRuntime: true);
		base.Owner.Commands.InterruptAllInterruptible();
	}

	protected override void OnActivateOrPostLoad()
	{
		if (!m_AllowDirectControl)
		{
			base.Owner.PreventDirectControl.Retain();
		}
	}

	protected override void OnDeactivate()
	{
		if (!m_AllowDirectControl)
		{
			base.Owner.PreventDirectControl.Release();
		}
		ComponentData componentData = RequestSavableData<ComponentData>();
		base.Owner.CombatGroup.Id = componentData.OriginalGroupId;
		base.Owner.Faction.Set(componentData.OriginalFaction);
		foreach (BlueprintFaction originalAttackFaction in componentData.OriginalAttackFactions)
		{
			base.Owner.Faction.AttackFactions.Add(originalAttackFaction);
		}
		base.Owner.CombatGroup.ResetFactionSet();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitFactionHandler>)delegate(IUnitFactionHandler h)
		{
			h.HandleFactionChanged();
		}, isCheckRuntime: true);
		base.Owner.Commands.InterruptAllInterruptible();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
