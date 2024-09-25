using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

public class CompanionStoriesManager : IHashable
{
	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintUnit, BlueprintCompanionStory>))]
	private readonly Dictionary<BlueprintUnit, List<BlueprintCompanionStory>> m_Stories = new Dictionary<BlueprintUnit, List<BlueprintCompanionStory>>();

	public IEnumerable<BlueprintCompanionStory> Get(BaseUnitEntity character)
	{
		if (!BlueprintRoot.Instance.CharGenRoot.CustomCompanions.Any((BlueprintUnitReference r) => r.Is(character.Blueprint)))
		{
			return Get(character.Blueprint);
		}
		return from r in BlueprintRoot.Instance.CharGenRoot.CustomCompanionStories
			select r.Get() into st
			where st.Gender == character.Gender && !st.IsDlcRestricted()
			select st;
	}

	private IEnumerable<BlueprintCompanionStory> Get(BlueprintUnit character)
	{
		m_Stories.TryGetValue(character, out var value);
		return value.EmptyIfNull();
	}

	public bool IsUnlocked(BlueprintCompanionStory companionStory)
	{
		foreach (KeyValuePair<BlueprintUnit, List<BlueprintCompanionStory>> story in m_Stories)
		{
			if (story.Value.Contains(companionStory))
			{
				return true;
			}
		}
		return false;
	}

	public void Unlock(BlueprintCompanionStory story)
	{
		if (!m_Stories.TryGetValue(story.Companion, out var value))
		{
			value = new List<BlueprintCompanionStory>();
			m_Stories.Add(story.Companion, value);
		}
		else if (value.Contains(story))
		{
			return;
		}
		value.Add(story);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintUnit, BlueprintCompanionStory>.GetHash128(m_Stories);
		result.Append(ref val);
		return result;
	}
}
