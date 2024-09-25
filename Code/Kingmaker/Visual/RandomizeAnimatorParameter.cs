using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual;

public class RandomizeAnimatorParameter : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string parameterName = "Animation";

	[SerializeField]
	private float randomizeInterval = 1f;

	[SerializeField]
	private int[] listOfValues;

	private float m_TimeAtTheEnd;

	private void Update()
	{
		RandomizeAndSetValue();
	}

	private void Awake()
	{
		if (animator == null)
		{
			UberDebug.LogError(this, "Animator is null");
			base.enabled = false;
		}
		if (listOfValues.Length < 1)
		{
			UberDebug.LogError(this, "No values to randomize");
			base.enabled = false;
		}
		if (randomizeInterval <= 0f)
		{
			randomizeInterval = 1f;
		}
	}

	private void RandomizeAndSetValue()
	{
		if (Time.time >= m_TimeAtTheEnd + randomizeInterval)
		{
			int value = Random.Range(0, listOfValues.Length);
			animator.SetInteger(parameterName, value);
			m_TimeAtTheEnd = Time.time;
		}
	}
}
