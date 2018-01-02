


namespace Dotc.Wpf.Controls.XmlEditor.Folding
{
	/// <summary>
	/// Description of FoldingRegion.
	/// </summary>
	public class FoldingRegion
	{
	    public DomRegion Region { get; set; }

	    public string Name { get; set; }

	    public FoldingRegion(string name, DomRegion region)
		{
			Region = region;
			Name = name;
		}
	}
}
