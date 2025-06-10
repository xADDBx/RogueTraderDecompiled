using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.DialogSystem.State;

[JsonObject]
public class DialogState : IHashable
{
	[JsonProperty]
	public readonly HashSet<string> SelectedAnswers = new HashSet<string>();

	[JsonProperty]
	public readonly Dictionary<string, CheckResult> AnswerChecks = new Dictionary<string, CheckResult>();

	[JsonProperty]
	public readonly HashSet<string> ShownAnswerLists = new HashSet<string>();

	[JsonProperty]
	public readonly HashSet<string> ShownCues = new HashSet<string>();

	[JsonProperty]
	public readonly HashSet<string> ShownDialogs = new HashSet<string>();

	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintDialog, BlueprintScriptableObject>))]
	public readonly Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> BookEventLog = new Dictionary<BlueprintDialog, List<BlueprintScriptableObject>>();

	public bool SelectedAnswersContains(BlueprintAnswer bp)
	{
		return SelectedAnswers.Contains(bp.AssetGuid);
	}

	public bool SelectedAnswersAdd(BlueprintAnswer bp)
	{
		return SelectedAnswers.Add(bp.AssetGuid);
	}

	public bool SelectedAnswersRemove(BlueprintAnswer bp)
	{
		return SelectedAnswers.Remove(bp.AssetGuid);
	}

	public bool AnswerChecksContains(BlueprintAnswer bp)
	{
		return AnswerChecks.ContainsKey(bp.AssetGuid);
	}

	public bool AnswerChecksTryGetValue(BlueprintAnswer bp, out CheckResult res)
	{
		return AnswerChecks.TryGetValue(bp.AssetGuid, out res);
	}

	public CheckResult AnswerChecksGet(BlueprintAnswer bp)
	{
		return AnswerChecks[bp.AssetGuid];
	}

	public void AnswerChecksAdd(BlueprintAnswer bp, CheckResult result)
	{
		AnswerChecks.Add(bp.AssetGuid, result);
	}

	public bool ShownAnswerListsContains(BlueprintAnswersList bp)
	{
		return ShownAnswerLists.Contains(bp.AssetGuid);
	}

	public bool ShownAnswerListsAdd(BlueprintAnswersList bp)
	{
		return ShownAnswerLists.Add(bp.AssetGuid);
	}

	public bool ShownCuesContains(BlueprintCueBase bp)
	{
		return ShownCues.Contains(bp.AssetGuid);
	}

	public bool ShownCuesAdd(BlueprintCueBase bp)
	{
		return ShownCues.Add(bp.AssetGuid);
	}

	public bool ShownCuesRemove(BlueprintCueBase bp)
	{
		return ShownCues.Remove(bp.AssetGuid);
	}

	public bool ShownDialogsContains(BlueprintDialog bp)
	{
		return ShownDialogs.Contains(bp.AssetGuid);
	}

	public bool ShownDialogsAdd(BlueprintDialog bp)
	{
		return ShownDialogs.Add(bp.AssetGuid);
	}

	public bool ShownDialogsRemove(BlueprintDialog bp)
	{
		return ShownDialogs.Remove(bp.AssetGuid);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<string> selectedAnswers = SelectedAnswers;
		if (selectedAnswers != null)
		{
			int num = 0;
			foreach (string item in selectedAnswers)
			{
				num ^= StringHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		Dictionary<string, CheckResult> answerChecks = AnswerChecks;
		if (answerChecks != null)
		{
			int val = 0;
			foreach (KeyValuePair<string, CheckResult> item2 in answerChecks)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = StringHasher.GetHash128(item2.Key);
				hash.Append(ref val2);
				CheckResult obj = item2.Value;
				Hash128 val3 = UnmanagedHasher<CheckResult>.GetHash128(ref obj);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		HashSet<string> shownAnswerLists = ShownAnswerLists;
		if (shownAnswerLists != null)
		{
			int num2 = 0;
			foreach (string item3 in shownAnswerLists)
			{
				num2 ^= StringHasher.GetHash128(item3).GetHashCode();
			}
			result.Append(num2);
		}
		HashSet<string> shownCues = ShownCues;
		if (shownCues != null)
		{
			int num3 = 0;
			foreach (string item4 in shownCues)
			{
				num3 ^= StringHasher.GetHash128(item4).GetHashCode();
			}
			result.Append(num3);
		}
		HashSet<string> shownDialogs = ShownDialogs;
		if (shownDialogs != null)
		{
			int num4 = 0;
			foreach (string item5 in shownDialogs)
			{
				num4 ^= StringHasher.GetHash128(item5).GetHashCode();
			}
			result.Append(num4);
		}
		Hash128 val4 = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintDialog, BlueprintScriptableObject>.GetHash128(BookEventLog);
		result.Append(ref val4);
		return result;
	}
}
