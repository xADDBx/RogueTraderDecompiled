using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

public class SceneControllablesController : IControllerStop, IController
{
	private Dictionary<string, ControllableComponent> m_Components = new Dictionary<string, ControllableComponent>();

	private SceneControllablesState m_CurrentState;

	private const string STATE_PREFIX = "scene_controllables_";

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
		if (m_CurrentState.TryGetValue(controllable.UniqueId, out var state))
		{
			controllable.SetState(state);
		}
		else
		{
			controllable.SetDefaultState();
		}
	}

	public void UnregisterControllable(ControllableComponent controllable)
	{
		m_Components.Remove(controllable.UniqueId);
	}

	public void SetState(string idOfObject, ControllableState state)
	{
		if (!m_Components.TryGetValue(idOfObject, out var value))
		{
			PFLog.Entity.Error("Cant find controllable with id " + idOfObject);
			return;
		}
		value.SetState(state);
		m_CurrentState.SetState(idOfObject, state);
	}

	public ControllableState GetState(string idOfObject)
	{
		if (!m_CurrentState.TryGetValue(idOfObject, out var state))
		{
			return new ControllableState();
		}
		return state;
	}

	public void Rescan()
	{
		InitState();
		ControllableComponent[] array = Object.FindObjectsOfType<ControllableComponent>(includeInactive: true);
		foreach (ControllableComponent controllable in array)
		{
			RegisterControllable(controllable);
		}
	}

	private void InitState()
	{
		string text = "scene_controllables_" + Game.Instance.LoadedAreaState.MainState.SceneName;
		SceneControllablesState sceneControllablesState = EntityService.Instance.GetEntity<SceneControllablesState>(text);
		if (sceneControllablesState == null)
		{
			SceneControllablesState sceneControllablesState2 = new SceneControllablesState(text, isInGame: true);
			Game.Instance.LoadedAreaState.MainState.AddEntityData(sceneControllablesState2);
			sceneControllablesState = Entity.Initialize(sceneControllablesState2);
		}
		m_CurrentState = sceneControllablesState;
	}

	public void OnStop()
	{
		m_Components.Clear();
	}
}
