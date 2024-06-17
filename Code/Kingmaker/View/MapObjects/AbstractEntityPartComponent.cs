using Kingmaker.EntitySystem.Interfaces;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(EntityViewBase))]
public abstract class AbstractEntityPartComponent : MonoBehaviour, IAbstractEntityPartComponent
{
	public abstract object GetSettings();

	public abstract void EnsureEntityPart();
}
