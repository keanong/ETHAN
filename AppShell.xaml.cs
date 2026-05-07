using ETHAN.Views;

namespace ETHAN
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            ////Register all routes
            Routing.RegisterRoute("Login", typeof(Login));
            Routing.RegisterRoute("LoginReg", typeof(LoginReg));
            Routing.RegisterRoute("LoginForgotPwd", typeof(LoginForgotPwd));
            Routing.RegisterRoute("LoginPwdReset", typeof(LoginPwdReset));
            Routing.RegisterRoute("JobSummary", typeof(JobSummary));
            //Routing.RegisterRoute("Testpage", typeof(Testpage));
            //Routing.RegisterRoute("Testpage2", typeof(Testpage2));
            //Routing.RegisterRoute("Homepage", typeof(Homepage));
            //Routing.RegisterRoute("Settings", typeof(Settings));
            //Routing.RegisterRoute("Homepage/Barcode", typeof(BarcodeScanningPage));
            Routing.RegisterRoute("CardShellPage/Barcode", typeof(BarcodeScanningPage));
            Routing.RegisterRoute("CreateJob", typeof(CreateJob));
            Routing.RegisterRoute("AddressPage", typeof(AddressPage));
            //Routing.RegisterRoute("Address2", typeof(Address2));
            Routing.RegisterRoute("AddressBookPage", typeof(AddressBookPage));
            //Routing.RegisterRoute("AddressEditPage", typeof(AddressEditPage));
            //Routing.RegisterRoute("CreateJobPage", typeof(CreateJobPage));
            Routing.RegisterRoute("ManageJobPage", typeof(ManageJobPage));
            Routing.RegisterRoute("InventoryPage", typeof(InventoryPage));
            Routing.RegisterRoute("PrepaidListPage", typeof(PrepaidListPage));
            Routing.RegisterRoute("ChatPage", typeof(ChatPage));
            Routing.RegisterRoute("ChangePwdPage", typeof(ChangePwdPage));
        }
    }
}
