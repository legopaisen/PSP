using Oracle.ManagedDataAccess.Client;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
namespace PSP.Models
{
    public class ODSFunctions : IDisposable
    {
        public void Dispose() { GC.SuppressFinalize(this); }
        private string[] _sFileData;
        private string _sFileMessage;

        [XmlRoot("CreateBookingRequest")]
        public class CreateBookingRequest
        {
            [XmlElement("CreateBookingInputDetails")]
            public CreateBookingInputDetails createbookinginputDetails { get; set; }
            public struct CreateBookingInputDetails
            {
                [XmlElement("BookingMaster")]
                public List<bookingMaster> BookingMaster { get; set; }
            }
            public struct bookingMaster
            {
                public string BookingRequestID { get; set; }
                public string ExternalBusinessEventID { get; set; }
                public int ForcedBookingIndicator { get; set; }
                public int ReversalCorrectionIndicator { get; set; }
                public string TransactionDate { get; set; }
                public string TransactionID { get; set; }
                public string TransactionSource { get; set; }
                public int OrderingSystemID { get; set; }
                public string Entity { get; set; }

                [XmlElement("Bookingdetailslist")]
                public List<bookingDetailsList> Bookingdetailslist { get; set; }
            }
            public struct bookingDetailsList
            {
                public AccountAmount AccountAmount { get; set; }
                public AccountNumber AccountNumber { get; set; }
                public BaseCurrencyAmount BaseCurrencyAmount { get; set; }
                public int BookingType { get; set; }
                public string BusinessTransactionDetail1 { get; set; }
                public string BusinessTransactionDetail2 { get; set; }
                public string CashBlockID { get; set; }
                public int CustomerExchangeRate { get; set; }
                public int CashIndicator { get; set; }
                public string ReconciliationReferenceID { get; set; }
                public string NarrativeID { get; set; }
                //public NarrativePlaceholders NarrativePlaceholders { get; set; }
                public string BtcCode { get; set; }
                public string EpcCode { get; set; }
                public TransactionAmount TransactionAmount { get; set; }
                public string ValueDate { get; set; }
                public string AccountingDate { get; set; }
                public string Entity { get; set; }
            }
            public struct AccountAmount
            {
                public decimal Amount { get; set; }
                public string Crncy { get; set; }
            }
            public struct AccountNumber
            {
                public int AcntFrmt { get; set; }
                public string AcntNumber { get; set; }
            }
            public struct BaseCurrencyAmount
            {
                public decimal Amount { get; set; }
                public string Crncy { get; set; }
            }
            public struct NarrativePlaceholders
            {
                public string PLHName { get; set; }
                public string PLHValue { get; set; }
            }
            public struct TransactionAmount
            {
                public decimal Amount { get; set; }
                public string Crncy { get; set; }
            }

        }

        private void InsertBookingID(int iBookingIDSeries)
        {
            string sQuery = "DELETE FROM BookingRequestIDList ";
            sQuery += $"where TransDate = '{DateTime.Now:yyyy-MM-dd}'";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = sQuery;
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                }
            }

            sQuery = "INSERT INTO BookingRequestIDList (transdate, BookingRequestID) ";
            sQuery += $"values('{DateTime.Now:yyyy-MM-dd}', '{iBookingIDSeries}')";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = sQuery;
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                }
            }
        }

        private int GetNewBookingSeries()
        {
            int iSeq = 0;
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString("d2");

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = $"select isnull(MAX(BookingRequestID),0)+1 from BookingRequestIDList where MONTH(TransDate) = '{sMonth}' and YEAR(TransDate) = '{sYear}'";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            iSeq = reader.GetInt32(0);
                        }
                    }
                }
            }

            GetBookingID(iSeq);

            return iSeq;
        }

        private string GetBookingID(int iSeqCnt)
        {
            string sBookingID = string.Empty;
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString("d2");
            string sDay = DateTime.Now.Day.ToString("d2");
            string sDate = sYear + sMonth + sDay;
            string sPrefixSystem = "PRP";
            string sSequence = string.Empty;
            switch (iSeqCnt.ToString().Length)
            {
                case 1: sSequence = "00000000000000" + iSeqCnt.ToString(); break;
                case 2: sSequence = "0000000000000" + iSeqCnt.ToString(); break;
                case 3: sSequence = "000000000000" + iSeqCnt.ToString(); break;
                case 4: sSequence = "00000000000" + iSeqCnt.ToString(); break;
                case 5: sSequence = "0000000000" + iSeqCnt.ToString(); break;
                case 6: sSequence = "000000000" + iSeqCnt.ToString(); break;
                case 7: sSequence = "00000000" + iSeqCnt.ToString(); break;
                case 8: sSequence = "0000000" + iSeqCnt.ToString(); break;
                case 9: sSequence = "000000" + iSeqCnt.ToString(); break;
                case 10: sSequence = "00000" + iSeqCnt.ToString(); break;
                case 11: sSequence = "0000" + iSeqCnt.ToString(); break;
                case 12: sSequence = "000" + iSeqCnt.ToString(); break;
                case 13: sSequence = "00" + iSeqCnt.ToString(); break;
                case 14: sSequence = "0" + iSeqCnt.ToString(); break;
                case 15: sSequence = iSeqCnt.ToString(); break;
            }

            sBookingID = sPrefixSystem + sDate + sSequence;

            return sBookingID;
        }

        public List<CreateBookingRequest.bookingMaster> FillMaster()
        {
            List<CreateBookingRequest.bookingMaster> bookingMasterlist = new List<CreateBookingRequest.bookingMaster>();
            string sBookingID = string.Empty;
            string sFileData = string.Empty;
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString("d2");
            string sDay = DateTime.Now.Day.ToString("d2");
            string sDateToday = sYear + sMonth + sDay;
            int iCntBookingID = 0;
            int iFileCount = _sFileData.Length;

            iCntBookingID = GetNewBookingSeries();
            sFileData = _sFileData[0];
            string sMotherAccount = sFileData.Substring(38, 12).Trim();

            for (int i=1; i < iFileCount - 1; i++)
            {
                sFileData = _sFileData[i];
                string sChildAccount = sFileData.Substring(18, 12).Trim();
                string sAmount = sFileData.Substring(30, 13).Trim();
                decimal decAmount = Convert.ToDecimal(sAmount);
                sBookingID = GetBookingID(iCntBookingID);
                bookingMasterlist.Add(new CreateBookingRequest.bookingMaster()
                {
                    BookingRequestID = sBookingID,
                    ExternalBusinessEventID = "19001",
                    ForcedBookingIndicator = 2,
                    ReversalCorrectionIndicator = 3,
                    TransactionDate = sDateToday,
                    TransactionID = "61-15355",
                    TransactionSource = "3777",
                    OrderingSystemID = 19,
                    Entity = "GCTBCPH001",
                    Bookingdetailslist = FillBookingDetails(sMotherAccount, sChildAccount, decAmount.ToString())
                });
                iCntBookingID++;
            }

            InsertBookingID(iCntBookingID);


            return bookingMasterlist;
        }

        public List<CreateBookingRequest.bookingDetailsList> FillBookingDetails(string sMotherAccount, string sChildAccount, string sAmount)
        {
            List<CreateBookingRequest.bookingDetailsList> bookingdetailslist = new List<CreateBookingRequest.bookingDetailsList>();
            decimal dAmount = Convert.ToDecimal(sAmount);
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString("d2");
            string sDay = DateTime.Now.Day.ToString("d2");
            string sDateToday = sYear + sMonth + sDay;

            //debit
            bookingdetailslist.Add(new CreateBookingRequest.bookingDetailsList()
            {
                AccountAmount = new CreateBookingRequest.AccountAmount
                {
                    Amount = dAmount,
                    Crncy = "PHP"
                },
                AccountNumber = new CreateBookingRequest.AccountNumber
                {
                    AcntFrmt = 2,
                    AcntNumber = sMotherAccount
                },
                BaseCurrencyAmount = new CreateBookingRequest.BaseCurrencyAmount()
                {
                    Amount = dAmount,
                    Crncy = "PHP"
                },
                BookingType = 2,
                CashIndicator = 2,
                TransactionAmount = new CreateBookingRequest.TransactionAmount()
                {
                    Amount = dAmount,
                    Crncy = "PHP"
                },
                ValueDate = sDateToday, //yyyymmdd
                Entity = "GCTBCPH001"
            });

            //credit
            bookingdetailslist.Add(new CreateBookingRequest.bookingDetailsList()
            {
                AccountAmount = new CreateBookingRequest.AccountAmount
                {
                    Amount = dAmount,
                    Crncy = "PHP"
                },
                AccountNumber = new CreateBookingRequest.AccountNumber
                {
                    AcntFrmt = 2,
                    AcntNumber = sChildAccount
                },
                BaseCurrencyAmount = new CreateBookingRequest.BaseCurrencyAmount()
                {
                    Amount = dAmount,
                    Crncy = "PHP"
                },
                BookingType = 1,
                CashIndicator = 2,
                TransactionAmount = new CreateBookingRequest.TransactionAmount()
                {
                    Amount = dAmount,
                    Crncy = "PHP"
                },
                ValueDate = sDateToday, //yyyymmdd
                Entity = "GCTBCPH001"
            });

            return bookingdetailslist;
        }

        public Response StartGeneration(string sFileName)
        {
            bool blnReturn = false;
            Response response = new Response();
            List<CreateBookingRequest.bookingMaster> bookingmasterlist = new List<CreateBookingRequest.bookingMaster>();
            List<CreateBookingRequest.bookingDetailsList> bookingdetailslist = new List<CreateBookingRequest.bookingDetailsList>();
            _sFileData = File.ReadAllLines("C:\\Payroll Files\\temp\\decrypted.txt");
            if (!IsValidFile(_sFileData))
            {
                response.ResponseStat = 1;
                response.Description = "There are errors found in the file. Check Logs file.";
                return response;
            }

            try
            {
                bookingmasterlist = FillMaster();
                var filePath = GenerateXMLFile(bookingmasterlist);
                File.Delete("C:\\Payroll Files\\temp\\decrypted.txt");
                Encryptor.EncryptAesManaged(filePath);
                UploadXML(filePath);

                string sRootFile = "C:\\Payroll Files\\" + sFileName;
                string sDestination = "C:\\Payroll Files\\Archive\\";
                if (!Directory.Exists(sDestination))
                {
                    _ = Directory.CreateDirectory(sDestination);
                }
                GC.Collect();
                File.Move(sRootFile, sDestination + sFileName);
                GC.Collect();
                response.ResponseStat = 2;
                response.Description = "File processed successfully!\r\n" + _sFileMessage;
            }
            catch (Exception ex)
            {
                response.ResponseStat = 1;
                response.Description = "Error in generating file. Please contact system administrator";
                return response;
            }

            return response;
        }

        private bool IsValidFile(string[] sFile)
        {
            bool blnReturn = false;
            try
            {
                System.Data.DataTable tblAccountNumbers = new System.Data.DataTable();
                System.Data.DataRow[] result = null;
                System.Data.DataRow drw = null;
                tblAccountNumbers.Columns.Add("AccountNumber");
                tblAccountNumbers.Columns.Add("Amount");
                tblAccountNumbers.Columns.Add("ItemNumber", typeof(Int32));

                int intHeaderRecordCount = 0;
                int intDetailsRecordCount = 0;
                int intTrailerRecordCount = 0;
                int intErrorCount = 0;
                int intFileRowCount = 0;
                int intHashCount = 0;
                long lngTotalAmount = 0;
                bool blnDetailHasError = false;

                string strCompanyName = "";
                string strMotherAccount = "";
                string strBranch = "";
                string strCompanyCode = "";
                string strPayrollDate = "";
                string strLineData = "";
                string[] strData;

                strData = sFile;
                intFileRowCount = strData.Length;

                if (intFileRowCount > 0)
                {
                    ///////////////////////////////////
                    //////// Header Validation ////////
                    ///////////////////////////////////
                    strLineData = strData[0];
                    if (strLineData.Length > 50 || strLineData.Length < 50) { LogWriter.Write("Length of valid characters per line should not be less than or greater than 50 characters."); intErrorCount++; }
                    else
                    {
                        if (strLineData.Trim().Length == 0) { LogWriter.Write("No header record found"); intErrorCount++; }
                        if (strLineData.Substring(0, 1) != "0") { LogWriter.Write("Invalid header record indicator"); intErrorCount++; }
                        if (strLineData.Substring(0, 1) == "0") { intHeaderRecordCount++; }
                    }

                    if (intHeaderRecordCount == 1)
                    {
                        if (strLineData.Substring(1, 8).Trim().Length == 0) { LogWriter.Write("Header date is missing."); intErrorCount++; }
                        if (strLineData.Substring(9, 4).Trim().Length == 0) { LogWriter.Write("Header company code is missing."); intErrorCount++; }
                        if (strLineData.Substring(13, 25).Trim().Length == 0) { LogWriter.Write("Header company name is missing."); intErrorCount++; }
                        if (strLineData.Substring(38, 12).Trim().Length == 0) { LogWriter.Write("Mother Account No. is missing."); intErrorCount++; }

                        if (strLineData.Substring(13, 25).Trim().Length > 0) { strCompanyName = strLineData.Substring(13, 25).Trim(); }
                        if (strLineData.Substring(9, 4).Trim().Length > 0) { strCompanyCode = strLineData.Substring(9, 4).Trim(); }
                        if (strLineData.Substring(38, 12).Trim().Length > 0) { strMotherAccount = strLineData.Substring(38, 12).Trim(); }

                        if (strLineData.Substring(1, 8).Trim().Length == 8)
                        {
                            strPayrollDate = strLineData.Substring(1, 8).Trim();
                            strPayrollDate = strPayrollDate.Substring(4, 2) + "/" + strPayrollDate.Substring(6, 2) + "/" + strPayrollDate.Substring(0, 4);
                            //_strFileDestination += Convert.ToDateTime(strPayrollDate).ToString("MMddyyyy") + "\\";
                            //if (!System.IO.Directory.Exists(this._strFileDestination)) { System.IO.Directory.CreateDirectory(this._strFileDestination); }
                        }
                    }

                    ////////////////////////////////////
                    //////// Details Validation ////////
                    ////////////////////////////////////
                    for (int intIndex = 1; intIndex < intFileRowCount; intIndex++)
                    {
                        strLineData = strData[intIndex];
                        if (strLineData.Substring(0, 1) == "0") { intHeaderRecordCount++; }
                        if (strLineData.Substring(0, 1) == "1") { intDetailsRecordCount++; }
                        if (strLineData.Substring(0, 1) == "2") { intTrailerRecordCount++; }
                        if (strLineData.Substring(0, 1).IndexOf("012") < -1) { LogWriter.Write("Invalid data found in line " + intIndex.ToString()); intErrorCount++; }

                        if (strLineData.Substring(0, 1) == "1")
                        {
                            if (strLineData.Length > 50 || strLineData.Length < 50) { LogWriter.Write("Length of valid characters per line should not be less than or greater than 50 characters."); intErrorCount++; }
                            else
                            {
                                if (strLineData.Substring(1, 8).Trim().Length == 0) { LogWriter.Write("Detail date is blank."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(1, 8).Trim().Length < 8) { LogWriter.Write("Detail date should not be 8 digits."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(9, 4).Trim().Length == 0) { LogWriter.Write("Detail company code is blank."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(9, 4).Trim().Length < 4) { LogWriter.Write("Detail company code should be numeric."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(13, 17).Trim().Length == 0) { LogWriter.Write("Detail Account No. is blank."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(13, 17).Trim().Length < 17) { LogWriter.Write("Detail Account No. should not be less than 17 digits."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(30, 13).Trim().Length == 0) { LogWriter.Write("Detail amount is blank."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(30, 13).Trim().Length < 13) { LogWriter.Write("Detail amount should not be less than 13 digits."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(43, 4).Trim() != "") { LogWriter.Write("Detail batch no. should be blank"); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(47, 3).Trim().Length == 0) { LogWriter.Write("Filler should not be blank."); intErrorCount++; blnDetailHasError = true; }
                                if (strLineData.Substring(47, 3).Trim().Length < 3) { LogWriter.Write("Filler should not be less than 3 digits."); intErrorCount++; blnDetailHasError = true; }

                                if (!blnDetailHasError)
                                {
                                    lngTotalAmount += strLineData.Substring(30, 13).Trim().Length == 13 ? Convert.ToInt64(strLineData.Substring(30, 13).Trim()) : 0;
                                    intHashCount += strLineData.Substring(13, 17).Trim().Length == 17 ? Convert.ToInt32(strLineData.Substring(13, 17).Trim().Substring(strLineData.Substring(13, 17).Trim().Length - 1, 1)) : 0;

                                    //if (strLineData.Substring(13, 17).Trim().Length == 17)
                                    //{
                                    drw = tblAccountNumbers.NewRow();
                                    drw["AccountNumber"] = strLineData.Substring(13, 17).Trim();
                                    drw["Amount"] = strLineData.Substring(30, 13).Trim();
                                    drw["ItemNumber"] = intDetailsRecordCount;
                                    tblAccountNumbers.Rows.Add(drw);
                                    //}

                                }

                            }
                        }
                    }

                    ////////////////////////////////////
                    //////// Trailer Validation ////////
                    ////////////////////////////////////
                    strLineData = strData[strData.Length - 1];
                    if (strLineData.Length > 50 || strLineData.Length < 50) { LogWriter.Write("Length of valid characters per line should not be less than or greater than 50 characters."); intErrorCount++; }
                    else
                    {
                        if (strLineData.Trim().Length == 0) { LogWriter.Write("No trailer record found"); intErrorCount++; }
                        if (strLineData.Substring(0, 1) != "2") { LogWriter.Write("Invalid trailer record indicator"); intErrorCount++; }
                        if (strLineData.Substring(0, 1) == "2") { intTrailerRecordCount++; }
                    }

                    if (intTrailerRecordCount == 1)
                    {
                        if (strLineData.Substring(1, 8).Trim().Length == 0) { LogWriter.Write("Trailer date is missing."); intErrorCount++; }
                        if (strLineData.Substring(1, 8).Trim().Length < 8) { LogWriter.Write("Detail date should not be 8 digits."); intErrorCount++; }
                        if (strLineData.Substring(9, 4).Trim().Length == 0) { LogWriter.Write("Trailer company code is blank."); intErrorCount++; }
                        if (strLineData.Substring(13, 6).Trim().Length == 0) { LogWriter.Write("Total record count is blank."); intErrorCount++; }
                        if (strLineData.Substring(13, 6).Trim().Length < 6) { LogWriter.Write("Total record count should not be less than 6 digits."); intErrorCount++; }
                        if (strLineData.Substring(19, 7).Trim().Length == 0) { LogWriter.Write("Hash total checkdigit is blank."); intErrorCount++; }
                        if (strLineData.Substring(19, 7).Trim().Length < 7) { LogWriter.Write("Hash total checkdigit should not be less than 7 digits."); intErrorCount++; }
                        if (strLineData.Substring(26, 15).Trim().Length == 0) { LogWriter.Write("Total amount is blank."); intErrorCount++; }
                        if (strLineData.Substring(26, 15).Trim().Length < 15) { LogWriter.Write("Total amount should not be less than 15 digits."); intErrorCount++; }
                        if (strLineData.Substring(41, 9).Trim().Length == 0) { LogWriter.Write("Filler is blank."); intErrorCount++; }
                        if (strLineData.Substring(41, 9).Trim().Length < 9) { LogWriter.Write("Filler should not be less than 9 characters."); intErrorCount++; }
                    }

                    if (intHeaderRecordCount == 0) { LogWriter.Write("No header record found."); intErrorCount++; }
                    if (intDetailsRecordCount == 0) { LogWriter.Write("No detail record found."); intErrorCount++; }
                    if (intTrailerRecordCount == 0) { LogWriter.Write("No trailer record found."); intErrorCount++; }

                    System.Data.DataTable tblTemp = tblAccountNumbers;
                    string strExistingAccounts = "";
                    foreach (System.Data.DataRow drwX in tblAccountNumbers.Rows)
                    {
                        result = tblTemp.Select("AccountNumber='" + drwX["AccountNumber"].ToString() + "'");
                        if (result.Length > 1)
                        {
                            if (strExistingAccounts.IndexOf(drwX["AccountNumber"].ToString()) == -1)
                            {
                                strExistingAccounts += drwX["AccountNumber"].ToString();
                            }
                            //LogWriter.Write(result.Length.ToString() + " instances of Account No. - " + drwX["AccountNumber"].ToString() + " were found.");
                            //intErrorCount++;
                        }
                    }


                    if (intErrorCount > 0)
                    {
                        //throw exception - An error occured while validating the PYCTB File.\nCheck logs file
                    }
                    else
                    {
                        if (strExistingAccounts.Length > 0)
                        {
                            //add condition here - Multiple entries of the same account number were found.\nDo you want to continue?
                            blnReturn = true;
                        }
                        else
                        { blnReturn = true; }

                        if (blnReturn)
                        {
                            _sFileMessage = "Transaction Date: " + strPayrollDate;
                            _sFileMessage += "\nTotal Amount: " + (Convert.ToDecimal(lngTotalAmount) / 100).ToString("###,###,##0.00");
                            _sFileMessage += "\nTotal Detail Record Count: " + intDetailsRecordCount.ToString();
                            //strMessage += "\nType of Run: " + (_rtRunType == SystemCore.RunType.Bonus ? "BONUS RUN" : "REGULAR RUN");
                        }

                    }
                }
                else
                {
                    intErrorCount++;
                    //throw exception "PYCTB file is empty".
                }
            }
            catch (Exception e)
            {
                //throw exception
            }

            return blnReturn;
        }

        private void InsertFileSeries(int iSeries)
        {
            string sQuery = $"DELETE FROM XMLFileSeries where FileDate = '{DateTime.Now.ToString("yyyy-MM-dd")}'";
            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = sQuery;
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                }
            }

            sQuery = "INSERT INTO XMLFileSeries (CurrFileID, FileDate) ";
            sQuery += $"values('{iSeries}', '{DateTime.Now.ToString("yyyy-MM-dd")}')";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = sQuery;
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                }
            }
        }

        private string GetFileNameSeries()
        {
            string sSeries = string.Empty;
            int lSeries = 0;
            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = $"select isnull(MAX(CurrFileID),0)+1 from XMLFileSeries where MONTH(FileDate) = '{DateTime.Now.Month.ToString("d2")}' and YEAR(FileDate) = '{DateTime.Now.Year.ToString("d2")}'";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lSeries = reader.GetInt32(0);
                        }
                    }
                }
            }

            int iSeriesCnt = lSeries.ToString().Length;
            switch (iSeriesCnt)
            {
                case 1: sSeries = "00000000" + lSeries.ToString(); break;
                case 2: sSeries = "0000000" + lSeries.ToString(); break;
                case 3: sSeries = "000000" + lSeries.ToString(); break;
                case 4: sSeries = "00000" + lSeries.ToString(); break;
                case 5: sSeries = "0000" + lSeries.ToString(); break;
                case 6: sSeries = "000" + lSeries.ToString(); break;
                case 7: sSeries = "00" + lSeries.ToString(); break;
                case 8: sSeries = "0" + lSeries.ToString(); break;
                case 9: sSeries = lSeries.ToString(); break;
            }

            InsertFileSeries(lSeries);

            return sSeries;
        }

        public string GenerateXMLFileName()
        {
            string XMLFileName = string.Empty;
            string sDay = DateTime.Now.Day.ToString("d2");
            string sMonth = DateTime.Now.Month.ToString("d2");
            string sYear = DateTime.Now.Year.ToString();

            XMLFileName = "BKNGREQ_PRP_BANCS_001001_" + sYear + sMonth + sDay + "-" + GetFileNameSeries() + "_P_N_T_FF";

            return XMLFileName;
        }

        public string GenerateXMLFile(List<CreateBookingRequest.bookingMaster> bookingmasterlist) //for creation of booking
        {
            string XMLFilePath = string.Empty;
            var dir = Path.GetFullPath("C:\\Payroll Files\\FileUpload");
            var file = Path.Combine(dir, GenerateXMLFileName() + ".xml");
            var streamWriter = new StreamWriter(file, false);
            var xmlserializer = new XmlSerializer(typeof(CreateBookingRequest));
            var xmlnamespace = new XmlSerializerNamespaces();
            List<CreateBookingRequest.bookingDetailsList> bookingDetailsList = new List<CreateBookingRequest.bookingDetailsList>();
            List<CreateBookingRequest.bookingMaster> bookingMaster = new List<CreateBookingRequest.bookingMaster>();

            var CreateBookingRequest = new CreateBookingRequest
            {
                createbookinginputDetails = new CreateBookingRequest.CreateBookingInputDetails
                {
                    BookingMaster = bookingmasterlist
                },

            };

            xmlnamespace.Add("", "");
            xmlserializer.Serialize(streamWriter, CreateBookingRequest, xmlnamespace);
            streamWriter.Close();
            XMLFilePath = file;
            return XMLFilePath;
        }

        public void UploadXML(string sFilePath)
        {
            string host = "10.64.44.22";
            int port = 22;
            string username = "t000512";
            string password = "gbp2022";
            string destination = "/home/appgbp/apps/nfspath/SIGBP_BKNGMNG_IN/in";

            string filePath = sFilePath;

            using (var client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                client.ChangeDirectory(destination);
                if (client.IsConnected)
                {
                    using (var stream = new FileStream(filePath, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(stream, Path.GetFileName(filePath));
                        stream.Close();
                    }
                }
                else
                {

                }
            }
        }

    }
}
