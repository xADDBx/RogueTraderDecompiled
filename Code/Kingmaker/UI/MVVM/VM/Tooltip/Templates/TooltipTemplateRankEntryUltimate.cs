using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateRankEntryUltimate : TooltipTemplateUIFeature
{
	private readonly UIFeature m_UIFeature;

	private readonly BaseUnitEntity m_Caster;

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit;

	public TooltipTemplateRankEntryUltimate(UIFeature uiFeature, BaseUnitEntity caster, ReactiveProperty<BaseUnitEntity> previewUnit)
		: base(uiFeature)
	{
		m_UIFeature = uiFeature;
		m_Caster = caster;
		m_PreviewUnit = previewUnit;
	}

	protected override void AddDescription(List<ITooltipBrick> bricks)
	{
		try
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				using (GameLogContext.Scope)
				{
					GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_PreviewUnit.Value;
					EntityFact entityFact = null;
					if (m_PreviewUnit.Value.Facts.Get<UnitFact>(UIFeature.Feature) == null)
					{
						entityFact = m_PreviewUnit.Value.AddFact(UIFeature.Feature);
					}
					string fullDescription = TooltipTemplateUtils.GetFullDescription(UIFeature.Feature);
					fullDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(fullDescription, m_PreviewUnit.Value);
					bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(fullDescription, null)));
					if (entityFact != null)
					{
						m_PreviewUnit.Value.Facts.Remove(entityFact);
					}
				}
			}
		}
		catch (Exception arg)
		{
			bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(UIFeature.Description, null)));
			Debug.LogError($"Can't create TooltipTemplate for: {UIFeature.Feature.name}: {arg}");
		}
	}
}
