using Kingmaker.Utility.Random;
using UnityEngine;

public class LanternKingExpressionsRandom : MonoBehaviour
{
	public Animator AnimController;

	private void Update()
	{
		if (AnimController != null)
		{
			int value = PFStatefulRandom.Visuals.Animation2.Range(0, 10);
			AnimController.SetInteger("random", value);
		}
	}
}
