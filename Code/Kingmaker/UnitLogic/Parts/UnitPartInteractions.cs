using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Interaction;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartInteractions : EntityPart<AbstractUnitEntity>, IHashable
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitPartInteractions");

	[NotNull]
	private readonly List<IUnitInteraction> m_Interactions = new List<IUnitInteraction>();

	public readonly Dictionary<BaseUnitEntity, float> Distances = new Dictionary<BaseUnitEntity, float>();

	public readonly List<float> Cooldowns = new List<float>();

	private bool m_HadMultipleClickInteractionsError;

	[NotNull]
	public IReadOnlyList<IUnitInteraction> Interactions => m_Interactions;

	public bool HasApproachInteractions { get; private set; }

	public bool HasDialogInteractions { get; private set; }

	public static void SetupBlueprintInteractions(MechanicEntity unit)
	{
		List<UnitInteractionComponent> list = unit.Blueprint.GetComponents<UnitInteractionComponent>().ToList();
		if (list.Count > 0)
		{
			UnitPartInteractions orCreate = unit.GetOrCreate<UnitPartInteractions>();
			orCreate.m_Interactions.AddRange(list);
			orCreate.Cooldowns.Clear();
			orCreate.Cooldowns.AddRange(Enumerable.Repeat(0f, orCreate.m_Interactions.Count));
			orCreate.CheckForApproachAndDialogInteractions();
		}
	}

	public void AddInteraction(IUnitInteraction interaction)
	{
		m_Interactions.Insert(0, interaction);
		Cooldowns.Insert(0, 0f);
		CheckForApproachAndDialogInteractions();
	}

	private void CheckForApproachAndDialogInteractions()
	{
		HasApproachInteractions = false;
		HasDialogInteractions = false;
		foreach (IUnitInteraction interaction in m_Interactions)
		{
			if (interaction.IsApproach)
			{
				HasApproachInteractions = true;
			}
			if (interaction is DialogOnClick || (interaction is SpawnerInteractionPart.Wrapper { Source: SpawnerInteractionDialog source } && source != null))
			{
				HasDialogInteractions = true;
			}
		}
	}

	public void UpdateCooldowns()
	{
		for (int i = 0; i < Cooldowns.Count; i++)
		{
			Cooldowns[i] = Math.Max(0f, Cooldowns[i] - Game.Instance.TimeController.DeltaTime);
		}
	}

	[CanBeNull]
	public IUnitInteraction SelectClickInteraction(BaseUnitEntity initiator)
	{
		List<IUnitInteraction> list = TempList.Get<IUnitInteraction>();
		foreach (IUnitInteraction interaction in m_Interactions)
		{
			if (!interaction.IsApproach && interaction.IsAvailable(initiator, base.Owner))
			{
				list.Add(interaction);
			}
		}
		if (list.Count > 1 && !m_HadMultipleClickInteractionsError)
		{
			Logger.Error("Multiple click interactions with object {0}, selecting first available", base.Owner);
			m_HadMultipleClickInteractionsError = true;
		}
		return list.FirstOrDefault();
	}

	public void RemoveInteraction(IUnitInteraction interaction)
	{
		int num = m_Interactions.IndexOf(interaction);
		if (num != -1)
		{
			m_Interactions.RemoveAt(num);
			Cooldowns.RemoveAt(num);
		}
	}

	public void RemoveInteractions(Predicate<IUnitInteraction> pred)
	{
		int i;
		for (i = 0; i < m_Interactions.Count && !pred(m_Interactions[i]); i++)
		{
		}
		for (int j = i + 1; j < m_Interactions.Count; j++)
		{
			IUnitInteraction unitInteraction = m_Interactions[j];
			if (!pred(unitInteraction))
			{
				Cooldowns[i] = Cooldowns[j];
				m_Interactions[i] = unitInteraction;
				i++;
			}
		}
		m_Interactions.RemoveRange(i, m_Interactions.Count - i);
		Cooldowns.RemoveRange(i, m_Interactions.Count - i);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
