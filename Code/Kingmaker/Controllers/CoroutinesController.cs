using System.Collections;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.ManualCoroutines;
using UnityEngine;

namespace Kingmaker.Controllers;

public class CoroutinesController : IControllerTick, IController, IControllerReset
{
	private readonly CoroutineManager m_CoroutineManager = new CoroutineManager();

	private readonly TickType m_TickType;

	private CoroutinesController()
	{
	}

	public CoroutinesController(TickType tickType)
	{
		m_TickType = tickType;
	}

	public CoroutineHandler Start(IEnumerator routine, Object objectHolder = null)
	{
		return m_CoroutineManager.Start(routine, objectHolder);
	}

	public void Stop(IEnumerator routine)
	{
		m_CoroutineManager.Stop(routine);
	}

	public void Stop(CoroutineHandler handler)
	{
		m_CoroutineManager.Stop(handler);
	}

	public void Stop(ref CoroutineHandler handler)
	{
		m_CoroutineManager.Stop(ref handler);
	}

	public void StopAll()
	{
		m_CoroutineManager.StopAll();
	}

	TickType IControllerTick.GetTickType()
	{
		return m_TickType;
	}

	void IControllerTick.Tick()
	{
		m_CoroutineManager.Update();
	}

	void IControllerReset.OnReset()
	{
		StopAll();
	}
}
