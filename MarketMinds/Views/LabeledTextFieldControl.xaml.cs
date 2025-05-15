// <copyright file="LabeledTextFieldControl.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A user control that combines a label with a text field, providing support for data binding, error messages, and read-only state.
    /// </summary>
    public sealed partial class LabeledTextFieldControl : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(LabeledTextFieldControl),
            new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Identifies the <see cref="TextValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextValueProperty = DependencyProperty.Register(
            nameof(TextValue),
            typeof(string),
            typeof(LabeledTextFieldControl),
            new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Identifies the <see cref="ErrorMessage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(LabeledTextFieldControl),
            new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Identifies the <see cref="IsReadOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(LabeledTextFieldControl),
            new PropertyMetadata(null, OnChildViewModelChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledTextFieldControl"/> class.
        /// </summary>
        public LabeledTextFieldControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the label text associated with the text field.
        /// </summary>
        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the text value of the text field.
        /// </summary>
        public string TextValue
        {
            get => (string)this.GetValue(TextValueProperty);
            set => this.SetValue(TextValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the error message displayed below the text field.
        /// </summary>
        public string ErrorMessage
        {
            get => (string)this.GetValue(ErrorMessageProperty);
            set => this.SetValue(ErrorMessageProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the text field is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the error message is visible.
        /// </summary>
        public bool ErrorMessageVisible => !string.IsNullOrWhiteSpace(this.ErrorMessage);

        /// <summary>
        /// Called when the value of a dependency property changes.
        /// </summary>
        /// <param name="dependencyObject">The <see cref="DependencyObject"/> whose property has changed.</param>
        /// <param name="eventArguments">Event data that is issued by any event that changes the value of a dependency property.</param>
        private static void OnChildViewModelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArguments)
        {
            var control = (LabeledTextFieldControl)dependencyObject;
            control.DataContext = eventArguments.NewValue;
        }
    }
}