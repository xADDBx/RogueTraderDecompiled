using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class LampSliderPrediction : SliderPrediction
{
	[SerializeField]
	private RectTransform m_DisableLampsContainer;

	[SerializeField]
	private Image m_DisableLampImage;

	private List<Image> m_DisableLampsPool;

	[SerializeField]
	private RectTransform m_BackLampsContainer;

	[SerializeField]
	private Image m_BackLampImage;

	private List<Image> m_BackLampsPool;

	[SerializeField]
	private RectTransform m_FrontLampsContainer;

	[SerializeField]
	private Image m_FrontLampImage;

	[SerializeField]
	private Image m_HintPlace;

	private List<Image> m_FrontLampsPool;

	private bool m_IsActionPoints;

	public override IDisposable Bind(IReactiveProperty<float> maxValue, IReactiveProperty<float> currentValue, IReactiveProperty<float> predictionValue, bool isYellowPoints)
	{
		Prepare();
		IDisposable result = base.Bind(maxValue, currentValue, predictionValue, isYellowPoints);
		AddDisposable(maxValue.CombineLatest(currentValue, (float max, float current) => new { max, current }).Subscribe(value =>
		{
			float f = ((value.current > value.max) ? value.current : value.max);
			DrawLamps(m_DisableLampsContainer, m_DisableLampImage, m_DisableLampsPool, Mathf.RoundToInt(f));
			DrawLamps(m_BackLampsContainer, m_BackLampImage, m_BackLampsPool, Mathf.RoundToInt(f));
			DrawLamps(m_FrontLampsContainer, m_FrontLampImage, m_FrontLampsPool, Mathf.RoundToInt(f));
		}));
		m_IsActionPoints = isYellowPoints;
		if ((bool)m_HintPlace)
		{
			AddDisposable(currentValue.Subscribe(delegate(float value)
			{
				SetPointsHint(value);
			}));
		}
		return result;
	}

	private void Prepare()
	{
		TryCollectLampsPool(m_DisableLampsContainer, ref m_DisableLampsPool);
		TryCollectLampsPool(m_BackLampsContainer, ref m_BackLampsPool);
		TryCollectLampsPool(m_FrontLampsContainer, ref m_FrontLampsPool);
	}

	private bool TryCollectLampsPool(RectTransform container, ref List<Image> pool)
	{
		if (pool != null)
		{
			return false;
		}
		pool = new List<Image>();
		pool = container.GetComponentsInChildren<Image>().ToList();
		return true;
	}

	private void DrawLamps(RectTransform container, Image lamp, List<Image> pool, int num)
	{
		for (int i = 0; i < num; i++)
		{
			if (pool.Count > i)
			{
				pool[i].gameObject.SetActive(value: true);
				continue;
			}
			pool.Add(UnityEngine.Object.Instantiate(lamp, container, worldPositionStays: false));
			pool.Last().gameObject.SetActive(value: true);
		}
		for (int j = num; j < pool.Count; j++)
		{
			pool[j].gameObject.SetActive(value: false);
		}
	}

	private void SetPointsHint(float value)
	{
		if (m_IsActionPoints)
		{
			m_HintPlace.SetHint($"{UIStrings.Instance.ActionBar.ActionPoints.Text}: {value}");
		}
		else
		{
			m_HintPlace.SetHint($"{UIStrings.Instance.ActionBar.MovementPoints.Text}: {value}");
		}
	}
}
