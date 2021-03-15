using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking
{
    class Branch
    {
        string address;
        string city;
        string zip;
        string name;
        public string Address { get => address; set { address = value; } }
        public string City { get => city; set { city = value; } }
        public string ZIP { get => zip; set{ zip = value; } }
        public string NameBranch { get => name; set { name = value; } }
        public Branch()
        {
            
        }
    }
}
