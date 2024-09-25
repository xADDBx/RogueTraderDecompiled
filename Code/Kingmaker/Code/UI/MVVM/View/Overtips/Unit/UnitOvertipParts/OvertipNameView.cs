using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipNameView : ViewBase<OvertipNameBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	private CanvasRenderer m_CharacterNameCanvasRenderer;

	private CanvasRenderer CharacterNameCanvasRenderer => m_CharacterNameCanvasRenderer ?? (m_CharacterNameCanvasRenderer = m_CharacterName.GetComponent<CanvasRenderer>());

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UnitState.Name.Subscribe(delegate(string value)
		{
			m_CharacterName.text = value;
		}));
		if (m_MultiSelectable != null)
		{
			AddDisposable(base.ViewModel.UnitState.IsEnemy.Subscribe(delegate(bool value)
			{
				m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.UnitState.IsPlayer.Value ? "Party" : "Ally"));
			}));
		}
		AddDisposable(m_CharacterName.SetHint(base.ViewModel.UnitState.Name, null, CharacterNameCanvasRenderer.GetColor()));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
