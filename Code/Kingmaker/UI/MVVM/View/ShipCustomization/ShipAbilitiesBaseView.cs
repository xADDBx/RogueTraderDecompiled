using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.VM.ShipCustomization;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipAbilitiesBaseView : ViewBase<ShipAbilitiesVM>
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[Header("Active Abilities")]
	[SerializeField]
	private TextMeshProUGUI m_ActiveAbilitiesTitle;

	[SerializeField]
	protected WidgetListMVVM m_ActiveAbilitiesWidgetList;

	[Header("Passive Abilities")]
	[SerializeField]
	private TextMeshProUGUI m_PassiveAbilitiesTitle;

	[SerializeField]
	protected WidgetListMVVM m_PassiveAbilitiesWidgetList;

	[Space]
	[SerializeField]
	protected ScrollRectExtended m_ActiveAbilitiesRectExtended;

	[SerializeField]
	protected ScrollRectExtended m_PassiveAbilitiesRectExtended;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_ActiveAbilitiesTitle.text = UIStrings.Instance.Inspect.ActiveAbilitiesTitle;
		m_PassiveAbilitiesTitle.text = UIStrings.Instance.Inspect.PassiveAbilitiesTitle;
		ShowWindow();
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
		m_ActiveAbilitiesWidgetList.Clear();
		m_PassiveAbilitiesWidgetList.Clear();
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		m_FadeAnimator.AppearAnimation();
	}

	private void HideWindow()
	{
		m_FadeAnimator.DisappearAnimation(OnDisappearEnd);
	}

	private void DrawEntities()
	{
		DrawEntitiesImpl();
	}

	protected virtual void DrawEntitiesImpl()
	{
	}

	private void OnDisappearEnd()
	{
		base.gameObject.SetActive(value: false);
	}
}
