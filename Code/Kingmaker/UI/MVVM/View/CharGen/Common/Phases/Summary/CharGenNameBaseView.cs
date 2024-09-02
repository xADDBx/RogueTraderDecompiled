using Kingmaker.Code.UI.MVVM.View.MessageBox;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Summary;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Summary;

public class CharGenNameBaseView : CharInfoComponentWithLevelUpView<CharGenNameVM>
{
	[SerializeField]
	private ScrambledTMP m_NameFieldScrambled;

	[SerializeField]
	protected MessageBoxBaseView m_MessageBoxView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.UnitName.Subscribe(delegate
		{
			SetName();
		}));
		AddDisposable(base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxView.Bind));
	}

	private void SetName()
	{
		string value = base.ViewModel.UnitName.Value;
		if (m_NameFieldScrambled != null && m_NameFieldScrambled.Text != value)
		{
			m_NameFieldScrambled.SetText(string.Empty, value);
		}
	}

	protected void GenerateRandomName()
	{
		base.ViewModel.SetNameAndNotify(base.ViewModel.GetRandomName());
	}
}
