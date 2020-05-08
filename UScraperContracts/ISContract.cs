using System.Windows.Forms;

namespace InstaTransfer.ScraperContracts
{
    /// <summary>
    /// Contiene todos los elementos comunes para todos los scrapers
    /// </summary>
    public interface ISContract
    {
        string StartDate { get; set; }

        string EndDate { get; set; }

        /// <summary>
        /// Parametros de entrada
        /// </summary>
        string[] EntryValues { get; set; }

        ToolStripSpringTextBox TsstbWebAddress { get; set; }

        WebBrowser WbUmbrellaExplorer { get; set; }

        ToolStrip TsSBrowser { get; set; }

        ToolStripTextBox TstbUser { get; set; }

        ToolStripTextBox TstbPassword { get; set; }

        ToolStripButton TsbLogin { get; set; }

        ToolStripMenuItem TsmiLogout { get; set; }

        ToolStripMenuItem TsmiDates { get; set; }

        ToolStripMenuItem TsmiAccount { get; set; }

        ToolStripMenuItem TsmiDay { get; set; }

        ToolStripMenuItem TsmiRange { get; set; }

        ToolStripMenuItem TsmiAccountTD { get; set; }

        ToolStripMenuItem TsmiAccountBFI { get; set; }

        ToolStripStatusLabel TsslWebPageStatus { get; set; }

        ToolStripButton TsbHome { get; set; }

        ToolStripDropDownButton TsddbMenu { get; set; }

        ToolStripLabel TslPassword { get; set; }

        ToolStripLabel TslUser { get; set; }

        Form SBrowser { get; }

        void FillCredentials();
        string BuildFilePathString(string[] entryValues);
    }
}
