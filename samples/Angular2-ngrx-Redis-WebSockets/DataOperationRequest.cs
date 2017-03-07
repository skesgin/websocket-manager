using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketsSample
{
    public class DataOperationRequest
    {
        public Guid OperationId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        //public List<KeyValuePair<string, object>> Params { get; set; }
        public object[] Params { get; set; }

        public DataOperationRequest()
        {
            Params = new object[0];
        }
    }
}
