using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Owlcat.Runtime.Core.Updatables;

[ExecuteInEditMode]
public abstract class UpdateableInEditorBehaviour : RegisteredBehaviour, IUpdatable
{
	public abstract void DoUpdate();
}
