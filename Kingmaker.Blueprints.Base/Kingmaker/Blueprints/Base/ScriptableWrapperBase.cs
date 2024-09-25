using UnityEngine;

namespace Kingmaker.Blueprints.Base;

public abstract class ScriptableWrapperBase : ScriptableObject
{
	public virtual IHavePrototype PrototypeableInstance { get; }

	public abstract void SyncPropertiesWithProto();

	public abstract void SetBlueprintDirty();

	public abstract string GetOwnerBlueprintId();
}
