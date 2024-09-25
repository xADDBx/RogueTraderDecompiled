using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(EntityViewBase))]
[KnowledgeDatabaseID("8c4e829910cc0d84f8a2ac26a1332cf2")]
public abstract class AbstractEntityPartComponent : MonoBehaviour, IAbstractEntityPartComponent
{
	public abstract object GetSettings();

	public abstract void EnsureEntityPart();
}
