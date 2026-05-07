using CommunityToolkit.Mvvm.Input;
using ETHAN.classes;
using System.Collections.ObjectModel;
using XDelServiceRef;

namespace ETHAN.ViewModel
{
    public partial class CreateJobVM
    {

        public CreateJobVM()
        {
            if (invoicesP1 == null)
                invoicesP1 = new ObservableCollection<invoice>();
            if (invoicesP2 == null)
                invoicesP2 = new ObservableCollection<invoice>();
        }

        [RelayCommand]
        async Task BackToHome()
        {
            try
            {
                //await Shell.Current.GoToAsync("///Homepage");
                string v = string.Empty;
                //await Shell.Current.GoToAsync($"..?bcval={v}", true);
                await Shell.Current.GoToAsync($"///Homepage?bcval={v}", true);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        [RelayCommand]
        async Task cfmbox()
        {
            try
            {
                await Shell.Current.DisplayAlert("pending", "Collection Address form under construction.", "OK");
            } catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public JobInfo? job1 { get; set; } = new JobInfo();
        public JobInfo? job2 { get; set; } = null;

        public long JobsIDX { get; set; } = 0;
        public long JobsIDX2 { get; set; } = 0;
        public long JobsIDX2_ { get; set; } = 0;

        public long ParentJobsIDX { get; set; } = 0;
        public long ParentJobsIDX2 { get; set; } = 0;

        public string JobNo { get; set; } = "TEMPJOB";
        public string JobNo2 { get; set; } = "TEMPJOB";

        public string Reference { get; set; } = "";
        public string Reference2 { get; set; } = "";

        public string ConsignmentNote { get; set; } = "";
        public string ConsignmentNote2 { get; set; } = "";

        public int FLAG_1_1 { get; set; } = 0;
        public int FLAG_1_2 { get; set; } = 0;

        public int FLAG_2_1 { get; set; } = 0;
        public int FLAG_2_2 { get; set; } = 0;

        public bool ColdItemSupport_1 { get; set; } = false;
        public bool ColdItemSupport_2 { get; set; } = false;

        public bool BSContentType2opened { get; set; } = false;
        public bool BSDeliveryTimeopened { get; set; } = false;

        public bool BSAddressColopened {  get; set; } = false;
        public bool BSAddressDelopened { get; set; } = false;
        public bool BSAddressRtnopened { get; set; } = false;

        public void setBSAddressColopened()
        {
            try {
                BSAddressColopened = true;
                BSAddressDelopened = false;
                BSAddressRtnopened = false;
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void setBSAddressDelopened()
        {
            try
            {
                BSAddressColopened = false;
                BSAddressDelopened = true;
                BSAddressRtnopened = false;
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public void setBSAddressRtnopened()
        {
            try
            {
                BSAddressColopened = false;
                BSAddressDelopened = false;
                BSAddressRtnopened = true;
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public bool FromSummary { get; set; } = false;

        public bool TwoWay { get; set; } = false;

        #region Address

        public bool txtPostalTextChangedSubscribed { get; set; } = false;

        public int selectedLocationType {  get; set; }

        public int AddressMode {  get; set; }

        //public AddressStructure ColAddress { get; set; }

        //public AddressStructure DelAddress { get; set; }

        //public AddressStructure RtnAddress { get; set; }

        public XDelServiceRef.AddressStructure? ColAddress { get; set; }

        public XDelServiceRef.AddressStructure? ColAddressFinal { get; set; }
        public string PUSI { get; set; } = "";

        public XDelServiceRef.AddressStructure? DelAddress { get; set; }
        
        public XDelServiceRef.AddressStructure? DelAddressFinal { get; set; }
        public string DLSI { get; set; } = "";

        public XDelServiceRef.AddressStructure? RtnAddress { get; set; }

        public XDelServiceRef.AddressStructure? RtnAddressFinal { get; set; }

        public XDelServiceRef.AddressStructure? RtnPUAddress { get; set; }

        public XDelServiceRef.AddressStructure? RtnPUAddressFinal { get; set; }
        public string DLSI2 { get; set; } = "";

        public string CollectionPostal = "159551";

        #endregion

        #region ContentType

        public bool reqCold1 { get; set; }
        public string ContentType1 = "Select";
        public int pcs1 { get; set; } = 1;
        public int kg1 { get; set; } = 1;
        public int length1 { get; set; } = 1;
        public int width1 { get; set; } = 1;
        public int height1 { get; set; } = 1;

        public bool reqCold2 { get; set; }
        public string ContentType2 = "Select";
        public int pcs2 { get; set; } = 1;
        public int kg2 { get; set; } = 1;
        public int length2 { get; set; } = 1;
        public int width2 { get; set; } = 1;
        public int height2 { get; set; } = 1;

        #endregion

        #region CollectionTime

        public string colDatePrevSelectedValue { get; set; } //dd/MM/yyyy

        public string colDateSelectedValue { get; set; } //dd/MM/yyyy

        public string ReadyTimePrevSelectedValue { get; set; }

        public string ReadyTimeSelectedValue { get; set; } //eg. "09:00"

        public string colDateTimeFinalValue { get; set; }

        public string colLunchPrevSelectedIDX { get; set; }

        public string colLunchSelectedIDX { get; set; } = "0";

        public string colDateLunchTimeValue { get; set; } = "0";//int

        #endregion

        public string colDateTimeFinalDispText { get; set; } = "Select";

        public string dfbDateTimeFinalDispText { get; set; } = "Select";

        public string rtndfbDateTimeFinalDispText { get; set; } = "Select";

        #region DeliveryFromTime

        public string dfDatePrevSelectedValue { get; set; } //"dd/MM/yyyy HH:mm"

        public string dfDateSelectedValue { get; set; } //"dd/MM/yyyy HH:mm"

        public string dfTimePrevSelectedValue { get; set; } = "";

        public string dfTimeSelectedValue { get; set; } = "";

        public string dfDateTimeFinalValue { get; set; } = "";

        public string dfLunchPrevSelectedIDX { get; set; } = "0";

        public string dfLunchSelectedIDX { get; set; } = "0";

        public string dfDateLunchTimeValue { get; set; } = "0"; //int

        #endregion

        #region DeliveryByTime

        public string dbTimePrevSelectedValue { get; set; } = "";

        public string dbTimeSelectedValue { get; set; } = "";

        public string dbTimeSelectedActualValue { get; set; } = ""; //"HH:mm"

        public string dbExtFromPrevDateTimeValue { get; set; } = ""; //"dd/MM/yyyy h:mm tt"

        public string dbExtFromDateTimeValue { get; set; } = ""; //"dd/MM/yyyy h:mm tt"

        public string dbExtByPrevDateTimeValue { get; set; } = ""; //"dd/MM/yyyy h:mm tt"

        public string dbExtByDateTimeValue { get; set; } = ""; //"dd/MM/yyyy h:mm tt"

        public string dbDateTimePrevFinalValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public string dbDateTimeFinalValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public DateTime dbExtfromDateTime { get; set; } = DateTime.MinValue;

        public DateTime dbExtbyDateTime { get; set; } = DateTime.MinValue;

        #endregion

        #region DeliveryRtnFromTime

        public string rtndfDatePrevSelectedValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public string rtndfDateSelectedValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public string rtndfDateTimeSelectedValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public string rtndfTimePrevSelectedValue { get; set; } = "";

        public string rtndfTimeSelectedValue { get; set; } = "";
        
        public string rtndfDateTimeFinalValue { get; set; } = "";

        public string rtndfLunchPrevSelectedIDX { get; set; } = "0";

        public string rtndfLunchSelectedIDX { get; set; } = "0";

        public string rtndfDateLunchTimeValue { get; set; } = "0"; //int

        #endregion

        #region DeliveryRtnByTime

        public string rtndbTimeSelectedValue { get; set; } = "";

        public string rtndbTimeSelectedActualValue { get; set; } = ""; //"HH:mm"

        public string rtndbExtByPrevDateTimeValue { get; set; } = "";//"dd/MM/yyyy h:mm tt"

        public string rtndbExtByDateTimeValue { get; set; } = ""; //"dd/MM/yyyy h:mm tt"

        public string rtndbDateTimePrevFinalValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public string rtndbDateTimeFinalValue { get; set; } = ""; //"dd/MM/yyyy HH:mm"

        public DateTime rtndbExtfromDateTime { get; set; } = DateTime.MinValue;

        public DateTime rtndbExtbyDateTime { get; set; } = DateTime.MinValue;

        #endregion

        #region ExpressType

        public string expStr1 { get; set; } = "";

        public string exp1 { get; set; } = "";

        public string expStr2 { get; set; } = "";

        public string exp2 { get; set; } = "";

        #endregion

        #region Payable

        public double payable1 { get; set; } = 0;

        public double payable2 { get; set; } = 0;

        #endregion

        #region Invoice

        public ObservableCollection<invoice> invoicesP1 { get; set; }

        public ObservableCollection<invoice> invoicesP2 { get; set; }

        public bool BSinvP2opened { get; set; }

        public void newInvoicesP1()
        {
            invoicesP1 = new ObservableCollection<invoice>();
        }

        public void newInvoicesP2()
        {
            invoicesP2 = new ObservableCollection<invoice>();
        }

        public ObservableCollection<invoice> getInvoicesP1()
        {
            return invoicesP1;
        }

        public ObservableCollection<invoice> getInvoicesP2()
        {
            return invoicesP2;
        }

        public void addInvP1(invoice invoice)
        {
            invoicesP1.Add(invoice);
        }

        public void addInvP2(invoice invoice)
        {
            invoicesP2.Add(invoice);
        }

        public void removeInvP1(invoice invoice)
        {
            try
            {
                if (invoicesP1 != null && invoicesP1.Count > 0)
                {
                    foreach (invoice inv in invoicesP1)
                    {
                        if (inv == invoice)
                        {
                            invoicesP1.Remove(inv);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public void removeInvP2(invoice invoice)
        {
            try
            {
                if (invoicesP2 != null && invoicesP2.Count > 0)
                {
                    foreach (invoice inv in invoicesP2)
                    {
                        if (inv == invoice)
                        {
                            invoicesP2.Remove(inv);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public void clearInvP1()
        {
            try
            {
                if (invoicesP1 != null)
                    invoicesP1.Clear();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public void clearInvP2()
        {
            try
            {
                if (invoicesP2 != null)
                    invoicesP2.Clear();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        #endregion



    }

}
