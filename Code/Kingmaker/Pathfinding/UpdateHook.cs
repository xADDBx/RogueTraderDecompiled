using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class UpdateHook : MonoBehaviour
{
	public static readonly List<UpdateHook> Instances = new List<UpdateHook>();

	public event Action OnUpdate;

	private void OnEnable()
	{
		if (!Instances.Contains(this))
		{
			Instances.Add(this);
		}
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}

	public void Tick()
	{
		this.OnUpdate?.Invoke();
	}
}
