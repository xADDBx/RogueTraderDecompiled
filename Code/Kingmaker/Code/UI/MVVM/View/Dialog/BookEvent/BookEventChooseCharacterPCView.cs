using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventChooseCharacterPCView : ViewBase<BookEventChooseCharacterVM>
{
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[SerializeField]
	private Button m_CloseButton;

	[Header("Characters Block")]
	[SerializeField]
	private WidgetListMVVM m_WidgetListCharacter;

	[SerializeField]
	private BookEventCharacterPCView m_BookEventCharacterViewPrefab;

	[Header("Skills Block")]
	[SerializeField]
	private WidgetListMVVM m_WidgetListSkills;

	[SerializeField]
	private BookEventSkillsBlockPCView m_BookEventSkillsBlockViewPrefab;

	[Header("Confirm")]
	[SerializeField]
	private Button m_ConfirmButton;

	[Header("Labels")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmLabel;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_WindowAnimator.Initialize();
			m_Title.text = UIStrings.Instance.SkillcheckTooltips.ChooseCharacter;
			m_ConfirmLabel.text = UIStrings.Instance.CommonTexts.Accept;
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(base.ViewModel.SelectedIndex.Subscribe(OnChoose));
		m_ConfirmButton.interactable = false;
		AddDisposable(m_CloseButton.OnClickAsObservable().Subscribe(delegate
		{
			Hide();
		}));
		AddDisposable(m_ConfirmButton.OnClickAsObservable().Subscribe(delegate
		{
			Confirm();
		}));
		DrawCharacterEntities();
		DrawSkillsEntities();
	}

	private void DrawCharacterEntities()
	{
		m_WidgetListCharacter.DrawEntries(base.ViewModel.BookEventCharacters.ToArray(), m_BookEventCharacterViewPrefab);
	}

	private void DrawSkillsEntities()
	{
		m_WidgetListSkills.DrawEntries(base.ViewModel.BookEventSkillsBlocks.ToArray(), m_BookEventSkillsBlockViewPrefab);
	}

	private void Confirm()
	{
		base.ViewModel.ConfirmUnit();
		Hide();
	}

	private void OnChoose(int? index)
	{
		m_WidgetListSkills.Entries?.ForEach(delegate(IWidgetView s)
		{
			(s as BookEventSkillsBlockPCView)?.SelectSkill(index);
		});
		m_ConfirmButton.interactable = index.HasValue;
	}

	private void Show()
	{
		m_WindowAnimator.AppearAnimation();
	}

	private void Hide()
	{
		EventBus.RaiseEvent(delegate(IBookEventUIHandler h)
		{
			h.HandleChooseCharacterEnd();
		});
		m_WindowAnimator.DisappearAnimation();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}
}
