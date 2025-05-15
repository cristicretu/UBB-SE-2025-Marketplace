namespace SharedClassLibrary.Domain
{
    public interface IContract
    {
        string AdditionalTerms { get; set; }
        string ContractContent { get; set; }
        long ContractID { get; set; }
        string ContractStatus { get; set; }
        int OrderID { get; set; }
        int PDFID { get; set; }
        int? PredefinedContractID { get; set; }
        int RenewalCount { get; set; }
        long? RenewedFromContractID { get; set; }

        Contract ToContract() {
            return new Contract {
                ContractID = this.ContractID,
                OrderID = this.OrderID,
                ContractStatus = this.ContractStatus,
                ContractContent = this.ContractContent,
                RenewalCount = this.RenewalCount,
                PredefinedContractID = this.PredefinedContractID,
                PDFID = this.PDFID,
                AdditionalTerms = this.AdditionalTerms,
                RenewedFromContractID = this.RenewedFromContractID
            };
        }
    }
}