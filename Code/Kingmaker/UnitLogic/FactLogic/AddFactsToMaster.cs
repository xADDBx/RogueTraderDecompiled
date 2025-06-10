using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("9a8a0eefc6594af8a645c2d7b226385f")]
public class AddFactsToMaster : UnitFactComponentDelegate, IDifficultyChangedClassHandler, ISubscriber, IHashable
{
	[NotNull]
	[SerializeField]
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
		BaseUnitEntity master = base.Owner.Master;
		if (master == null)
		{
			return;
		}
		if (base.IsReapplying)
		{
			EntityFact[] array = master.Facts.FindAllBySource(base.Fact, this).ToArray();
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
		BaseUnitEntity master = base.Owner.Master;
		if (master != null)
		{
			EntityFact[] array = master.Facts.FindAllBySource(base.Fact, this).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.Reapply();
			}
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
		BaseUnitEntity master = base.Owner.Master;
		if (master == null)
		{
			return;
		}
		List<BlueprintUnitFact> list = TempList.Get<BlueprintUnitFact>();
		foreach (BlueprintUnitFact fact in Facts)
		{
			if (master.Facts.FindBySource(fact, base.Fact, this) == null)
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
		BaseUnitEntity master = base.Owner.Master;
		if (master == null)
		{
			return;
		}
		List<UnitFact> list = TempList.Get<UnitFact>();
		foreach (EntityFact item in master.Facts.FindAllBySource(base.Fact, this))
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
				if (master.Facts.FindBySource(fact, base.Fact, this) == null)
				{
					list2.Add(fact);
				}
			}
		}
		foreach (UnitFact item2 in list)
		{
			master.Facts.Remove(item2);
		}
		foreach (BlueprintUnitFact item3 in list2)
		{
			TryAddFact(item3);
		}
	}

	private void TryAddFact(BlueprintUnitFact blueprintFact)
	{
		BaseUnitEntity master = base.Owner.Master;
		if (master != null)
		{
			MechanicEntityFact fact = blueprintFact.CreateFact(null, master, null);
			master.Facts.Add(fact)?.AddSource(base.Fact, this);
		}
	}

	private void Clear()
	{
		BaseUnitEntity master = base.Owner.Master;
		if (master != null)
		{
			RemoveAllFactsOriginatedFromThisComponent(master);
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
