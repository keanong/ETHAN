using System.Collections.ObjectModel;
using System.Windows.Input;
using The49.Maui.BottomSheet;
using ETHAN.ViewModel;
using CommunityToolkit.Mvvm.Messaging;

namespace ETHAN.BS;

public class ListAction
{
    public string Title { get; set; }
    public ICommand Command { get; set; }
}

public partial class CJob_Content_BS : The49.Maui.BottomSheet.BottomSheet
{
    List<string> source;

    public ObservableCollection<ListAction> Actions => new()
    {
        new ListAction
        {
            Title = "Share",
            Command = new Command(() => { }),
        },
        new ListAction
        {
            Title = "Copy",
            Command = new Command(() => { }),
        },
        new ListAction
        {
            Title = "Open in browser",
            Command = new Command(() => { }),
        },
        // new ListAction
        //{
        //    Title = "Resize",
        //    Command = new Command(Resize),
        //},
        new ListAction
        {
            Title = "Dismiss",
            Command = new Command(() => DismissAsync()),
        }
    };

    public CJob_Content_BS()
	{
        try
        {
            InitializeComponent();
            addRB();
        }
        catch (Exception ex) {
            string s = ex.Message;
        }
    }

    void addRB()
    {
        try
        {
            ContentTypeVM ctvm = new ContentTypeVM();

            // Create the RadioButton
            var radioButton1 = new RadioButton
            {
                Content = "Document",  // This sets the label of the RadioButton
                Value = "Document",      // Optional: To bind a value to the RadioButton
                TextColor = Colors.White, // Set text color to white
                FontSize = 18, // Optional: change font size for better visibility
                FontAttributes = FontAttributes.Bold,
                ControlTemplate = frt
            };
            var radioButton2 = new RadioButton
            {
                Content = "Light Parcel",  // This sets the label of the RadioButton
                Value = "LightParcel",      // Optional: To bind a value to the RadioButton
                TextColor = Colors.White, // Set text color to white
                FontSize = 18, // Optional: change font size for better visibility
                FontAttributes = FontAttributes.Bold,
                ControlTemplate = frt
            };
            var radioButton3 = new RadioButton
            {
                Content = "Parcel",  // This sets the label of the RadioButton
                Value = "Parcel",      // Optional: To bind a value to the RadioButton
                TextColor = Colors.White, // Set text color to white
                FontSize = 18, // Optional: change font size for better visibility
                FontAttributes = FontAttributes.Bold,
                ControlTemplate = frt
            };
            var radioButton4 = new RadioButton
            {
                Content = "Medication",  // This sets the label of the RadioButton
                Value = "Medication",      // Optional: To bind a value to the RadioButton
                TextColor = Colors.White, // Set text color to white
                FontSize = 18, // Optional: change font size for better visibility
                FontAttributes = FontAttributes.Bold,
                ControlTemplate = frt
            };
            var radioButton5 = new RadioButton
            {
                Content = "Sim Card",  // This sets the label of the RadioButton
                Value = "SimCard",      // Optional: To bind a value to the RadioButton
                TextColor = Colors.White, // Set text color to white
                FontSize = 18, // Optional: change font size for better visibility
                FontAttributes = FontAttributes.Bold,
                ControlTemplate = frt
            };

            radioButton1.CheckedChanged += OnRadioButtonCheckedChanged;
            radioButton2.CheckedChanged += OnRadioButtonCheckedChanged;
            radioButton3.CheckedChanged += OnRadioButtonCheckedChanged;
            radioButton4.CheckedChanged += OnRadioButtonCheckedChanged;
            radioButton5.CheckedChanged += OnRadioButtonCheckedChanged;

            hsl.Children.Add(radioButton1);
            hsl.Children.Add(radioButton2);
            hsl.Children.Add(radioButton3);
            hsl.Children.Add(radioButton4);
            hsl.Children.Add(radioButton5);

            radioButton1.IsChecked = true;
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            RadioButton button = sender as RadioButton;
            //lblv.Text = $"You have chosen: {button.Value}";

            if (button != null && e.Value)
            {
                String t = button.Content.ToString();
                String v = button.Value.ToString();
                lblv.Text = t;
                gridLWH.IsVisible = (v.Equals("Parcel") || v.Equals("LightParcel"));
                //hideKeyboard();
            }
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }

    public void hideKeyboard()
    {
        txtLength.IsEnabled = false;
        txtLength.IsEnabled = true;
        txtWidth.IsEnabled = false;
        txtWidth.IsEnabled = true;
        txtHeight.IsEnabled = false;
        txtHeight.IsEnabled = true;
        txtPcs.IsEnabled = false;
        txtPcs.IsEnabled = true;
        txtWt.IsEnabled = false;
        txtWt.IsEnabled = true;
    }

    void btnCXSaveContentClick(Object sender, EventArgs e)
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

    void btnSaveContentClick(object sender, EventArgs e)
    {
        try
        {
            string v = lblv.Text;
            Msg_ContentType ct = new Msg_ContentType(v);
            ct.pcs = 1;
            ct.Length = 30;
            ct.Width = 20;
            ct.Height = 15;
            WeakReferenceMessenger.Default.Send(ct);
            DismissAsync();
        }
        catch (Exception ex)
        {
            string s = ex.Message;
        }
    }
}