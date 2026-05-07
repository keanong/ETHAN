using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.classes
{
    public class NumericEntryBehavior : Behavior<Entry>
    {
        // Bindable property to control whether empty input is allowed
        public static readonly BindableProperty AllowEmptyProperty =
            BindableProperty.Create(nameof(AllowEmpty), typeof(bool), typeof(NumericEntryBehavior), false);

        public bool AllowEmpty
        {
            get => (bool)GetValue(AllowEmptyProperty);
            set => SetValue(AllowEmptyProperty, value);
        }


        // Bindable property for the default value when entry is empty
        public static readonly BindableProperty DefValueProperty =
            BindableProperty.Create(nameof(DefValue), typeof(string), typeof(NumericEntryBehavior), "");

        public string DefValue
        {
            get => (string)GetValue(DefValueProperty);
            set => SetValue(DefValueProperty, value);
        }

        // Bindable property to control whether empty input is allowed
        public static readonly BindableProperty IsDecimalProperty =
            BindableProperty.Create(nameof(IsDecimal), typeof(bool), typeof(NumericEntryBehavior), false);

        public bool IsDecimal
        {
            get => (bool)GetValue(IsDecimalProperty);
            set => SetValue(IsDecimalProperty, value);
        }

        // Bindable property to control whether empty input is allowed
        public static readonly BindableProperty IsNumberOnlyProperty =
            BindableProperty.Create(nameof(IsNumberOnly), typeof(bool), typeof(NumericEntryBehavior), false);

        public bool IsNumberOnly
        {
            get => (bool)GetValue(IsNumberOnlyProperty);
            set => SetValue(IsNumberOnlyProperty, value);
        }

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            entry.Unfocused += OnEntryUnfocused; // Handle the Unfocused event
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            entry.Unfocused -= OnEntryUnfocused; // Clean up the Unfocused event
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            try
            {
                var entry = sender as Entry;
                if (entry == null) return;

                string filteredText = "";

                string newText = args.NewTextValue;
                if (String.IsNullOrEmpty(newText))
                {
                    return;
                }



                if (IsDecimal)
                {
                    filteredText = new string(newText.Where(c => char.IsDigit(c) || (c == '.' && newText.Count(x => x == '.') == 1)).ToArray());
                } else if (IsNumberOnly)
                {
                    filteredText = newText;
                    filteredText = filteredText.Replace(".", "");
                } else
                {
                    filteredText = newText;
                }
                    

                if (IsDecimal && filteredText.Contains("."))
                {
                    int decimalIndex = filteredText.IndexOf(".");
                    string decimalPart = filteredText.Substring(decimalIndex + 1);
                    if (decimalPart.Length > 2)
                    {
                        filteredText = filteredText.Substring(0, decimalIndex + 3); // Truncate to 2 decimal places
                    }
                }

                if (newText != filteredText)
                {
                    entry.Text = filteredText;
                    return;
                } else
                {
                    if (!AllowEmpty && !IsDecimal && filteredText.Equals("0"))
                        filteredText = DefValue.ToString();
                    entry.Text = filteredText;
                    return;
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public bool IsZeroDecimal(string value)
        {
            bool zd = false;
            try
            {
                string[] zeroDecimal = new string[] { ".", ".0", ".00", "0.0", "0" };
                // Check if the value is in the zeroDecimal array
                zd = zeroDecimal.Contains(value);

                if (!zd)
                    zd = value.All(c => c == '0');
                // Check if the value only contain zero
            }
            catch (Exception e)
            {
                string s = e.Message;
            }

            return zd;
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs args)
        {
            try
            {
                var entry = sender as Entry;
                if (entry == null) return;

                string v = entry.Text;
                bool empty = string.IsNullOrEmpty(v);

                // Perform actions when the Entry loses focus
                if (empty || (IsDecimal && IsZeroDecimal(v)))
                {
                    entry.Text = DefValue.ToString();
                    //// Apply the default value if the Entry is empty

                } else if (!empty && IsDecimal)
                {
                    entry.Text = EnsureTwoDecimalPlaces(v);
                    //// ensure value is 2 decimal place
                } else
                {
                    //do nothing
                }


                entry.IsEnabled = false;
                entry.IsEnabled = true;
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        static string EnsureTwoDecimalPlaces(string version)
        {
            // Try to parse the string as a double
            if (double.TryParse(version, out double versionNumber))
            {
                // Format the number to two decimal places
                return versionNumber.ToString("F2");
            }
            else
            {
                // If parsing fails, return the original string or handle the error
                throw new ArgumentException("Invalid version format.");
            }
        }

    }
}
