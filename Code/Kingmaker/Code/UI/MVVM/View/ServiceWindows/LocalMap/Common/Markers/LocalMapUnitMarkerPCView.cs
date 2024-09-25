using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;

public class LocalMapUnitMarkerPCView : LocalMapMarkerPCView
{
	[SerializeField]
	private Image m_Mark;

	[SerializeField]
	private Sprite m_EnemySprite;

	[SerializeField]
	private Sprite m_NpcSprite;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsEnemy.Subscribe(delegate(bool value)
		{
			m_Mark.sprite = (value ? m_EnemySprite : m_NpcSprite);
		}));
	}
}
