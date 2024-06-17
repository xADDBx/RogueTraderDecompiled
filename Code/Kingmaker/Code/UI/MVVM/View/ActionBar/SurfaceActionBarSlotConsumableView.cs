using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class SurfaceActionBarSlotConsumableView : ActionBarBaseSlotView
{
	[Header("Resource Block")]
	[SerializeField]
	private TextMeshProUGUI m_ResourceCount;

	private VisibilityController m_ResourceCountVisibility;

	[SerializeField]
	private Image m_ResourceCountShadow;

	private VisibilityController m_ResourceCountShadowVisibility;

	[Header("Locked Block")]
	[SerializeField]
	private GameObject m_LockedContainer;

	private VisibilityController m_LockedContainerVisibility;

	protected override void Awake()
	{
		base.Awake();
		m_ResourceCountVisibility = VisibilityController.Control(m_ResourceCount);
		m_ResourceCountShadowVisibility = VisibilityController.Control(m_ResourceCountShadow);
		m_LockedContainerVisibility = VisibilityController.Control(m_LockedContainer);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_ResourceCount != null)
		{
			AddDisposable(base.ViewModel.ResourceCount.Subscribe(delegate
			{
				SetResourceCount();
			}));
		}
	}

	private void SetResourceCount()
	{
		int value = base.ViewModel.ResourceCount.Value;
		bool flag = value > 0;
		m_ResourceCountVisibility.SetVisible(flag);
		m_ResourceCountShadowVisibility.SetVisible(flag);
		if (flag)
		{
			m_ResourceCount.text = value.ToString();
		}
	}
}
