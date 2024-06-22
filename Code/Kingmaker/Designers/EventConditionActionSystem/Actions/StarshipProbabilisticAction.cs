using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("08e1e72d39371a242b5c5c88db0f4f75")]
public class StarshipProbabilisticAction : ContextAction
{
	[Range(0f, 1f)]
	public float probability = 1f;

	public bool modifyWithOwnerHPPercent;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float minHpPercent;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float maxHpPercent = 1f;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float probModAtMin = 1f;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float probModAtMax = 1f;

	public ActionList Actions;

	public ActionList FailActions;

	public override string GetCaption()
	{
		string text = $"{probability * 100f}% chances";
		if (modifyWithOwnerHPPercent)
		{
			text += ", modified with owner HP left,";
		}
		if (Actions.Actions.Length != 0)
		{
			text = text + " to " + Actions.Actions[0].GetCaption();
		}
		return text;
	}

	protected override void RunAction()
	{
		float num = probability;
		if (modifyWithOwnerHPPercent)
		{
			PartHealth healthOptional = base.Target.Entity.GetHealthOptional();
			if (healthOptional == null)
			{
				Element.LogError(this, "Invalid target for effect '{0}'", GetType().Name);
				return;
			}
			float num2 = 1f - (float)healthOptional.Damage / (float)healthOptional.MaxHitPoints;
			float num3 = ((num2 < minHpPercent) ? probModAtMin : ((!(num2 > maxHpPercent)) ? Rescale(num2, minHpPercent, maxHpPercent, probModAtMin, probModAtMax) : probModAtMax));
			num *= num3;
		}
		if (PFStatefulRandom.SpaceCombat.Range(0f, 1f) <= num)
		{
			Actions.Run();
		}
		else
		{
			FailActions.Run();
		}
	}

	private float Rescale(float value, float spaceSrcA, float spaceSrcB, float spaceDstA, float spaceDstB)
	{
		return (value - spaceSrcA) * (spaceDstB - spaceDstA) / (spaceSrcB - spaceSrcA) + spaceDstA;
	}
}
