using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketsSample.Data
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ProductNumber { get; set; }
        public decimal ListPrice { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
