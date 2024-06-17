using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;

public class PostAbilitiesGroupBaseView : ViewBase<AbilitiesInfoGroupVM>
{
	[SerializeField]
	protected PostAbilityView[] m_AbilitySlots;

	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UpdateEventsCommand.Subscribe(delegate
		{
			DrawSlots();
		}));
		DrawSlots();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawSlots()
	{
		for (int i = 0; i < m_AbilitySlots.Length; i++)
		{
			if (i < base.ViewModel.CurrentAbilities.Count)
			{
				m_AbilitySlots[i].Bind(base.ViewModel.CurrentAbilities[i]);
			}
		}
	}

	private void Clear()
	{
		m_AbilitySlots.ForEach(delegate(PostAbilityView slot)
		{
			slot.Unbind();
		});
	}

	public ConsoleNavigationBehaviour GetNavigation(bool isReverse)
	{
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		if (isReverse)
		{
			IEnumerable<PostAbilityView> source = m_AbilitySlots.Reverse();
			gridConsoleNavigationBehaviour.AddColumn(source.ToList());
		}
		else
		{
			gridConsoleNavigationBehaviour.AddRow(m_AbilitySlots);
		}
		return gridConsoleNavigationBehaviour;
	}
}
