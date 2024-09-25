using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class Feature : UnitFact<BlueprintFeature>, IFactWithRanks, IHashable
{
	public class ParamContextData : ContextData<ParamContextData>
	{
		public FeatureParam Param { get; private set; }

		public ParamContextData Setup(FeatureParam param)
		{
			Param = param;
			return this;
		}

		protected override void Reset()
		{
			Param = null;
		}
	}

	public class Data : ContextData<Data>
	{
		public Feature Feature { get; private set; }

		public Data Setup(Feature feature)
		{
			Feature = feature;
			return this;
		}

		protected override void Reset()
		{
			Feature = null;
		}
	}

	[JsonProperty]
	private MechanicsContext m_Context;

	[JsonProperty]
	public int Rank { get; private set; }

	[JsonProperty]
	public bool IsTemporary { get; set; }

	[JsonProperty(IsReference = false)]
	public FeatureParam Param { get; set; }

	[JsonProperty]
	public bool IgnorePrerequisites { get; set; } = true;


	[JsonProperty]
	public bool DisabledBecauseOfPrerequisites { get; set; }

	public MechanicsContext Context => m_Context;

	public override MechanicsContext MaybeContext => Context;

	[CanBeNull]
	public EntityFactSource SourceWithMaxLevel
	{
		get
		{
			int? num = null;
			EntityFactSource result = null;
			foreach (EntityFactSource source in base.Sources)
			{
				if (!num.HasValue || source.PathRank > num)
				{
					num = source.PathRank;
					result = source;
				}
			}
			return result;
		}
	}

	public int SourceLevel => (SourceWithMaxLevel?.PathRank).GetValueOrDefault();

	[CanBeNull]
	public BlueprintProgression SourceProgression => SourceWithMaxLevel?.Blueprint as BlueprintProgression;

	[CanBeNull]
	public BlueprintRace SourceRace => SourceWithMaxLevel?.Blueprint as BlueprintRace;

	[CanBeNull]
	public BlueprintCharacterClass SourceClass => SourceWithMaxLevel?.Blueprint as BlueprintCharacterClass;

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled)
			{
				return !DisabledBecauseOfPrerequisites;
			}
			return false;
		}
	}

	public override bool Hidden
	{
		get
		{
			if (!base.Hidden)
			{
				return base.Blueprint.HideInUI;
			}
			return true;
		}
	}

	public override string Name
	{
		get
		{
			string text = (base.Name.IsNullOrEmpty() ? GetNameFromItemSource() : base.Name);
			if (Param == null)
			{
				return text;
			}
			LocalizedTexts instance = LocalizedTexts.Instance;
			FeatureParam param = Param;
			if (param.WeaponCategory.HasValue)
			{
				return text + " (" + instance.Stats.GetText(param.WeaponCategory.Value) + ")";
			}
			if (param.StatType.HasValue)
			{
				return text + " (" + instance.Stats.GetText(param.StatType.Value) + ")";
			}
			if (param.Blueprint is IUIDataProvider iUIDataProvider)
			{
				return text + " (" + iUIDataProvider.Name + ")";
			}
			return text;
		}
	}

	private string GetNameFromItemSource()
	{
		EntityFactSource entityFactSource = base.Sources.FirstOrDefault((EntityFactSource source) => source.Entity is ItemEntity);
		if (!(entityFactSource != null))
		{
			return "";
		}
		return ((ItemEntity)entityFactSource.Entity).Name;
	}

	public Feature(BlueprintFeature blueprint, BaseUnitEntity owner, MechanicsContext parentContext = null)
		: base(blueprint)
	{
		Rank = 1;
		m_Context = parentContext?.CloneFor(blueprint, owner) ?? new MechanicsContext(null, owner, blueprint);
		Param = ContextData<ParamContextData>.Current?.Param ?? new FeatureParam();
	}

	public Feature(JsonConstructorMark _)
	{
	}

	public override int GetRank()
	{
		return Rank;
	}

	public void AddRank(int count = 1)
	{
		if (count <= 0)
		{
			return;
		}
		try
		{
			m_IsReapplying.Retain();
			bool isActive = base.IsActive;
			if (isActive)
			{
				Deactivate();
			}
			Rank += count;
			if (isActive)
			{
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
	}

	public void RemoveRank(int count = 1)
	{
		if (Rank < 1)
		{
			PFLog.Default.ErrorWithReport($"Can't remove rank from feature {base.Blueprint} (current rank {Rank})");
			return;
		}
		try
		{
			m_IsReapplying.Retain();
			bool isActive = base.IsActive;
			if (isActive)
			{
				Deactivate();
			}
			int prevRank = Rank;
			Rank = Math.Max(Rank - count, 0);
			EntityFactSource entityFactSource = base.Sources.FirstItem((EntityFactSource i) => i.FeatureRank == prevRank);
			if (entityFactSource != null)
			{
				RemoveSource(entityFactSource);
			}
			if (isActive)
			{
				Activate();
			}
		}
		finally
		{
			m_IsReapplying.Release();
		}
	}

	protected override void OnDispose()
	{
		m_Context = null;
	}

	public override void RunActionInContext(ActionList action, ITargetWrapper target = null)
	{
		using (ContextData<Data>.Request().Setup(this))
		{
			base.RunActionInContext(action, target);
		}
	}

	[CanBeNull]
	public BlueprintCharacterClass GetSourceClass()
	{
		Feature feature = this;
		while (feature != null)
		{
			BlueprintCharacterClass sourceClass = feature.SourceClass;
			if (sourceClass != null)
			{
				return sourceClass;
			}
			BlueprintFeature sourceFeature = SourceWithMaxLevel?.Blueprint as BlueprintFeature;
			if (sourceFeature == null)
			{
				return null;
			}
			feature = base.Owner.Facts.Get((Feature i) => i.Blueprint == sourceFeature);
		}
		return null;
	}

	protected override bool SupportsMultipleSources()
	{
		return true;
	}

	[Obsolete]
	public void AddSource(FeatureSource source, int pathRank)
	{
		EntityFactSource entityFactSource = base.Sources.FirstItem((EntityFactSource i) => i.Blueprint == source.Blueprint && i.PathRank > pathRank);
		if (entityFactSource?.Blueprint != null)
		{
			PFLog.Default.ErrorWithReport($"Feature.AddSource: source {source.Blueprint} has level {entityFactSource.PathRank}," + $" but new level {pathRank} is lower");
		}
		AddSource(source.Blueprint, pathRank);
	}

	public void AddSource(BlueprintPath path, BlueprintScriptableObject pathFeatureSource, int pathRank)
	{
		if (base.Sources.FirstItem((EntityFactSource i) => i.Blueprint == path && i.FeatureRank >= Rank)?.Blueprint != null)
		{
			PFLog.EntityFact.ErrorWithReport("Feature.AddSource: conflictingSource.FeatureRank >= Rank");
		}
		AddSource(new EntityFactSource(path, pathFeatureSource, pathRank, Rank));
	}

	public void SetSamePathSource(Feature feature)
	{
		foreach (EntityFactSource source in feature.Sources)
		{
			if (source.Path != null && source.PathRank.HasValue && source.FeatureRank.HasValue)
			{
				AddSource(new EntityFactSource(source.Path, source.PathFeatureSource, source.PathRank.Value, source.FeatureRank.Value));
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val2);
		int val3 = Rank;
		result.Append(ref val3);
		bool val4 = IsTemporary;
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<FeatureParam>.GetHash128(Param);
		result.Append(ref val5);
		bool val6 = IgnorePrerequisites;
		result.Append(ref val6);
		bool val7 = DisabledBecauseOfPrerequisites;
		result.Append(ref val7);
		return result;
	}
}
