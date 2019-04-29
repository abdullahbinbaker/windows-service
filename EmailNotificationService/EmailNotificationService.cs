using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Configuration;


namespace EmailNotificationService
{
    public partial class EmailNotificationService : ServiceBase
    {
        #region Members

        string conn1 = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
        ManualResetEvent _threadController = new ManualResetEvent(false);
        Dictionary<string, DateTime> _threadsTimer = new Dictionary<string, DateTime>();
        private bool ServiceWorking = false;
        string NumbersOfEmails = ConfigurationSettings.AppSettings["NumbersOfEmails"];
        string senderEmail = ConfigurationSettings.AppSettings["senderEmailAddress"];
        string status = ConfigurationSettings.AppSettings["state"];
        string fileDirectory = ConfigurationSettings.AppSettings["FilePath"];
        string host = ConfigurationSettings.AppSettings["host"];
        string senderEmailPassword = ConfigurationSettings.AppSettings["senderEmailPassword"];
        string Subject = ConfigurationSettings.AppSettings["EmailSubject"];
        Queue<BillInformation> BillInformationQueue = new Queue<BillInformation>();
        Queue<MailStatus> MailStatusQueue = new Queue<MailStatus>();

        string message;


        #endregion

        #region Service Methods
        public EmailNotificationService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log("starting service");
            StartService();
            Log("service started");
        }

        protected override void OnStop()
        {
            Log("stopping service");
            StopService();
            Log("service stopped");
        }

        #endregion

        #region Run Methods
        public void StartService()
        {
            InitCulture();
            ServiceWorking = true;
            Thread.Sleep(50);
            Log("starting notification manager");
            NotificationManager();
            Thread.Sleep(10);
            Log("notification manager started");
        }
        private void StopService()
        {
            ServiceWorking = false;
            _threadController.Set();
            Thread.Sleep(1000);
        }
        #endregion

        #region Helper Methods

        private void CleanMemory()
        {
            try
            {
                Thread.Sleep(100);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Thread.Sleep(250);
            }
            catch { }
        }

        private void InitCulture()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            }
            catch { }
        }

        private void ValidateThreadTimer(string threadName, DateTime dateTime)
        {
            if (!_threadsTimer.ContainsKey(threadName))
                _threadsTimer.Add(threadName, dateTime);
            else _threadsTimer[threadName] = dateTime;
        }
        private bool IsThreadReadyToExecute(string threadName, double seconds)
        {
            if (_threadsTimer.ContainsKey(threadName))
            {
                TimeSpan ts = DateTime.Now.Subtract(_threadsTimer[threadName]);
                if (ts.TotalSeconds >= seconds)
                    return true;
            }
            return false;
        }


        #endregion

        #region Notification Methods

        private void NotificationManager()
        {
            string caption = "NotificationManager";
            new Thread(new ThreadStart(delegate()
            {
                InitCulture();
                ValidateThreadTimer(caption, DateTime.Now.AddDays(-1));
                while (ServiceWorking)
                {
                    try
                    {
                        if (!IsThreadReadyToExecute(caption, Variables.NOTIFICATION_PERIOD_SECONDS))
                            continue;
                        ValidateThreadTimer(caption, DateTime.Now);
                        DoWork();
                        CleanMemory();
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                    }
                    finally { _threadController.WaitOne(1000); }
                }
            })) { IsBackground = true }.Start();
        }


        #endregion

        #region MyRegion
        //DateTime _date1;
        //DateTime _date2;

        private void Log(string text)
        {

            try
            {
                if (status.Equals("ON"))
                {
                    string fileTitle = "EmailNotificationService";
                    if (!Directory.Exists(fileDirectory))
                        Directory.CreateDirectory(fileDirectory);
                    using (StreamWriter sw = new StreamWriter(fileDirectory + '\\' + fileTitle + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", true))
                    {
                        sw.WriteLine(DateTime.Now + "  " + text);
                        sw.Flush();
                    }
                }
            }
            catch
            {

            }
        }

        private void DoWork()
        {
            // Here do your code
            try
            {

                using (SqlConnection conn = new SqlConnection("Data Source=.\\SQL20121;Initial Catalog=billSystem;User Id=serv;Password=ASDasd!@#123"))
                {
                    conn.Open();
                    Log("Now connection is open");
                    SqlCommand cmd1 = new SqlCommand("widowsServiseProsedure", conn);
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@p1", Convert.ToInt16(NumbersOfEmails));
                    Log("You oreder the service to send ( " + NumbersOfEmails + " ) Emails ");
                    using (SqlDataReader reader = cmd1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BillInformationQueue.Enqueue(new BillInformation(reader["cMail"].ToString(), reader["cName"].ToString(), reader["cPhone"].ToString(), reader["billNo"].ToString(), reader["billPrice"].ToString()));

                        }
                    }
                    Log("reader is closed now");
                }
                Log("connection is closed now");
                while (BillInformationQueue.Count != 0)
                {
                    BillInformation information = (BillInformation)BillInformationQueue.Dequeue();
                    message = "Bill NO = " + information.billNo + " has been issued with total price: " + information.billPrice + " for the customer: " + information.customerName + " -PhoneNo: " + information.customerPhone + " - Email :" + information.customerMail;

                    MailMessage msg = new MailMessage(senderEmail, information.customerMail, Subject, message);
                    msg.IsBodyHtml = true;
                    SmtpClient sc = new SmtpClient(host, 587);
                    sc.UseDefaultCredentials = false;
                    NetworkCredential cre = new NetworkCredential(senderEmail, senderEmailPassword);//your mail password
                    sc.Credentials = cre;
                    sc.EnableSsl = true;
                    try
                    {
                        sc.Send(msg);
                        MailStatusQueue.Enqueue(new MailStatus(Convert.ToInt16(information.billNo), "done"));
                        Log("there is an email sent for bill No :" + Convert.ToInt16(information.billNo));
                    }
                    catch (Exception e)
                    {
                        MailStatusQueue.Enqueue(new MailStatus(Convert.ToInt16(information.billNo), "failed"));
                        Log("there is an email didnt send for bill No :" + Convert.ToInt16(information.billNo) + " Because of :" + e.Message + "\n" + e.InnerException.Message);
                    }
                }
                using (SqlConnection conn = new SqlConnection("Data Source=.\\SQL20121;Initial Catalog=billSystem;User Id=serv;Password=ASDasd!@#123"))
                {
                    conn.Open();
                    Log("updating status");
                    while (MailStatusQueue.Count != 0)
                    {
                        MailStatus mailStatus = MailStatusQueue.Dequeue();
                        SqlCommand cmd2 = new SqlCommand("widowsServiceUpdateStatus", conn);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.AddWithValue("@p1", mailStatus.billNo);
                        cmd2.Parameters.AddWithValue("@p2", mailStatus.Status);
                        cmd2.ExecuteNonQuery();
                    }
                    Log("statuses has been updated ");

                }

            }
            catch (Exception e)
            {
                Log("there is a problem in the connection :" + e.Message);
            }
        }

        #endregion


    }
}
