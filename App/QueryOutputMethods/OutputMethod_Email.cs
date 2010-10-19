using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLConsole.Data;
using System.Text.RegularExpressions;

namespace sqlconsole.QueryOutputMethods
{
    public class OutputMethod_Email:OutputMethod_Template
    {
        public override string output(System.Collections.ArrayList columnHeaders,
            System.Collections.ArrayList rows,
            System.Collections.Hashtable rowLengthLookup)
        {

            //email://andrewc@hendrygroup.com.au/Trial Signup Report

            Regex regex = new Regex(
              "(?<Proto>[a-zA-Z]*)://(?<ToAddress>[a-zA-Z@._-]*)/(?<Subject"+
              ">[\\sa-zA-Z@._-]*)",
            RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );

            if (regex.IsMatch(PropertyBag.GetProperty("data.output")))
            {
                Match match = regex.Match(PropertyBag.GetProperty("data.output"));
                string emailAddress = match.Groups[2].Value;
                string subject = match.Groups[3].Value;
            
                Console.WriteLine("Sending email to: " + emailAddress);
                string bodySrc = "";

                if (PropertyBag.GetProperty("data.html", "false").ToLower() == "true")
                {
                    OutputMethod_HTML output = new OutputMethod_HTML();
                    bodySrc = output.output(columnHeaders, rows, rowLengthLookup);
                }
                else {
                    OutputMethod_TXTOnly output = new OutputMethod_TXTOnly();
                    bodySrc = output.output(columnHeaders, rows, rowLengthLookup);
                }

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(
                    PropertyBag.GetProperty("mail.fromaddress", "local@localhost"),
                    emailAddress,
                    subject,
                    bodySrc);

                if (PropertyBag.GetProperty("data.html", "false").ToLower() == "true")
                    message.IsBodyHtml = true;

                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(
                    PropertyBag.GetProperty("mail.smtpserver", "127.0.0.1"), 25);

                client.SendCompleted += (a, b) =>
                {
                    Console.WriteLine("[ERROR] Failed to send email.");
                };
                
                client.SendAsync(message, message);

            }

            return "";
        }
        public override string OutputMethodName()
        {
            return "Email";
        }
    }
}
