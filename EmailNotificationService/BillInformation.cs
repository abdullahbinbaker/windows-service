using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailNotificationService
{
    class BillInformation
    {
       public String customerMail, customerName, customerPhone, billNo,billPrice;

        public BillInformation(String mail,String Name ,String Phone, String billNumber, String Price)
        {
           customerMail=mail;
           customerName=Name;
           customerPhone=Phone;
           billNo=billNumber;
           billPrice = Price;

        }
    }
}
