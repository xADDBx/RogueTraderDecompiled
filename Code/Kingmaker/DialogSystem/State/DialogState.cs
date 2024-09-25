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
	public readonly HashSet<BlueprintAnswer> SelectedAnswers = new HashSet<BlueprintAnswer>();

	[JsonProperty]
	public readonly Dictionary<BlueprintAnswer, CheckResult> AnswerChecks = new Dictionary<BlueprintAnswer, CheckResult>();

	[JsonProperty]
	public readonly HashSet<BlueprintAnswersList> ShownAnswerLists = new HashSet<BlueprintAnswersList>();

	[JsonProperty]
	public readonly HashSet<BlueprintCueBase> ShownCues = new HashSet<BlueprintCueBase>();

	[JsonProperty]
	public readonly HashSet<BlueprintDialog> ShownDialogs = new HashSet<BlueprintDialog>();

	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintDialog, BlueprintScriptableObject>))]
	public readonly Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> BookEventLog = new Dictionary<BlueprintDialog, List<BlueprintScriptableObject>>();

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		HashSet<BlueprintAnswer> selectedAnswers = SelectedAnswers;
		if (selectedAnswers != null)
		{
			int num = 0;
			foreach (BlueprintAnswer item in selectedAnswers)
			{
				num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
			}
			result.Append(num);
		}
		Dictionary<BlueprintAnswer, CheckResult> answerChecks = AnswerChecks;
		if (answerChecks != null)
		{
			int val = 0;
			foreach (KeyValuePair<BlueprintAnswer, CheckResult> item2 in answerChecks)
			{
				Hash128 hash = default(Hash128);
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash.Append(ref val2);
				CheckResult obj = item2.Value;
				Hash128 val3 = UnmanagedHasher<CheckResult>.GetHash128(ref obj);
				hash.Append(ref val3);
				val ^= hash.GetHashCode();
			}
			result.Append(ref val);
		}
		HashSet<BlueprintAnswersList> shownAnswerLists = ShownAnswerLists;
		if (shownAnswerLists != null)
		{
			int num2 = 0;
			foreach (BlueprintAnswersList item3 in shownAnswerLists)
			{
				num2 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item3).GetHashCode();
			}
			result.Append(num2);
		}
		HashSet<BlueprintCueBase> shownCues = ShownCues;
		if (shownCues != null)
		{
			int num3 = 0;
			foreach (BlueprintCueBase item4 in shownCues)
			{
				num3 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item4).GetHashCode();
			}
			result.Append(num3);
		}
		HashSet<BlueprintDialog> shownDialogs = ShownDialogs;
		if (shownDialogs != null)
		{
			int num4 = 0;
			foreach (BlueprintDialog item5 in shownDialogs)
			{
				num4 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item5).GetHashCode();
			}
			result.Append(num4);
		}
		Hash128 val4 = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintDialog, BlueprintScriptableObject>.GetHash128(BookEventLog);
		result.Append(ref val4);
		return result;
	}
}
