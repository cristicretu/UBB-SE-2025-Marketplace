namespace SharedClassLibrary.DataTransferObjects
{
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Represents the data needed to add a new contract, including the PDF file.
    /// </summary>
    public class AddContractRequest
    {
        public Contract? Contract { get; set; }
        public byte[]? PdfFile { get; set; }
    }
}
