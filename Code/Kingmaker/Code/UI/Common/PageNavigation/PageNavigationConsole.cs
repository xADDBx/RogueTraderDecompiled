using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.PageNavigation;

public class PageNavigationConsole : PageNavigationBase
{
	[Header("Console")]
	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	private readonly CompositeDisposable m_Disposable = new CompositeDisposable();

	public new BoolReactiveProperty HasPrevious { get; } = new BoolReactiveProperty();


	public new BoolReactiveProperty HasNext { get; } = new BoolReactiveProperty();


	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> isActive, bool addDpad = false, bool showHints = true)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			OnPreviousClick();
		}, 14, isActive.And(HasPrevious).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			OnNextClick();
		}, 15, isActive.And(HasNext).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		if (showHints)
		{
			m_Disposable.Add(m_PreviousHint.Bind(inputBindStruct));
			m_Disposable.Add(m_NextHint.Bind(inputBindStruct2));
		}
		else
		{
			m_Disposable.Add(inputBindStruct);
			m_Disposable.Add(inputBindStruct2);
		}
		if (addDpad)
		{
			m_Disposable.Add(inputLayer.AddButton(delegate
			{
				OnPreviousClick();
			}, 4, isActive.And(HasPrevious).ToReactiveProperty()));
			m_Disposable.Add(inputLayer.AddButton(delegate
			{
				OnNextClick();
			}, 5, isActive.And(HasNext).ToReactiveProperty()));
		}
	}

	public void AddInput()
	{
		if (!m_Disposable.Any())
		{
			InputLayer currentInputLayer = GamePad.Instance.CurrentInputLayer;
			m_Disposable.Add(m_PreviousHint.Bind(currentInputLayer.AddButton(delegate
			{
				OnPreviousClick();
			}, 14)));
			m_Disposable.Add(m_NextHint.Bind(currentInputLayer.AddButton(delegate
			{
				OnNextClick();
			}, 15)));
		}
	}

	public void ClearInput()
	{
		m_Disposable.Clear();
	}

	protected override void OnCurrentIndexChanged(int index)
	{
		base.OnCurrentIndexChanged(index);
		HasPrevious.Value = base.HasPrevious;
		HasNext.Value = base.HasNext;
	}

	public override void Dispose()
	{
		base.Dispose();
		m_Disposable.Clear();
	}
}
