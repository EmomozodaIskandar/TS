using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TS.Classes
{
    class Products
    {
        private static int IdCounter = 0;
        public int id { get; }
        public Products() 
        {
            this.id = ++IdCounter;
        }

        public decimal Weight { get; set; }
        public string Describe { get; set; }
        public int SenderId { get; set; }
        public  int RecipientId { get; set; }
        

    }

}
