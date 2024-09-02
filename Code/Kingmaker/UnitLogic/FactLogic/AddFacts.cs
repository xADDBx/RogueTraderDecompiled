using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("d79ad4ed064aa4f43ace0e9c238fb9b9")]
public class AddFacts : UnitFactComponentDelegate, IDifficultyChangedClassHandler, ISubscriber, IHashable
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Facts")]
	private BlueprintUnitFactReference[] m_Facts = new BlueprintUnitFactReference[0];

	public BlueprintUnitReference Dummy;

	public bool DoNotRestoreMissingFacts;

	public bool HasDifficultyRequirements;

	[ShowIf("HasDifficultyRequirements")]
	[Tooltip("Разрешаем использовать факт на сложностях только меньших или равных указанной.")]
	public bool InvertDifficultyRequirements;

	[ShowIf("HasDifficultyRequirements")]
	[Tooltip("Факт применяется при сложностях большх или равных указанной.")]
	public GameDifficultyOption MinDifficulty;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	private bool IsRestrictedByDifficulty()
	{
		if (!HasDifficultyRequirements)
		{
			return false;
		}
		int num = SettingsController.Instance.DifficultyPresetsController.CurrentDifficultyCompareTo(MinDifficulty);
		if (InvertDifficultyRequirements || num >= 0)
		{
			if (InvertDifficultyRequirements)
			{
				return num > 0;
			}
			return false;
		}
		return true;
	}

	protected override void OnActivate()
	{
		if (base.IsReapplying)
		{
			EntityFact[] array = base.Owner.Facts.FindAllBySource(base.Fact, this).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Reapply();
			}
		}
		else
		{
			UpdateFacts(postLoad: false);
		}
	}

	protected override void OnDeactivate()
	{
		if (!base.IsReapplying)
		{
			Clear();
		}
	}

	protected override void OnRecalculate()
	{
		EntityFact[] array = base.Owner.Facts.FindAllBySource(base.Fact, this).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Reapply();
		}
	}

	protected override void OnPostLoad()
	{
		if (base.Fact.Active)
		{
			UpdateFacts(postLoad: true);
		}
	}

	public void HandleDifficultyChanged()
	{
		if (!HasDifficultyRequirements)
		{
			return;
		}
		if (IsRestrictedByDifficulty())
		{
			Clear();
			return;
		}
		List<BlueprintUnitFact> list = TempList.Get<BlueprintUnitFact>();
		foreach (BlueprintUnitFact fact in Facts)
		{
			if (base.Owner.Facts.FindBySource(fact, base.Fact, this) == null)
			{
				list.Add(fact);
			}
		}
		foreach (BlueprintUnitFact item in list)
		{
			TryAddFact(item);
		}
	}

	private void UpdateFacts(bool postLoad)
	{
		if (IsRestrictedByDifficulty())
		{
			Clear();
			return;
		}
		List<UnitFact> list = TempList.Get<UnitFact>();
		foreach (EntityFact item in base.Owner.Facts.FindAllBySource(base.Fact, this))
		{
			if (item is UnitFact unitFact && !Facts.HasReference(unitFact.Blueprint))
			{
				list.Add(unitFact);
			}
		}
		List<BlueprintUnitFact> list2 = TempList.Get<BlueprintUnitFact>();
		if (!postLoad || !DoNotRestoreMissingFacts)
		{
			foreach (BlueprintUnitFact fact in Facts)
			{
				if (base.Owner.Facts.FindBySource(fact, base.Fact, this) == null)
				{
					list2.Add(fact);
				}
			}
		}
		foreach (UnitFact item2 in list)
		{
			base.Owner.Facts.Remove(item2);
		}
		foreach (BlueprintUnitFact item3 in list2)
		{
			TryAddFact(item3);
		}
	}

	private void TryAddFact(BlueprintUnitFact blueprintFact)
	{
		MechanicEntityFact fact = blueprintFact.CreateFact(null, base.Owner, null);
		base.Owner.Facts.Add(fact)?.AddSource(base.Fact, this);
	}

	private void Clear()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public void RemoveAllFacts()
	{
		m_Facts = Array.Empty<BlueprintUnitFactReference>();
	}

	public void AddFact(BlueprintUnitFact blueprintFact)
	{
		if (blueprintFact != null)
		{
			Array.Resize(ref m_Facts, m_Facts.Length + 1);
			m_Facts[^1] = blueprintFact.ToReference<BlueprintUnitFactReference>();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
