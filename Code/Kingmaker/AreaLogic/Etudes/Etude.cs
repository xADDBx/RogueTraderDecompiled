using System.Collections.Generic;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

public class Etude : EntityFact<EtudesSystem>, IHashable
{
	public readonly List<Etude> Children = new List<Etude>();

	public readonly HashSet<Etude> ComplitionBlockers = new HashSet<Etude>();

	public Etude Parent => (Etude)base.SourceFact;

	public new BlueprintEtude Blueprint => (BlueprintEtude)base.Blueprint;

	public bool IsPlaying => base.IsActive;

	[JsonProperty]
	public bool IsCompleted { get; private set; }

	[JsonProperty]
	public bool CompletionInProgress { get; private set; }

	public Etude(BlueprintEtude blueprint, [CanBeNull] Etude parent)
		: base((BlueprintFact)blueprint)
	{
		if (parent != null)
		{
			AddSource(parent);
		}
	}

	public Etude(JsonConstructorMark _)
	{
	}

	public void MarkCompleted()
	{
		if (CompletionInProgress)
		{
			return;
		}
		CompletionInProgress = true;
		foreach (Etude child in Children)
		{
			child.MarkCompleted();
		}
		GameCoreHistoryLog.Instance.EtudeEvent(null, ToString());
	}

	public void FinishCompletion()
	{
		if (!CompletionInProgress)
		{
			PFLog.Etudes.Error(Blueprint, $"Cannot complete etude {this}: complete not started");
			return;
		}
		if (IsPlaying)
		{
			PFLog.Etudes.Error(Blueprint, $"Cannot complete etude {this}: still playing");
			return;
		}
		if (IsCompleted)
		{
			PFLog.Etudes.Warning(Blueprint, $"Cannot complete etude {this}: already completed");
			return;
		}
		IsCompleted = true;
		GameCoreHistoryLog.Instance.EtudeEvent(null, ToString());
	}

	protected override void OnActivate()
	{
		base.Owner.MarkConditionsDirty();
		base.OnActivate();
		base.Owner.Facts.EnsureFactProcessor<EtudesTree>().AddToPlaying(this);
		PFLog.Etudes.Log("Etude playing: " + Blueprint.name);
		if (IsCompleted || CompletionInProgress)
		{
			PFLog.Etudes.Error(Blueprint, $"Cannot activate etude {this}: already completed");
			Deactivate();
		}
		GameCoreHistoryLog.Instance.EtudeEvent(null, ToString());
	}

	protected override void OnDeactivate()
	{
		base.Owner.MarkConditionsDirty();
		base.OnDeactivate();
		PFLog.Etudes.Log("Etude stopping: " + Blueprint.name);
		base.Owner.Facts.EnsureFactProcessor<EtudesTree>().RemoveFromPlaying(this);
		GameCoreHistoryLog.Instance.EtudeEvent(null, "Etude[" + Blueprint.NameSafe() + "]:stoping");
	}

	public override string ToString()
	{
		return "Etude[" + Blueprint.NameSafe() + "]:" + (IsCompleted ? "IsCompleted" : (CompletionInProgress ? "CompletionInProgress" : (IsPlaying ? "IsPlaying" : (base.IsAttached ? "Started" : "None"))));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsCompleted;
		result.Append(ref val2);
		bool val3 = CompletionInProgress;
		result.Append(ref val3);
		return result;
	}
}
