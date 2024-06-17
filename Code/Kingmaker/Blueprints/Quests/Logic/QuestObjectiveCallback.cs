using System.Text;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[TypeId("c3f36a2e95828334e9f701ba86c781ba")]
public class QuestObjectiveCallback : QuestObjectiveComponentDelegate, IQuestObjectiveCallback, IHashable
{
	[SerializeField]
	private ActionList m_OnComplete;

	[SerializeField]
	private ActionList m_OnFail;

	void IQuestObjectiveCallback.OnComplete()
	{
		m_OnComplete.Run();
	}

	void IQuestObjectiveCallback.OnFail()
	{
		m_OnFail.Run();
	}

	public override string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (m_OnComplete.HasActions)
		{
			stringBuilder.Append("On Complete ");
			stringBuilder.Append(ElementsDescription.Actions(true, m_OnComplete));
		}
		if (m_OnFail.HasActions)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append("\n");
			}
			stringBuilder.Append("On Fail ");
			stringBuilder.Append(ElementsDescription.Actions(true, m_OnFail));
		}
		return stringBuilder.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
