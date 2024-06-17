using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual;

[ExecuteInEditMode]
public class WindController : MonoBehaviour
{
	public float DirectionScale = 1f;

	public float FlutterScale = 1f;

	private static readonly List<WindController> s_Instances = new List<WindController>();

	public static WindController Instance { get; private set; }

	private void Start()
	{
		s_Instances.Add(this);
		RefreshInstanceInternal();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		s_Instances.Remove(this);
	}

	private void RefreshInstanceInternal()
	{
		if (Game.Instance == null)
		{
			Instance = this;
			return;
		}
		if (CameraRig.Instance == null)
		{
			Instance = this;
			return;
		}
		BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		if (currentlyLoadedAreaPart != null)
		{
			if (currentlyLoadedAreaPart.StaticScene.SceneName == base.gameObject.scene.name)
			{
				Instance = this;
			}
		}
		else
		{
			Instance = this;
		}
	}

	public static void RefreshInstance()
	{
		foreach (WindController s_Instance in s_Instances)
		{
			s_Instance.RefreshInstanceInternal();
		}
	}

	private void Update()
	{
		Vector4 value = Instance.transform.forward * Instance.DirectionScale;
		value.w = Instance.FlutterScale;
		Shader.SetGlobalVector("_Wind", value);
	}
}
