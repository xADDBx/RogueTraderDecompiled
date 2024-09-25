using Kingmaker.Visual.Particles;
using UnityEngine;

public class ObjectRotator : MonoBehaviour, IDeactivatableComponent
{
	private float _timeAccumulator;

	public Vector3 AngularSpeed;

	public float TimeStep;

	private void Update()
	{
		if (TimeStep > 0f)
		{
			_timeAccumulator += Time.deltaTime;
			if (_timeAccumulator > TimeStep)
			{
				base.transform.localRotation *= Quaternion.Euler(AngularSpeed);
				_timeAccumulator = 0f;
			}
		}
		else
		{
			base.transform.localRotation *= Quaternion.Euler(AngularSpeed * Time.deltaTime);
		}
	}

	public void Stop()
	{
		base.enabled = false;
	}
}
