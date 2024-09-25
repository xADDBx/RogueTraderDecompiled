using System.Collections;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public class Birds : MonoBehaviour
{
	private readonly List<BirdLane> m_Lanes = new List<BirdLane>();

	private readonly List<Bird> m_Birds = new List<Bird>();

	public float Pause;

	public float PauseMax;

	public bool DrawLines;

	private void Start()
	{
		m_Lanes.Clear();
		m_Birds.Clear();
		GetComponentsInChildren(m_Lanes);
		GetComponentsInChildren(m_Birds);
		foreach (Bird bird in m_Birds)
		{
			bird.Init();
			Game.Instance.CoroutinesController.Start(Fly(bird));
		}
	}

	private IEnumerator Fly(Bird bird)
	{
		float seconds = PFStatefulRandom.Visuals.Critters.Range(0, 5);
		yield return YieldInstructions.WaitForSecondsGameTime(seconds);
		while ((bool)this)
		{
			seconds = PFStatefulRandom.Visuals.Critters.Range(Pause, (PauseMax > Pause) ? PauseMax : (Pause * 2f));
			yield return YieldInstructions.WaitForSecondsGameTime(seconds);
			if (Game.Instance.Player.Weather.ActualWeather < InclemencyType.Heavy && !Game.Instance.Player.IsInCombat)
			{
				BirdLane lane = m_Lanes.Random(PFStatefulRandom.Visuals.Critters);
				bird.Lane = lane;
				while (bird.Lane != null)
				{
					yield return null;
				}
			}
		}
	}
}
