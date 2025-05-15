namespace SharedClassLibrary.Domain
{
    public class Contract : IContract
    {
        // Unique identifier for the contract (Identity field in the database)
        public long ContractID { get; set; }

        // Foreign key linking to the Order table (assumed to be of type integer)
        public int OrderID { get; set; }

        // The status of the contract.
        // Valid values: "ACTIVE", "RENEWED", "EXPIRED"
        public string ContractStatus { get; set; }

        // The full content/text of the contract
        public string ContractContent { get; set; }

        // The count of how many times this contract has been renewed
        public int RenewalCount { get; set; }

        // Optional foreign key to the PredefinedContract table
        public int? PredefinedContractID { get; set; }

        // Foreign key to the PDF table (holds the contract's PDF reference) -> required
        public int PDFID { get; set; }

        // Additional terms or conditions associated with the contract
        public string AdditionalTerms { get; set; }

        // Holds the ID of the original contract if this is a renewal; null otherwise
        public long? RenewedFromContractID { get; set; }
    }
}