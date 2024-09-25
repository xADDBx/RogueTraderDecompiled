using System;
using Owlcat.QA.Validation;

namespace Kingmaker.View.Spawners;

[Serializable]
public class SquadSettings
{
	[ValidateNoNullEntries]
	public UnitSpawner[] Spawners = new UnitSpawner[0];

	[ValidateNotNull]
	public UnitSpawner Leader;
}
