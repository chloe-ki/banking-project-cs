using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WDT_assignment_1.Models
{
    public class Login
    {
        public string LoginID { get; set; }
        public int CustomerID { get; set; }
        public string PasswordHash { get; set; }
    }
}
