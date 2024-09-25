using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface ITargetWrapper
{
	Vector3 Point { get; }

	IMechanicEntity IEntity { get; }

	bool IsPoint { get; }
}
