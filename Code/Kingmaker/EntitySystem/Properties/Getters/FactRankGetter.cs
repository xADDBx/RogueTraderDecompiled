using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("af573f5008f24a98b3444959084f87f5")]
public class FactRankGetter : PropertyGetter, PropertyContextAccessor.IOptionalTargetByType, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public enum AggregationMode
	{
		Max,
		Sum
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	[Tooltip("Specifies the unit to count the fact ranks on")]
	public PropertyTargetType CheckTarget;

	public AggregationMode Aggregation;

	public bool BuffWithCasterFromTargetType;

	[ShowIf("BuffWithCasterFromTargetType")]
	[FormerlySerializedAs("Target")]
	[Tooltip("Only counts the buffs applied by this target")]
	public PropertyTargetType BuffCaster;

	public BlueprintUnitFact Fact => m_Fact?.Get();

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = (BuffWithCasterFromTargetType ? (" cast by " + BuffCaster.Colorized()) : "");
		return "Ranks of " + (Fact?.ToString() ?? "<null>") + " on " + CheckTarget.Colorized() + text;
	}

	protected override int GetBaseValue()
	{
		Entity entity = (BuffWithCasterFromTargetType ? this.GetTargetByType(BuffCaster) : null);
		BlueprintUnitFact fact = Fact;
		int num = 0;
		foreach (EntityFact item in this.GetTargetByType(CheckTarget).Facts.GetAll<EntityFact>())
		{
			if (item.Blueprint == fact && (!BuffWithCasterFromTargetType || item.MaybeContext?.MaybeCaster == entity))
			{
				int rank = item.GetRank();
				num = Aggregation switch
				{
					AggregationMode.Max => Math.Max(num, rank), 
					AggregationMode.Sum => num + rank, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
		}
		return num;
	}
}
