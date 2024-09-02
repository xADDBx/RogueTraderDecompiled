using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

public class SceneControllablesController : IControllerStop, IController, IAreaLoadingStagesHandler, ISubscriber
{
	private const string STATE_PREFIX = "scene_controllables_";

	private readonly Dictionary<string, ControllableComponent> m_Components = new Dictionary<string, ControllableComponent>();

	private SceneControllablesState m_CurrentState;

	private SceneControllablesState CurrentState
	{
		get
		{
			if (m_CurrentState == null || m_CurrentState.UniqueId != StateName)
			{
				InitState();
			}
			return m_CurrentState;
		}
		set
		{
			m_CurrentState = value;
		}
	}

	private string StateName => "scene_controllables_" + Game.Instance.LoadedAreaState.MainState.SceneName;

	public IEnumerable<ControllableComponent> AllControllables => m_Components.Values;

	public bool TryGetControllable(string idOfObject, out ControllableComponent controllableComponent)
	{
		return m_Components.TryGetValue(idOfObject, out controllableComponent);
	}

	public void RegisterControllable(ControllableComponent controllable)
	{
		if (m_Components.TryGetValue(controllable.UniqueId, out var value) && value != null)
		{
			if (value == controllable)
			{
				return;
			}
			controllable.Reset();
		}
		m_Components[controllable.UniqueId] = controllable;
		ControllableState state = (CurrentState.TryGetValue(controllable.UniqueId, out state) ? controllable.GetDefaultState().MergeWith(state) : controllable.GetDefaultState());
		controllable.SetState(state);
	}

	public void UnregisterControllable(ControllableComponent controllable)
	{
		m_Components.Remove(controllable.UniqueId);
	}

	public void SetState(string idOfObject, ControllableState state)
	{
		if (!m_Components.TryGetValue(idOfObject, out var value))
		{
			PFLog.Entity.Warning("Cant find controllable with id " + idOfObject);
		}
		if (!Game.Instance.SceneControllables.TryGetState(idOfObject, out var state2))
		{
			state2 = ((value != null) ? value.GetDefaultState() : new ControllableState());
		}
		state = state2.MergeWith(state);
		value?.SetState(state);
		CurrentState.SetState(idOfObject, state);
	}

	public bool TryGetState(string idOfObject, out ControllableState state)
	{
		state = null;
		if (CurrentState == null)
		{
			return false;
		}
		if (CurrentState.TryGetValue(idOfObject, out state))
		{
			return true;
		}
		return false;
	}

	public void Rescan()
	{
		InitState();
		ControllableComponent[] array = UnityEngine.Object.FindObjectsOfType<ControllableComponent>(includeInactive: true);
		Array.Sort(array, (ControllableComponent a, ControllableComponent b) => string.Compare(a.UniqueId, b.UniqueId, StringComparison.Ordinal));
		ControllableComponent[] array2 = array;
		foreach (ControllableComponent controllableComponent in array2)
		{
			controllableComponent.GatherStateAtStartUp();
			(controllableComponent as ControllableAnimator)?.Setup();
			RegisterControllable(controllableComponent);
		}
	}

	private void InitState()
	{
		string stateName = StateName;
		SceneControllablesState sceneControllablesState = EntityService.Instance.GetEntity<SceneControllablesState>(stateName);
		if (sceneControllablesState == null)
		{
			SceneControllablesState sceneControllablesState2 = new SceneControllablesState(stateName, isInGame: true);
			Game.Instance.LoadedAreaState.MainState.AddEntityData(sceneControllablesState2);
			sceneControllablesState = Entity.Initialize(sceneControllablesState2);
		}
		CurrentState = sceneControllablesState;
	}

	public void OnStop()
	{
		m_Components.Clear();
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		Rescan();
	}
}
