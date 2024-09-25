using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class CustomControllerDemo_Player : MonoBehaviour
{
	public int playerId;

	public float speed = 1f;

	public float bulletSpeed = 20f;

	public GameObject bulletPrefab;

	private Player _player;

	private CharacterController cc;

	private Player player
	{
		get
		{
			if (_player == null)
			{
				_player = ReInput.players.GetPlayer(playerId);
			}
			return _player;
		}
	}

	private void Awake()
	{
		cc = GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (ReInput.isReady)
		{
			Vector2 vector = new Vector2(player.GetAxis("Move Horizontal"), player.GetAxis("Move Vertical"));
			cc.Move(vector * speed * Time.deltaTime);
			if (player.GetButtonDown("Fire"))
			{
				Vector3 vector2 = Vector3.Scale(new Vector3(1f, 0f, 0f), base.transform.right);
				Object.Instantiate(bulletPrefab, base.transform.position + vector2, Quaternion.identity).GetComponent<Rigidbody>().velocity = new Vector3(bulletSpeed * base.transform.right.x, 0f, 0f);
			}
			if (player.GetButtonDown("Change Color"))
			{
				Renderer component = GetComponent<Renderer>();
				Material material = component.material;
				material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
				component.material = material;
			}
		}
	}
}
