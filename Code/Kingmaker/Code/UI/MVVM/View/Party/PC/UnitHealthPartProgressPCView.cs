using Kingmaker.Code.UI.MVVM.VM.Party;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Party.PC;

public class UnitHealthPartProgressPCView : ViewBase<UnitHealthPartVM>
{
	[SerializeField]
	private Slider m_Health;

	[SerializeField]
	private Slider m_TempHealth;

	[Header("Party")]
	[SerializeField]
	private DamageColorSet m_Party;

	[Header("Ally")]
	[SerializeField]
	private DamageColorSet m_Ally;

	[Header("Enemy")]
	[SerializeField]
	private DamageColorSet m_Enemy;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.CurrentHpRatio.CombineLatest(base.ViewModel.MaxHpRatio, base.ViewModel.TempHpRatio, base.ViewModel.IsDead, base.ViewModel.IsPlayer, base.ViewModel.IsEnemy, (float currentValue, float maxValue, float tempValue, bool isDead, bool isPlayer, bool isEnemy) => new { currentValue, maxValue, tempValue, isDead, isPlayer, isEnemy }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			DamageColorSet damageColorSet = (value.isPlayer ? m_Party : (value.isEnemy ? m_Enemy : m_Ally));
			if (!value.isDead)
			{
				m_Health.targetGraphic.color = Color.Lerp((value.currentValue >= 0.5f) ? damageColorSet.DamageColor : damageColorSet.NearDeathColor, (value.currentValue >= 0.5f) ? damageColorSet.NormalColor : damageColorSet.DamageColor, value.currentValue);
			}
			else
			{
				m_Health.targetGraphic.color = damageColorSet.NearDeathColor;
			}
			if (value.maxValue != 0f)
			{
				float currentValue2 = value.currentValue;
				m_Health.value = currentValue2;
				m_TempHealth.value = value.tempValue;
			}
		}));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
