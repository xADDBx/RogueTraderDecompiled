using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Buffs;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuffVM : TooltipBrickFeatureVM
{
	public StringReactiveProperty Duration = new StringReactiveProperty();

	public string SourceName;

	public string Stack;

	private readonly Buff m_Buff;

	public TooltipBrickBuffVM(Buff buff)
	{
		m_Buff = buff;
		Name = buff.Name;
		if (buff.Icon != null)
		{
			Icon = buff.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(buff.Name);
			IconColor = UIUtility.GetColorByText(buff.Name);
			Acronym = UIUtility.GetAbilityAcronym(buff.Name);
		}
		SourceName = (m_Buff?.Context?.MaybeCaster as BaseUnitEntity)?.CharacterName ?? string.Empty;
		Tooltip = new TooltipTemplateBuff(buff);
		AvailableBackground = true;
		Stack = ((m_Buff != null && m_Buff.Blueprint.MaxRank > 1) ? (m_Buff.GetRank() + "/" + m_Buff.Blueprint.MaxRank) : string.Empty);
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			Duration.Value = BuffTooltipUtils.GetDuration(m_Buff);
		}));
	}
}
