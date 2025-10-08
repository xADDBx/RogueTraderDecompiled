using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuffVM : TooltipBrickFeatureVM
{
	public StringReactiveProperty Duration = new StringReactiveProperty();

	public string SourceName;

	public string Stack;

	public bool IsDOT;

	public string DOTDesc;

	public StringReactiveProperty DOTDamage = new StringReactiveProperty();

	private readonly Buff m_Buff;

	public TooltipBrickBuffVM(Buff buff, List<Buff> additionalSources = null)
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
		if (additionalSources != null && additionalSources.Count > 0)
		{
			List<string> list = additionalSources.Select((Buff b) => (b?.Context?.MaybeCaster as BaseUnitEntity)?.CharacterName ?? string.Empty).ToList();
			SourceName = ((list.Count > 1) ? $"{list[0]} + {list.Count - 1}" : (list[0] ?? ""));
		}
		else
		{
			SourceName = (m_Buff?.Context?.MaybeCaster as BaseUnitEntity)?.CharacterName ?? string.Empty;
		}
		Tooltip = new TooltipTemplateBuff(buff, additionalSources);
		AvailableBackground = true;
		Stack = ((m_Buff != null && m_Buff.Blueprint.MaxRank > 1) ? (m_Buff.GetRank() + "/" + m_Buff.Blueprint.MaxRank) : string.Empty);
		IsDOT = m_Buff?.Blueprint?.GetComponent<DOTLogicVisual>() != null;
		if (IsDOT)
		{
			DOTDesc = UIUtilityTexts.UpdateDescriptionWithUIProperties(m_Buff?.Description, ((IBuff)m_Buff)?.Caster);
		}
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			Duration.Value = BuffTooltipUtils.GetDuration(m_Buff);
			if (IsDOT)
			{
				DOTDamage.Value = DOTLogicUIExtensions.CalculateDOTDamage(m_Buff)?.AverageValue.ToString();
			}
		}));
	}
}
