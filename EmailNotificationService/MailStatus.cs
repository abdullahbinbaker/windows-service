using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailNotificationService
{
    class MailStatus
    {
        public int billNo;
        public String Status;

        public MailStatus(int No, String emailstatus)
        {
            billNo = No;
            Status=emailstatus;
        }
    }
}
