using System;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[ExecuteAlways]
public abstract class ControllableComponent : MonoBehaviour, ISerializationCallbackReceiver
{
	[HideInInspector]
	public string UniqueId;

	private ControllableState m_CurrentState;

	private ControllableState m_StateAtStartUp;

	[ExecuteAlways]
	protected virtual void Awake()
	{
		Initialize();
	}

	public void GatherStateAtStartUp()
	{
		if (m_StateAtStartUp == null)
		{
			m_StateAtStartUp = new ControllableState
			{
				Active = base.gameObject.activeSelf
			};
		}
	}

	protected virtual void OnDestroy()
	{
	}

	public void Reset()
	{
		UniqueId = Guid.NewGuid().ToString();
	}

	public virtual void SetState(ControllableState state)
	{
		m_CurrentState = state;
	}

	public ControllableState GetState()
	{
		return m_CurrentState;
	}

	[ExecuteAlways]
	protected virtual void OnEnable()
	{
		ControllableComponentCache.All.Add(this);
	}

	[ExecuteAlways]
	protected virtual void OnDisable()
	{
		ControllableComponentCache.All.Remove(this);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (ControllableComponentCache.All.TryFind((ControllableComponent x) => x.UniqueId == UniqueId, out var result) && result != this)
		{
			Reset();
		}
	}

	public virtual ControllableState GetDefaultState()
	{
		return m_StateAtStartUp;
	}
}
