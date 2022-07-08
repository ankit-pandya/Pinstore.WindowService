using System;
using System.Collections.Generic;
using System.Text;

namespace TCPWorker
{
    class PinResponse
    {
        public int code { get; set; }
        public string supplierCode { get; set; }
        public string productCode { get; set; }
        public decimal value { get; set; }
        public string pin { get; set; }
        public string serial { get; set; }
        public string helplineNumber { get; set; }
        public string info { get; set; }
        public string transactionId { get; set; }
    }
}
