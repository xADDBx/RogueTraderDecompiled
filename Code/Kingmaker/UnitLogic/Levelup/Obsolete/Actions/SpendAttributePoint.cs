using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SpendAttributePoint : ILevelUpAction, IHashable
{
	[JsonProperty]
	public StatType Attribute;

	public LevelUpActionPriority Priority => LevelUpActionPriority.AddAttribute;

	[JsonConstructor]
	public SpendAttributePoint()
	{
	}

	public SpendAttributePoint(StatType attribute)
	{
		Attribute = attribute;
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
		result.Append(ref Attribute);
		return result;
	}
}
