using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class UnlockRestrictionSettings
{
	[SerializeField]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();
}
