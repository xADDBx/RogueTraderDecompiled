using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.View;

public interface ICameraFocusTarget : IMechanicEntity, IEntity, IDisposable
{
	MechanicEntity CameraHolder { get; }

	float TimeToFocus { get; }

	void RetainCamera()
	{
		EventBus.RaiseEvent((IMechanicEntity)this, (Action<ICameraFocusTargetHandler>)delegate(ICameraFocusTargetHandler h)
		{
			h.HandleCameraRetain(CameraHolder);
		}, isCheckRuntime: true);
	}
}
