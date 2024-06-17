using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public class SurfaceCombatUnitPortraitZone : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_Picture;

	[SerializeField]
	private UIUtilityUnit.PortraitCombatSize m_Size = UIUtilityUnit.PortraitCombatSize.Small;

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetUnit(MechanicEntity unit)
	{
		base.gameObject.SetActive(value: true);
		MechanicEntityUIWrapper mechanicEntityUIWrapper = new MechanicEntityUIWrapper(unit);
		Image picture = m_Picture;
		picture.sprite = m_Size switch
		{
			UIUtilityUnit.PortraitCombatSize.Icon => mechanicEntityUIWrapper.Icon, 
			UIUtilityUnit.PortraitCombatSize.Small => mechanicEntityUIWrapper.SmallPortrait, 
			UIUtilityUnit.PortraitCombatSize.Middle => mechanicEntityUIWrapper.MiddlePortrait, 
			_ => mechanicEntityUIWrapper.SmallPortrait, 
		};
	}
}
