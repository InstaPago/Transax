using System.Windows.Forms;

namespace InstaTransfer.ScraperContracts.Banco.Provincial
{
    public interface ISProvincialContract : ISContract
    {
        ToolStripTextBox TstbRIF { get; set; }
    }
}
