using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual;

public class PostProcessSettings : MonoBehaviour
{
	public bool DefaultBloomEnabled = true;

	public bool DefaultDepthOfFieldEnabled = true;

	public static List<PostProcessSettings> Instances = new List<PostProcessSettings>();

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}
}
