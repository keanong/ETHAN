using The49.Maui.BottomSheet;

namespace ETHAN.BS;

public partial class CJob_DelTime_BS : The49.Maui.BottomSheet.BottomSheet
{
	public CJob_DelTime_BS()
	{
		InitializeComponent();
	}

    void btnCXSaveDelTimeClick(Object sender, EventArgs e)
    {
        try
        {
            DismissAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void btnSaveDelTimeClick(object sender, EventArgs e)
    {
        try
        {
            DismissAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }
}