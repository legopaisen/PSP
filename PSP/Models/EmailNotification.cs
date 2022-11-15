using CTBC;
using System;

namespace PSP.Models
{
    public class EmailNotification
    {
        public struct NotificationSettings
        {
            public string SMPTPServer { get; set; }
            public string Sender { get; set; }
            public string SubjectError { get; set; }
            public string SubjectSuccess { get; set; }
            public string BodyError { get; set; }
            public string BodySuccess { get; set; }
            public string LogFilePath { get; set; }
            public string Email { get; set; }
            public string AccountEmailSubject { get; set; }
            public string AccountEmailRecipient { get; set; }
        }
        public int SendErrorEmailNotification(string sCompany, string pBranchName, string pDescription)
        {
            int intReturn = 0;
            string strMailBody = "";
            strMailBody += "Dear " + pBranchName + ",";
            strMailBody += "\r\n";
            strMailBody += "\r\n";
            strMailBody += "There are errors in the encrypted file. Please check attached logs file." + "\r\n" + "\r\n";
            strMailBody += "Best Regards," + "\r\n";
            strMailBody += "System Owner (This is a test email)";
            try
            {
                using (CTBC.Mail.Mailer mail = new CTBC.Mail.Mailer())
                {
                    NotificationSettings notificationsettings = new NotificationSettings();
                    mail.Sender = "itp2dev4@ctbcbank.com.ph";//for testing
                    mail.Recipient = "itp2ba3@ctbcbank.com.ph"; //for testing
                    mail.Subject = "Error found in encrypted file of (" + sCompany + ") ";
                    mail.Body = strMailBody;
                    mail.SMTPServer = "172.16.4.52";
                    intReturn = mail.Send().ToInt();
                }
            }
            catch (Exception e)
            {
                CTBC.Logs.Write("Send Email Notification", e.Message, "Request List");
            }
            return intReturn;

        }
    }
}
