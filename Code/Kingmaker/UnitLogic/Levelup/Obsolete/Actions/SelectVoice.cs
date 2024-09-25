using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectVoice : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	public readonly BlueprintUnitAsksList Voice;

	public LevelUpActionPriority Priority => LevelUpActionPriority.Visual;

	[JsonConstructor]
	public SelectVoice()
	{
	}

	public SelectVoice([NotNull] BlueprintUnitAsksList voice)
	{
		Voice = voice;
	}

	public bool Check(LevelUpState state, BaseUnitEntity unit)
	{
		return false;
	}

	public void Apply(LevelUpState state, BaseUnitEntity unit)
	{
	}

	public void PostLoad()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Voice);
		result.Append(ref val);
		return result;
	}
}
