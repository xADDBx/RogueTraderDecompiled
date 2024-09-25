using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.CombinedSelector;

public abstract class CombinedSelectorBaseVM<TFirstVM, TSecondVM> : BaseCharGenAppearancePageComponentVM where TFirstVM : BaseCharGenAppearancePageComponentVM where TSecondVM : BaseCharGenAppearancePageComponentVM
{
	private readonly AutoDisposingList<TFirstVM> m_SlideSequentialSelectorVms = new AutoDisposingList<TFirstVM>();

	private readonly AutoDisposingList<TSecondVM> m_TextureSelectorVms = new AutoDisposingList<TSecondVM>();

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	public readonly ReactiveProperty<TFirstVM> CurrentSlideSelector = new ReactiveProperty<TFirstVM>();

	public readonly ReactiveProperty<TSecondVM> CurrentTextureSelector = new ReactiveProperty<TSecondVM>();

	public readonly ReactiveProperty<int> TotalItems = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentIndex = new ReactiveProperty<int>();

	public readonly ReactiveCommand OnSetValues = new ReactiveCommand();

	public IReadOnlyReactiveProperty<string> Title => m_Title;

	protected override void DisposeImplementation()
	{
		Clear();
	}

	private void Clear()
	{
		m_SlideSequentialSelectorVms.Clear();
		m_TextureSelectorVms.Clear();
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void SetValues(IEnumerable<TFirstVM> slideSelectors, IEnumerable<TSecondVM> textureSelectors)
	{
		if (slideSelectors.Any())
		{
			m_SlideSequentialSelectorVms.Clear();
			foreach (TFirstVM slideSelector in slideSelectors)
			{
				m_SlideSequentialSelectorVms.Add(slideSelector);
				AddDisposable(slideSelector.OnChanged.Subscribe(delegate
				{
					Changed();
				}));
			}
		}
		if (textureSelectors.Any())
		{
			m_TextureSelectorVms.Clear();
			foreach (TSecondVM textureSelector in textureSelectors)
			{
				m_TextureSelectorVms.Add(textureSelector);
				AddDisposable(textureSelector.OnChanged.Subscribe(delegate
				{
					Changed();
				}));
			}
		}
		TotalItems.Value = m_SlideSequentialSelectorVms.Count;
		SetIndex((CurrentIndex.Value < TotalItems.Value) ? CurrentIndex.Value : 0);
		OnSetValues.Execute();
	}

	public void SetIndex(int index)
	{
		if (index < 0 || index >= m_SlideSequentialSelectorVms.Count || index >= m_TextureSelectorVms.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		CurrentSlideSelector.Value = m_SlideSequentialSelectorVms[index];
		CurrentTextureSelector.Value = m_TextureSelectorVms[index];
		CurrentIndex.Value = index;
	}

	public void SetNextIndex()
	{
		SetIndex((CurrentIndex.Value + 1) % m_SlideSequentialSelectorVms.Count);
	}
}
