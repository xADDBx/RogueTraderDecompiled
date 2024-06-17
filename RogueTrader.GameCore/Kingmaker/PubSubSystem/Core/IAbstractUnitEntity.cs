using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.EntitySystem.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem.Core;

public interface IAbstractUnitEntity : IMechanicEntity, IEntity, IDisposable
{
	bool IsExtra { get; }

	new string UniqueId { get; }

	string CharacterName { get; }

	new Vector3 Position { get; set; }

	Transform ViewTransform { get; }

	Gender Gender { get; }
}
