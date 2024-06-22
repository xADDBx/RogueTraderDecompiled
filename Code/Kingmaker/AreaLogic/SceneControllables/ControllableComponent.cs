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

	[ExecuteAlways]
	protected virtual void Awake()
	{
		Initialize();
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
	private void OnEnable()
	{
		ControllableComponentCache.All.Add(this);
	}

	[ExecuteAlways]
	private void OnDisable()
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

	public virtual void SetDefaultState()
	{
	}
}
