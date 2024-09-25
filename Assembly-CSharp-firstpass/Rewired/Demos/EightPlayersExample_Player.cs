using System;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class EightPlayersExample_Player : MonoBehaviour
{
	public int playerId;

	public float moveSpeed = 3f;

	public float bulletSpeed = 15f;

	public GameObject bulletPrefab;

	private Player player;

	private CharacterController cc;

	private Vector3 moveVector;

	private bool fire;

	[NonSerialized]
	private bool initialized;

	private void Awake()
	{
		cc = GetComponent<CharacterController>();
	}

	private void Initialize()
	{
		player = ReInput.players.GetPlayer(playerId);
		initialized = true;
	}

	private void Update()
	{
		if (ReInput.isReady)
		{
			if (!initialized)
			{
				Initialize();
			}
			GetInput();
			ProcessInput();
		}
	}

	private void GetInput()
	{
		moveVector.x = player.GetAxis("Move Horizontal");
		moveVector.y = player.GetAxis("Move Vertical");
		fire = player.GetButtonDown("Fire");
	}

	private void ProcessInput()
	{
		if (moveVector.x != 0f || moveVector.y != 0f)
		{
			cc.Move(moveVector * moveSpeed * Time.deltaTime);
		}
		if (fire)
		{
			UnityEngine.Object.Instantiate(bulletPrefab, base.transform.position + base.transform.right, base.transform.rotation).GetComponent<Rigidbody>().AddForce(base.transform.right * bulletSpeed, ForceMode.VelocityChange);
		}
	}
}
