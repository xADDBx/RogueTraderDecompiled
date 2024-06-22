using System;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Controllers.Dialog;

public class DialogData : IDisposable
{
	public BlueprintDialog Dialog;

	public UnitReference Initiator;

	public UnitReference Unit;

	public EntityRef<MapObjectEntity> MapObject;

	public string CustomSpeakerName;

	private bool m_IsDisposed;

	public void Dispose()
	{
		if (!m_IsDisposed)
		{
			m_IsDisposed = true;
		}
	}
}
