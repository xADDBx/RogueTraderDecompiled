using System;

namespace Kingmaker.UI.Common;

[Serializable]
public class TooltipStringsFormat
{
	public string H1 = "<align=\"center\"><color=#{1}><size=140%><b>{0}</b></size></color></align>\n";

	public string H2 = "<align=\"center\"><color=#{1}><size=120%><uppercase>{0}</uppercase></size></color></align>\n";

	public string H3 = "<align=\"center\"><color=#{1}><size=120%><b>{0}</b></color></size></align>\n";

	public string H4 = "<align=\"center\"><color=#{1}><size=100%><b>{0}</b></color></size></align>\n";

	public string Separator1 = "<--separator1-->\n";

	public string Separator2 = "<--separator2-->\n";

	public string Separator3 = "<--separator3-->\n";

	public string SimpleParameter = "{0}: <b><size=100%>{1}</size></b>\n";

	public string ComponentParameter = "<pos=10%>{1}<pos=25%>—<pos=37%>{0}\n";

	public string CentredText = "<align=\"center\">{0}</align>\n";

	public string RightAlignment = "<align=\"right\">{0}</align>\n";

	public string CentredColorText = "<color=#{1}><align=\"center\">{0}</align></color>\n";

	public string EquipPosibility = "<color=#{1}><align=\"center\">{0}</align></color>\n";
}
