using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.View;

[RequireComponent(typeof(EntityViewBase))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("df652852c690a714e9d6119f9873ed7e")]
public class AwarenessCheckComponent : MonoBehaviour
{
	public SkillCheckDifficulty Difficulty;

	[SerializeField]
	private int DC;

	public float Radius;

	public int GetDC()
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC();
		}
		return DC;
	}

	public int GetCustomDC()
	{
		return DC;
	}

	public void SetCustomDC(int value)
	{
		DC = value;
	}
}
