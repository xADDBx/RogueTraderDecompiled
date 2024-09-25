using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UI.Selection;

public class HpBarDisplay : MonoBehaviour
{
	public Transform Bar;

	public Transform ThresholdBar;

	public BaseUnitEntity Target { get; set; }

	private void Start()
	{
		if (Bar == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		float num = ((Target != null) ? Mathf.Clamp((float)(Target.Health.MaxHitPoints - Target.Health.Damage) / (float)Target.Health.MaxHitPoints, 0f, 1f) : 0.5f);
		Bar.localScale = new Vector3(num, Bar.localScale.y, Bar.localScale.z);
		Bar.localPosition = new Vector3((1f - num) * 5f, Bar.localPosition.y, Bar.localPosition.z);
		ThresholdBar.localScale = new Vector3(num / 2f, ThresholdBar.localScale.y, ThresholdBar.localScale.z);
		ThresholdBar.localPosition = new Vector3((1f - num / 2f) * 5f, ThresholdBar.localPosition.y, ThresholdBar.localPosition.z);
	}
}
