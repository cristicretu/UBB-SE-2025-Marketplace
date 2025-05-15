using System.Diagnostics.CodeAnalysis;

namespace SharedClassLibrary.Domain
{
    // this class has 0 references in code!!! - Alex (delete when merging if needed)
    [ExcludeFromCodeCoverage]
    public class PDF
    {
        public int ContractID { get; set; } // this will be ignored in the table creation, no need for contractId, not deleted because it might be used in code by others(delete when merging if not needed) - Alex
        public int PdfID { get; set; }
        public byte[] File { get; set; }
    }
}
