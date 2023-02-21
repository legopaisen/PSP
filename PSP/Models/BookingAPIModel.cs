namespace PSP.Models
{
    public class BookingAPIModel
    {
        public class RequestObject
        {
            public bookingMaster bookingMaster { get; set; }
            public bookingDetails bookingDetails { get; set; }
        }


        public class bookingMaster
        {
            public string entity { get; set; }
            public string transactionDate { get; set; }
            public int externalBusinessEventId { get; set; }
            public int forcedBookingIndicator { get; set; }
            public int orderingSystemId { get; set; }
            public string bookingRequestId { get; set; }
            public string transactionId { get; set; }
            public string transactionSource { get; set; }
            public int reversalCorrectionIndicator { get; set; }
            public string externalTransactionReference { get; set; }
        }
        public class bookingDetails
        {
            public string entity { get; set; }
            public string accountCurrency { get; set; }
            public double accountAmount { get; set; }
            public int accountNumberFormat { get; set; }
            public string accountNumber { get; set; }
            public int bookingType { get; set; }
            public string cashBlockId { get; set; }
            public string orginialValueDate { get; set; }
            public int customerExchangeRate { get; set; }
            public string valueDate { get; set; }
            public string narrativeId { get; set; }
            public string accountingDate { get; set; }
            public double baseCurrencyAmount { get; set; }
            public double transactionAmount { get; set; }
            public string transactionCurrency { get; set; }
            public int cashIndicator { get; set; }
            public string narrativePlaceholders { get; set; }
            public string btcCode { get; set; }
            public string epcCode { get; set; }
            public string businessTransactionDetail1 { get; set; }
            public string businessTransactionDetail2 { get; set; }
            public string reconciliationReferenceId { get; set; }
            public int grossBookingRequired { get; set; }
            public int componentType { get; set; }
            public string virtualAccountNumber { get; set; }
            public string taxCode { get; set; }
            //ADDITIONAL FIELDS
            public string counterBranch { get; set; }
            public string branch { get; set; }
            public int? bookId { get; set; } //Bookid of the Account which is used to differentiate between the different books existing in the system which will be mainly used for internal account
            public int counterBookId { get; set; }

        }
    }
}
