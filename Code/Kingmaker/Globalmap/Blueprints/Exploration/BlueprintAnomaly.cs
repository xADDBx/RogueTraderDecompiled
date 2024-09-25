using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("69c5e318f05e41ea839a8e8f0e0ff91e")]
public class BlueprintAnomaly : BlueprintAnomalyFact
{
	public enum AnomalyInteractTime
	{
		OnTouch,
		ByClick,
		OnDistance
	}

	public enum AnomalyObjectType
	{
		Default,
		ShipSignature,
		Enemy,
		Gas,
		WarpHton,
		Loot
	}

	[Serializable]
	public new class Reference : BlueprintReference<BlueprintAnomaly>
	{
	}

	public AnomalyInteractTime InteractTime;

	[ShowIf("ShowInteractDistance")]
	public float InteractDistance = 10f;

	public bool RemoveAfterInteraction;

	public bool InfiniteInteraction;

	public AnomalyObjectType AnomalyType;

	public bool HideInUI;

	public bool ScriptZoneShape;

	public bool ShowOnGlobalMap;

	private bool ShowInteractDistance
	{
		get
		{
			if (IsInteractOnDistance)
			{
				return !ScriptZoneShape;
			}
			return false;
		}
	}

	private bool IsInteractOnDistance => InteractTime == AnomalyInteractTime.OnDistance;
}
