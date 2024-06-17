using Kingmaker.Blueprints.Credits;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class TitlesOneColumnPage : CreditsOneColumnPage
{
	[SerializeField]
	protected CreditsCompanyElement CompanyPrefab;

	public override void Append(string row, BlueprintCreditsGroup group)
	{
		base.Append(row, group);
		string text = PageGenerator.ReadCompany(row);
		if (!string.IsNullOrEmpty(text))
		{
			CreditsCompanyElement instance = CompanyPrefab.GetInstance<CreditsCompanyElement>();
			instance.Initialize(text, this);
			m_Rows.Add(instance);
		}
	}
}
