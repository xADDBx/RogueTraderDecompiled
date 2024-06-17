using System;
using UnityEngine;

[Serializable]
public class Enemy : MonoBehaviour
{
	public EnemyType type;

	public int health;

	public float speed;

	public Color color;

	public bool canSwim;

	public int spawnersMask;
}
