using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d0da535738ae4b4bb8152126acf94812")]
public class ContextActionOnRandomNodesAround : ContextAction
{
	private const int MAX_RADIUS = 100;

	[SerializeField]
	private ActionList m_Actions;

	[SerializeField]
	private ContextValue m_NumberOfPoints;

	[InfoBox("Max radius, in tiles, to search for. Limit is 100 tiles.")]
	[SerializeField]
	private ContextValue m_TilesRadius;

	[SerializeField]
	private bool m_OnlyFreeNodes;

	public override string GetCaption()
	{
		return "Run a context action on random grid nodes around target";
	}

	protected override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current || base.Context.MaybeCaster.IsPreview() || base.Context.MaybeCaster == null)
		{
			return;
		}
		int radius = Mathf.Clamp(m_TilesRadius.Calculate(base.Context), 1, 100);
		List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(base.Target.NearestNode, base.Target.SizeRect, radius))
		{
			if (item.Walkable && (!m_OnlyFreeNodes || !item.ContainsUnit()))
			{
				list.Add(item);
			}
		}
		if (list.Empty())
		{
			return;
		}
		int num = m_NumberOfPoints.Calculate(base.Context);
		if (num < list.Count)
		{
			while (num > 0 && !list.Empty())
			{
				CustomGridNodeBase customGridNodeBase = list.Random(PFStatefulRandom.Mechanics);
				RunActionsOnNode(customGridNodeBase);
				list.Remove(customGridNodeBase);
				num--;
			}
			return;
		}
		foreach (CustomGridNodeBase item2 in list)
		{
			RunActionsOnNode(item2);
		}
	}

	private void RunActionsOnNode(CustomGridNodeBase node)
	{
		using (base.Context.GetDataScope(new TargetWrapper(node.Vector3Position)))
		{
			m_Actions.Run();
		}
	}
}
