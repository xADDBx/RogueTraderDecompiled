using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;

public class LocalMapCharacterMarkerPCView : LocalMapMarkerPCView
{
	[SerializeField]
	private Image m_Portrait;

	[NonSerialized]
	public string CharacterName;

	[SerializeField]
	private GameObject m_SelectedActiveFrame;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_Portrait.sprite = value;
		}));
		CharacterName = base.ViewModel.Description.Value;
		AddDisposable(base.ViewModel.IsSelected.Subscribe(m_SelectedActiveFrame.SetActive));
	}
}
