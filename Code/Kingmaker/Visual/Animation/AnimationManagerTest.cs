using Kingmaker.Visual.Animation.Actions;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimationManagerTest : MonoBehaviour
{
	public AnimationManager Manager;

	public AnimatorControllerAction Locomotion;

	public AnimationClipAction Attack;

	public bool DoAttack;

	private void Start()
	{
	}

	private void Update()
	{
		if (DoAttack)
		{
			Manager.Execute(Attack);
			DoAttack = false;
		}
	}
}
