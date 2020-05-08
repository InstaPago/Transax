using System.Windows.Forms;

namespace InstaTransfer.ScraperContracts.Componentes
{
    public interface ISDateSelectionContract
    {
        Label LDateStart { get; set; }
        Label LDateEnd { get; set; }
        DateTimePicker DtpDateStart { get; set; }
        DateTimePicker DtpDateEnd { get; set; }
        Button BDateRangeCancel { get; set; }
        Button BDateRangeAccept { get; set; }
        Form SDateSelector { get; }
        string StartDate { get; set; }
        string EndDate { get; set; }
    }
}
