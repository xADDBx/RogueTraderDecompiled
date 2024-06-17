using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Formations;

public interface IPartyFormation : IImmutablePartyFormation
{
	void SetOffset(int index, AbstractUnitEntity unit, Vector2 pos);
}
