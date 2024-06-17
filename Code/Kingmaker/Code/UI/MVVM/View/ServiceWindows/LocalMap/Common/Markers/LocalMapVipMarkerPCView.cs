using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Common.Markers;

public class LocalMapVipMarkerPCView : LocalMapMarkerPCView
{
	[SerializeField]
	private Image m_Mark;

	[SerializeField]
	private Sprite m_MapObjectSprite;

	[SerializeField]
	private Sprite m_NpcSprite;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Mark.sprite = (base.ViewModel.IsMapObject.Value ? m_MapObjectSprite : m_NpcSprite);
	}
}
