using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Formations;

public interface IImmutablePartyFormation
{
	float Length { get; }

	[CanBeNull]
	AbstractUnitEntity Tank { get; }

	Vector2 GetOffset(int index, AbstractUnitEntity unit);
}
