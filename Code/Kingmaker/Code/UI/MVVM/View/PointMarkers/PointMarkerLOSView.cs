using Kingmaker.Code.UI.MVVM.View.Combat;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.PointMarkers;

public class PointMarkerLOSView : ViewBase<LineOfSightVM>
{
	[SerializeField]
	private LineOfSightColor[] m_ColorsTable;

	[SerializeField]
	private Image m_Image;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsVisible.Subscribe(base.gameObject.SetActive));
		AddDisposable(base.ViewModel.HitChance.Subscribe(delegate(float value)
		{
			m_Image.color = GetColorByHitChance(value);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	private Color GetColorByHitChance(float hitChance)
	{
		LineOfSightColor[] colorsTable = m_ColorsTable;
		foreach (LineOfSightColor lineOfSightColor in colorsTable)
		{
			if (hitChance <= lineOfSightColor.HitChance)
			{
				return lineOfSightColor.Color;
			}
		}
		return default(Color);
	}
}
