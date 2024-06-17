using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Newtonsoft.Json.Linq;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public abstract class JsonUpgraderBase : IJsonUpgrader
{
	protected JObject Root;

	public virtual bool NeedsPlayerPriorityLoad => false;

	public abstract void Upgrade();

	public virtual bool WillUpgrade(string jsonName)
	{
		return true;
	}

	public void Upgrade(JObject root)
	{
		Root = root;
		try
		{
			Upgrade();
		}
		finally
		{
			Root = null;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void CheckGuid(string s)
	{
		if (s.Length != 32)
		{
			throw new Exception(s + " does not look like a GUID!");
		}
		if (!Regex.IsMatch(s, "[0-9a-f]{32}"))
		{
			throw new Exception(s + " does not look like a GUID!");
		}
	}

	protected bool IsInArea(string guid)
	{
		return Root.SelectToken("..CurrentArea").Value<string>() == guid;
	}

	protected bool CueSeen(string guid)
	{
		return ArrayContains(Root.SelectToken("..m_Dialog.ShownCues") as JArray, guid);
	}

	protected bool AnswerSelected(string guid)
	{
		return ArrayContains(Root.SelectToken("..m_Dialog.SelectedAnswers") as JArray, guid);
	}

	protected bool DialogSeen(string guid)
	{
		return ArrayContains(Root.SelectToken("..m_Dialog.ShownDialogs") as JArray, guid);
	}

	protected bool HasQuest(string guid, QuestState state = QuestState.None)
	{
		if (Root.SelectToken("..PersistentQuests") is JArray jArray)
		{
			foreach (JObject item in jArray.Children<JObject>())
			{
				if (item != null && item["Blueprint"]?.Value<string>() == guid)
				{
					return state == QuestState.None || item["m_State"].Value<string>() == state.ToString();
				}
			}
		}
		return false;
	}

	protected bool HasObjective(string guid, QuestObjectiveState state = QuestObjectiveState.None)
	{
		foreach (JToken item in Root.SelectTokens("..PersistentQuests[*].PersistentObjectives[*]"))
		{
			if (item["Blueprint"]?.Value<string>() == guid)
			{
				return state == QuestObjectiveState.None || item["m_State"].Value<string>() == state.ToString();
			}
		}
		return false;
	}

	protected void SetQuestStateSilently(string guid, QuestState state)
	{
		foreach (JToken item in Root.SelectTokens("..PersistentQuests[*]"))
		{
			if (item["Blueprint"]?.Value<string>() == guid)
			{
				item["m_State"] = state.ToString();
			}
		}
	}

	protected void SetObjectiveStateSilently(string guid, QuestObjectiveState state)
	{
		foreach (JToken item in Root.SelectTokens("..PersistentQuests[*].PersistentObjectives[*]"))
		{
			if (item["Blueprint"]?.Value<string>() == guid)
			{
				item["m_State"] = state.ToString();
			}
		}
	}

	protected void UnlockFlag(string guid, int flagValue = 0, bool ignoreIfPresent = true)
	{
		if (!(Root.SelectToken("..m_UnlockableFlags.m_UnlockedFlags") is JArray jArray))
		{
			return;
		}
		JObject jObject = jArray.OfType<JObject>().FirstOrDefault((JObject f) => f.Value<string>("Key") == guid);
		if (jObject != null)
		{
			if (!ignoreIfPresent)
			{
				jObject["Value"] = flagValue;
			}
		}
		else
		{
			jObject = new JObject
			{
				["Key"] = guid,
				["Value"] = flagValue
			};
			jArray.Add(jObject);
		}
	}

	protected void LockFlag(string guid)
	{
		if (Root.SelectToken("..m_UnlockableFlags.m_UnlockedFlags") is JArray source)
		{
			source.OfType<JObject>().FirstOrDefault((JObject f) => f.Value<string>("Key") == guid)?.Remove();
		}
	}

	protected bool IsFlagUnlocked(string guid)
	{
		if (!(Root.SelectToken("..m_UnlockableFlags.m_UnlockedFlags") is JArray source))
		{
			return false;
		}
		return source.OfType<JObject>().Any((JObject f) => f.Value<string>("Key") == guid);
	}

	protected bool IsFlagUnlockedWithValue(string guid, int value)
	{
		if (!(Root.SelectToken("..m_UnlockableFlags.m_UnlockedFlags") is JArray source))
		{
			return false;
		}
		return source.OfType<JObject>().FirstOrDefault((JObject f) => f.Value<string>("Key") == guid)?.Value<string>("Value") == value.ToString();
	}

	protected int? GetFlagValue(string guid)
	{
		if (!(Root.SelectToken("..m_UnlockableFlags.m_UnlockedFlags") is JArray source))
		{
			return null;
		}
		string text = source.OfType<JObject>().FirstOrDefault((JObject f) => f.Value<string>("Key") == guid)?.Value<string>("Value");
		if (text == null || !int.TryParse(text, out var result))
		{
			return null;
		}
		return result;
	}

	protected void UnlockArtisan(string guid, string regionGuid, bool unlocked, int tierCount)
	{
		if (!(Root.SelectToken("..Kingdom.Regions") is JArray source))
		{
			return;
		}
		JObject jObject = source.OfType<JObject>().FirstOrDefault((JObject f) => f.Value<string>("Blueprint") == regionGuid);
		if (jObject != null && jObject["Artisans"] is JArray jArray && !jArray.OfType<JObject>().Any((JObject t) => t.Value<string>("Blueprint") == guid))
		{
			JObject jObject2 = new JObject
			{
				["Blueprint"] = guid,
				["m_Region"] = regionGuid,
				["BuildingUnlocked"] = unlocked,
				["TiersUnlocked"] = new JArray()
			};
			while (tierCount-- > 0)
			{
				((JArray)jObject2["TiersUnlocked"]).Add(false);
			}
			jArray.Add(jObject2);
		}
	}

	protected void UnlockCompanionStory(string companionGuid, string storyGuid)
	{
		if (!(Root.SelectToken("..CompanionStories.m_Stories") is JArray jArray))
		{
			return;
		}
		foreach (JToken item2 in jArray)
		{
			if (item2["Key"].Value<string>() == companionGuid)
			{
				if (!(item2["Value"] is JArray jArray2))
				{
					throw new Exception("could not add story " + companionGuid + ", " + storyGuid);
				}
				if (jArray2.All((JToken s) => s.Value<string>() != storyGuid))
				{
					jArray2.Add(storyGuid);
				}
				return;
			}
		}
		JObject item = new JObject
		{
			["Key"] = companionGuid,
			["Value"] = new JArray(storyGuid)
		};
		jArray.Add(item);
	}

	protected bool IsPlayerJson()
	{
		return Root.SelectToken(".CrossSceneState") != null;
	}

	protected bool IsKingdomExists()
	{
		return Root.SelectTokens(".Kingdom.Regions[*]").Any();
	}

	protected bool HasTimelineEvent(string guid, bool? triggered)
	{
		if (Root.SelectToken("..m_TimelineHistory") is JArray jArray)
		{
			foreach (JToken item in jArray.Children())
			{
				if (item is JObject jObject && jObject["EventId"].Value<string>() == guid && (!triggered.HasValue || jObject["DidTrigger"].Value<bool>() == triggered))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected List<JObject> GetAllObjectsOfType(Type type)
	{
		return Root.SelectTokens("$..*[?(@.$type == '" + type.AssemblyQualifiedName + "')]").Cast<JObject>().ToList();
	}

	protected List<JObject> GetAllKingdomEvents()
	{
		return Root.SelectTokens("$..*[?(@.EventBlueprint && @.m_RecurrentChanges)]").Cast<JObject>().ToList();
	}

	protected bool HasActiveKingdomEvent(string guid)
	{
		List<JObject> allKingdomEvents = GetAllKingdomEvents();
		if (allKingdomEvents != null)
		{
			foreach (JObject item in allKingdomEvents)
			{
				JToken jToken = item?.GetValue("EventBlueprint");
				if (jToken != null && jToken.Value<string>() == guid)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected JObject GetProjectHistoryEntry(string guid)
	{
		if (Root.SelectToken("$..Kingdom.EventHistory") is JArray jArray)
		{
			foreach (JObject item in jArray)
			{
				JToken jToken = item?.GetValue("Event");
				if (jToken != null && jToken.Value<string>() == guid)
				{
					return item;
				}
			}
		}
		return null;
	}

	protected bool HasProjectDone(string guid)
	{
		return GetProjectHistoryEntry(guid) != null;
	}

	protected void RemoveActiveKingdomEvent(string guid, bool all = false)
	{
		AddUpgradeAction(all ? PlayerUpgradeActionType.RemoveKingdomEventAll : PlayerUpgradeActionType.RemoveKingdomEvent, guid);
	}

	protected IEnumerable<JObject> GetSceneStates()
	{
		foreach (JToken item in Root.SelectTokens("..SceneName"))
		{
			JContainer parent = item.Parent;
			while (parent != null && !(parent is JObject))
			{
				parent = parent.Parent;
			}
			if (parent != null && parent["SceneName"] != null && parent["HasEntityData"] != null && parent["m_EntityData"] != null)
			{
				yield return parent as JObject;
			}
		}
	}

	protected void RemoveTypeMarkers(string type)
	{
		foreach (JToken item in Root.SelectTokens("..$type").ToList())
		{
			if ((item as JValue)?.Value is string text && text.Contains(type))
			{
				item.Parent.Remove();
			}
		}
	}

	protected void AddUpgradeAction(PlayerUpgradeActionType type, string blueprint)
	{
		if (IsPlayerJson())
		{
			JArray jArray = Root["UpgradeActions"] as JArray;
			if (jArray == null)
			{
				jArray = new JArray();
				Root["UpgradeActions"] = jArray;
			}
			JObject item = new JObject
			{
				["Type"] = type.ToString(),
				["Blueprint"] = blueprint
			};
			jArray.Add(item);
		}
	}

	protected void RecreateUnit(string blueprint)
	{
		foreach (JToken item in Root.SelectTokens("..Blueprint").ToList())
		{
			JContainer jContainer = item?.Parent?.Parent;
			if (jContainer != null && blueprint.Equals(item.Value<string>()))
			{
				jContainer["Recreate"] = true;
			}
		}
	}

	protected bool IsArea(string areaBlueprint)
	{
		JToken jToken = Root.SelectToken("$.m_Area.Blueprint");
		if (jToken == null)
		{
			return false;
		}
		return areaBlueprint == jToken.Value<string>();
	}

	protected JObject Dereference(JObject o)
	{
		if (o?["$ref"] != null)
		{
			o = Root.SelectToken("$..*[?(@.$id == '" + o.Value<string>("$ref") + "')]") as JObject;
		}
		return o;
	}

	protected void DestroySpawnerUnit(string spawnerId)
	{
		List<JToken> list = Root.SelectTokens("..UniqueId").ToList();
		foreach (JToken item in list)
		{
			if (item == null || spawnerId != item.Value<string>())
			{
				continue;
			}
			string text = (item?.Parent?.Parent)?["m_SpawnedUnit"]?["m_UniqueId"]?.Value<string>();
			if (text == null)
			{
				continue;
			}
			foreach (JToken item2 in list)
			{
				if (item2 != null && !(text != item2.Value<string>()))
				{
					JContainer jContainer = item2?.Parent?.Parent;
					if (jContainer != null)
					{
						jContainer["ShouldBeDestroyed"] = true;
					}
				}
			}
		}
	}

	protected IEnumerable<JObject> GetAllUnitDescriptors()
	{
		JArray jArray = (Root["m_EntityData"] as JArray) ?? (Root.SelectToken("..m_MainState.m_EntityData") as JArray);
		if (jArray == null)
		{
			yield break;
		}
		foreach (JToken item in jArray)
		{
			if (!(item is JObject o))
			{
				continue;
			}
			JObject jObject = Dereference(o);
			if (!(jObject?["$type"]?.Value<string>() != "Kingmaker.EntitySystem.Entities.UnitEntityData, Code"))
			{
				JObject o2 = jObject["Descriptor"] as JObject;
				o2 = Dereference(o2);
				if (o2 != null)
				{
					yield return o2;
				}
			}
		}
	}

	protected void RestartCutscene(string blueprint)
	{
		foreach (JToken item in Root.SelectTokens("..m_Cutscene").ToList())
		{
			JContainer jContainer = item?.Parent?.Parent;
			if (jContainer != null && blueprint.Equals(item.Value<string>()))
			{
				jContainer["m_Restart"] = true;
			}
		}
	}

	protected void RemoveCutscene(string blueprint)
	{
		foreach (JToken item in Root.SelectTokens("..m_Cutscene").ToList())
		{
			JContainer jContainer = item?.Parent?.Parent;
			if (jContainer != null && blueprint.Equals(item.Value<string>()))
			{
				jContainer["m_Remove"] = true;
			}
		}
	}

	[CanBeNull]
	protected JObject GetComponentData([NotNull] string componentName)
	{
		foreach (JToken item in Root.SelectTokens("..ComponentName").ToList())
		{
			if (item?.Value<string>() == componentName)
			{
				return item?.Parent?.Parent as JObject;
			}
		}
		return null;
	}

	protected static bool ArrayContains([CanBeNull] JArray array, [NotNull] string value)
	{
		if (array == null)
		{
			return false;
		}
		for (int i = 0; i < array.Count; i++)
		{
			if (array[i].ToObject<string>() == value)
			{
				return true;
			}
		}
		return false;
	}
}
