using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("4e4234bc142d4acc81469dc2ada73f33")]
public class DialogDebugRoot : BlueprintScriptableObject
{
	[Serializable]
	public class ForcedConditionsEntry
	{
		public BlueprintScriptableObjectReference Blueprint;

		public ForcedConditionsState State;
	}

	public class DialogDebugReference : BlueprintReference<DialogDebugRoot>
	{
		public DialogDebugReference()
		{
			guid = "6e39882fd2224c859127dcc94789bc47";
		}
	}

	[SerializeField]
	private List<ForcedConditionsEntry> m_ForcedConditionNodes = new List<ForcedConditionsEntry>();

	private static readonly DialogDebugReference s_Instance = new DialogDebugReference();

	public static DialogDebugRoot Instance => s_Instance;

	public void SetForcedCondition(BlueprintScriptableObject bp, ForcedConditionsState state)
	{
	}

	public ForcedConditionsState GetForcedCondition(BlueprintScriptableObject bp)
	{
		return ForcedConditionsState.NotForced;
	}

	public void ClearForcedConditions()
	{
		m_ForcedConditionNodes.Clear();
	}
}
