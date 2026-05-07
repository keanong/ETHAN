using System.Collections.ObjectModel;
using System.Windows.Input;
using ETHAN.ViewModel;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using CommunityToolkit.Mvvm.Messaging;
using ETHAN.classes;
using The49.Maui.BottomSheet;

namespace ETHAN.BS;

public partial class CJob_CollectionTime_BS : BottomSheet
{
    string[] times = new string[] { "09:00", "09:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30",
        "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30" };

    public string lblColDateSelected { get; set; }
    public string lblColLunchTimeSelected {  get; set; }
    public string lblColTimeSelected {  get; set; } 




    public CJob_CollectionTime_BS()
    {
        InitializeComponent();
        addLunchTime();
        addColDateList();
        if (!String.IsNullOrEmpty(lblColDateSelected))
            preSelectColDate();
    }

    void addLunchTime()
    {
        try
        {
            List<lunchtime> list = new List<lunchtime>() {
                new() { dispText = "None", value = "0" },
                new() { dispText = "11:30 AM to\r\n12:30 PM", value = "4" },
                new() { dispText = "12:00 PM to\r\n1:00 PM", value = "3" },
                new() { dispText = "12:00 PM to\r\n2:00 PM", value = "2" },
                new() { dispText = "1:00 PM to\r\n2:00 PM", value = "1" },
            };

            cvColLunch.ItemsSource = null;
            cvColLunch.ItemsSource = list;
            cvColLunch.SelectedItem = list[0];            
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addColDateList()
    {
        try
        {
            cvDate.ItemsSource = null;
            date d;
            List<date> list = new List<date>();
            DateTime nw = DateTime.Now;
            int dateV = 0;
            String dow = nw.ToString("ddd");
            string formattedDate = nw.ToString("dd/MM/yyyy");
            TimeSpan ts = new TimeSpan(17, 0, 0);
            for (int i = 1; i <= 7; i++)
            {
                if (i > 1)
                {
                    nw = nw.AddDays(1);
                }

                dow = nw.ToString("ddd");
                dateV = nw.Day;
                formattedDate = nw.ToString("dd/MM/yyyy");


                if (i == 1 && nw.TimeOfDay > ts)
                {
                    //do nothing, now time already passed 1700
                }
                else
                {
                    d = new date() { dispText = dow + "\n" + dateV.ToString(), formattedDate = formattedDate };
                    list.Add(d);
                }
            }

            cvDate.ItemsSource = list;

            int itemCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (String.IsNullOrEmpty(lblCDv.Text) && itemCount > 0)
            {
                if (cvDate.ItemsSource is List<date> items)
                {
                    // Select the first item (index 0)
                    cvDate.SelectedItem = items[0];
                }
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    List<common.eExpressType> fakeAllowedExpTypes { get; set; }

    void addExpTypes()
    {
        fakeAllowedExpTypes = new List<common.eExpressType> {
            //common.eExpressType.etOneHour,
            //common.eExpressType.etTwoHour,
            common.eExpressType.etThreeHour,
            common.eExpressType.etPriority,
            common.eExpressType.etGuaranteed,
            common.eExpressType.etNormal
        };
    }

    private void preSelectColDate()
    {
        try
        {
            int itemCount = cvDate.ItemsSource is List<date> collection ? collection.Count : 0;
            if (itemCount > 0)
            {
                if (cvDate.ItemsSource is List<date> items)
                {
                    string selectedvalue = lblColDateSelected;
                    cvDate.SelectedItem = items[0];
                    date item;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        if (item.formattedDate.Equals(selectedvalue))
                        {
                            cvDate.SelectedItem = items[i];
                            cvDate.ScrollTo(items[i], null, ScrollToPosition.Start, false);
                            break;
                        }

                    }
                }
            }

            List<lunchtime> lst = (List<lunchtime>)cvColLunch.ItemsSource;
            string selectedlunchtime = lblColLunchTimeSelected;
            if (!String.IsNullOrEmpty(selectedlunchtime) && lst.Count > 0)
            {
                if (cvColLunch.ItemsSource is List<lunchtime> items)
                {
                    cvColLunch.SelectedItem = items[0];
                    lunchtime item;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        if (item.value.Equals(selectedlunchtime))
                        {
                            cvColLunch.SelectedItem = items[i];
                            cvColLunch.ScrollTo(items[i], null, ScrollToPosition.Start, false);
                            break;
                        }

                    }
                }
            }
            else if (lst.Count > 0)
                cvColLunch.SelectedItem = lst[0];

        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvDateOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as date;
            if (selectedItem != null)
            {
                lblCDv.Text = selectedItem.formattedDate;
                //lblCDv.Text = selectedItem.Title;
                addColTimeList();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvColLunchOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as lunchtime;
            if (selectedItem != null)
            {
                lblCDLTv.Text = selectedItem.value;
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void addColTimeList()
    {
        try
        {
            cvTime.ItemsSource = null;
            DateTime nw = DateTime.Now;
            DateTime curr = DateTime.Now.Date;
            string selectedDateStr = lblCDv.Text;
            DateTime selectedDate = DateTime.ParseExact(selectedDateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            TimeSpan ts = new TimeSpan(9, 0, 0);
            int sHr = 0;
            int sMin = 0;
            DateTime dt = selectedDate;
            int c = 0;
            string lblReadyTimeValueText = lblReadyTimeValue.Text;
            time t;
            List<time> list = new List<time>();
            string val = "";
            string valdispText = "";
            DateTime dispDT = DateTime.Now;

            if (selectedDate == curr)
            {
                for (int i = 0; i <= times.Length - 1; i++)
                {
                    sHr = Int32.Parse(times[i].Substring(0, 2));
                    sMin = Int32.Parse(times[i].Substring(3, 2));
                    dt = selectedDate.Date.AddHours(sHr).AddMinutes(sMin);
                    ts = new TimeSpan(sHr, sMin, 0);
                    if (nw < dt)
                    {
                        dispDT = dispDT.Date + ts;
                        valdispText = dispDT.ToString("h:mm tt");
                        val = times[i];
                        t = new time() { dispText = valdispText, value = val, IsVisible = true };
                        list.Add(t);
                    }
                }
            }
            else if (selectedDate > curr)
            {
                for (int i = 0; i <= times.Length - 1; i++)
                {

                    sHr = Int32.Parse(times[i].Substring(0, 2));
                    sMin = Int32.Parse(times[i].Substring(3, 2));
                    ts = new TimeSpan(sHr, sMin, 0);
                    dispDT = dispDT.Date + ts;
                    valdispText = dispDT.ToString("h:mm tt");
                    val = times[i];
                    t = new time() { dispText = valdispText, value = val, IsVisible = true };
                    list.Add(t);
                }
            }

            cvTime.ItemsSource = list;
            string selectedvalue = lblColTimeSelected;

            if (!String.IsNullOrEmpty(selectedvalue) && list.Count > 0)
            {
                if (cvTime.ItemsSource is List<time> items)
                {
                    cvTime.SelectedItem = items[0];
                    time item;
                    for (int i = 0; i <= items.Count - 1; i++)
                    {
                        item = items[i];
                        if (item.value.Equals(selectedvalue))
                        {
                            cvTime.SelectedItem = items[i];
                            cvTime.ScrollTo(items[i], null, ScrollToPosition.Start, false);
                            break;
                        }

                    }
                }
            }
            else if (list.Count > 0)
                cvTime.SelectedItem = list[0];
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    private void cvTimeOnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedItem = e.CurrentSelection.FirstOrDefault() as time;
            if (selectedItem != null)
            {
                lblReadyTimeValue.Text = selectedItem.value;
                setColDateTime();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void setColDateTime()
    {
        try
        {
            string selectedDateStr = lblCDv.Text;
            DateTime selectedDate = DateTime.ParseExact(selectedDateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string selectedReadyTimeStr = lblReadyTimeValue.Text;
            int sHr = Int32.Parse(selectedReadyTimeStr.Substring(0, 2));
            int sMin = Int32.Parse(selectedReadyTimeStr.Substring(3, 2));
            //string selectedByTimeStr = lblByTimeValue.Text;
            selectedDate = selectedDate.AddHours(sHr).AddMinutes(sMin);
            lblCDTv.Text = selectedDate.ToString("dd/MM/yyyy HH:mm");
            //string format = "dd/MM/yyyy HH:mm";
            //DateTime dt = DateTime.ParseExact(lblCDTv.Text, format, System.Globalization.CultureInfo.InvariantCulture);
            //DateTime ddt = dt;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }


    void btnCXSaveCollectionTimeClick(Object sender, EventArgs e)
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

    void btnSaveCollectionTimeClick(object sender, EventArgs e)
    {
        try
        {
            Msg_CollectionTime m = new Msg_CollectionTime();
            string d = lblCDv.Text;
            string t = lblReadyTimeValue.Text;
            string v = lblCDTv.Text;
            string lt = lblCDLTv.Text;
            m.lblColTime = v;
            m.lblColDateTimeSelected = v;
            m.lblColDateSelected = d;
            m.lblColTimeSelected = t;
            m.lblColLunchTimeSelected = lt;

            WeakReferenceMessenger.Default.Send(m);
            DismissAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }
}