using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class CustomControllersTiltDemo : MonoBehaviour
{
	public Transform target;

	public float speed = 10f;

	private CustomController controller;

	private Player player;

	private void Awake()
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		player = ReInput.players.GetPlayer(0);
		ReInput.InputSourceUpdateEvent += OnInputUpdate;
		controller = (CustomController)player.controllers.GetControllerWithTag(ControllerType.Custom, "TiltController");
	}

	private void Update()
	{
		if (!(target == null))
		{
			Vector3 zero = Vector3.zero;
			zero.y = player.GetAxis("Tilt Vertical");
			zero.x = player.GetAxis("Tilt Horizontal");
			if (zero.sqrMagnitude > 1f)
			{
				zero.Normalize();
			}
			zero *= Time.deltaTime;
			target.Translate(zero * speed);
		}
	}

	private void OnInputUpdate()
	{
		Vector3 acceleration = Input.acceleration;
		controller.SetAxisValue(0, acceleration.x);
		controller.SetAxisValue(1, acceleration.y);
		controller.SetAxisValue(2, acceleration.z);
	}
}
