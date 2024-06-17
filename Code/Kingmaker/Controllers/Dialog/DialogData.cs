using System;
using System.Collections.Generic;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
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

	private readonly List<IDisposable> m_ContextData = new List<IDisposable>();

	private bool m_IsDisposed;

	public T AddContextData<T>() where T : ContextData<T>, new()
	{
		T val = ContextData<T>.Request();
		m_ContextData.Add(val);
		return val;
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		foreach (IDisposable contextDatum in m_ContextData)
		{
			contextDatum.Dispose();
		}
	}
}
