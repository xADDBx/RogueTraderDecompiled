using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;
using Owlcat.Runtime.Core.Updatables;

public static class Updateables
{
	public static readonly List<Type> IUpdateables = new List<Type>
	{
		typeof(PBDColliderBase),
		typeof(PBDColliderBox),
		typeof(PBDColliderCapsule),
		typeof(PBDColliderCapsuleTransform),
		typeof(PBDColliderPlane),
		typeof(PBDColliderSphere),
		typeof(PBDPositionalColliderBase),
		typeof(UpdateableBehaviour),
		typeof(UpdateableInEditorBehaviour)
	};

	public static readonly List<Type> ILateUpdateables = new List<Type> { typeof(LateUpdateableBehaviour) };
}
