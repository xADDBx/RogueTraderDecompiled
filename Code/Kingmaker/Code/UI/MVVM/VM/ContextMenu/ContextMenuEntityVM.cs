using System;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ContextMenu;

public class ContextMenuEntityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly ContextMenuCollectionEntity m_Entity;

	public readonly StringReactiveProperty Title = new StringReactiveProperty();

	public readonly StringReactiveProperty SubTitle = new StringReactiveProperty();

	private readonly BoolReactiveProperty m_IsEnabled = new BoolReactiveProperty(initialValue: true);

	public bool IsHeader => m_Entity?.IsHeader ?? false;

	public Sprite Sprite { get; }

	public IReadOnlyReactiveProperty<bool> IsEnabled => m_IsEnabled;

	public bool IsInteractable => m_Entity?.IsInteractable ?? false;

	public bool IsSeparator => m_Entity?.IsEmpty ?? false;

	public ContextMenuEntityVM(ContextMenuCollectionEntity entity)
	{
		m_Entity = entity;
		UpdateTitle();
		RefreshEnabling();
		Sprite = entity?.Icon;
	}

	public void UpdateTitle()
	{
		SubTitle.Value = m_Entity?.SubTitle;
		if (m_Entity?.Title != null)
		{
			Title.Value = m_Entity?.Title.Text;
		}
		else if (m_Entity?.TitleText != null)
		{
			Title.Value = m_Entity?.TitleText;
		}
	}

	public void RefreshEnabling()
	{
		m_IsEnabled.Value = m_Entity.IsEnabled;
	}

	public void Execute()
	{
		m_Entity?.Execute();
	}

	public UISounds.ButtonSoundsEnum GetClickSoundType()
	{
		return m_Entity.ClickSoundType;
	}

	public UISounds.ButtonSoundsEnum GetHoverSoundType()
	{
		return m_Entity.HoverSoundType;
	}

	protected override void DisposeImplementation()
	{
	}
}
