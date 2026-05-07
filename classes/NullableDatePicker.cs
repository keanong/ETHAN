using System;
using Microsoft.Maui.Controls;

namespace ETHAN.classes.NDatePicker
{
    public class NullableDatePicker : DatePicker
    {
        public static readonly BindableProperty NullableDateProperty =
            BindableProperty.Create(
                nameof(NullableDate),
                typeof(DateTime?),
                typeof(NullableDatePicker),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnNullableDateChanged);

        public DateTime? NullableDate
        {
            get => (DateTime?)GetValue(NullableDateProperty);
            set => SetValue(NullableDateProperty, value);
        }

        public string PlaceholderFormat { get; set; } = "''";

        public NullableDatePicker()
        {
            Format = PlaceholderFormat;

            DateSelected += (s, e) =>
            {
                NullableDate = e.NewDate;
                Format = "dd/MM/yyyy";
            };

            Unfocused += (s, e) =>
            {
                if (NullableDate == null && Format == PlaceholderFormat)
                {
                    NullableDate = Date;
                    Format = "dd/MM/yyyy";
                }
            };
        }

        private static void OnNullableDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var picker = (NullableDatePicker)bindable;

            if (newValue == null)
            {
                picker.Format = picker.PlaceholderFormat;
                picker.Date = DateTime.Today;
            }
            else
            {
                picker.Format = "dd/MM/yyyy";
                picker.Date = (DateTime)newValue;
            }
        }

        public void Clear() => NullableDate = null;
    }
}
