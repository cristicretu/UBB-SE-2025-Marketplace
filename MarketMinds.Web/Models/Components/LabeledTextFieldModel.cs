namespace WebMarketplace.Models.Components
{
    /// <summary>
    /// Model for the LabeledTextField component
    /// </summary>
    public class LabeledTextFieldModel
    {
        /// <summary>
        /// Gets or sets the HTML ID of the input field
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the input field
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the label text
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current value of the input field
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the placeholder text
        /// </summary>
        public string Placeholder { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message (if any)
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the input field is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the input type (text, email, password, etc.)
        /// </summary>
        public string InputType { get; set; } = "text";

        /// <summary>
        /// Gets a value indicating whether the field has an error
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
} 