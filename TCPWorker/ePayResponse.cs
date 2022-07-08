using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TCPWorker
{
    public class ePayResponse
    {
        //        {
        //    "AMOUNT": 5000,
        //    "CURRENCY": "GBP",
        //    "HOSTTXID": "EP9110002000901281",
        //    "MODE": "DIRECT",
        //    "PINCREDENTIALS": {
        //        "PIN": "898757550230217155706156",
        //        "SERIAL": "00000000000002208847",
        //        "VALIDFROMStr": "",
        //        "VALIDTO": "2022-04-29T00:00:00.0000000Z",
        //        "VALIDTOStr": "2022-04-29T00:00:00.0000000+01:00"
        //    },
        //    "RECEIPT": {
        //        "CUSTOMER": [
        //            "* Customer Copy *",
        //            "",
        //            "UP Test EV",
        //            "",
        //            "epay TXN ID: EP9110002000901281",
        //            "POS Date: 29/04/2022",
        //            "POS Time: 10:41:03",
        //            "Cashier: ",
        //            "StoreID: EP3RT1234567",
        //            "PIN Number",
        //            "898757550230217155706156",
        //            "Serial Number",
        //            "00000000000002208847",
        //            "Expiry: 29/04/2022",
        //            "Amount: £50.00",
        //            "",
        //            "",
        //            "Don't delay! Dial 2345 to",
        //            "activate this PIN and",
        //            "receive the credit",
        //            "",
        //            "Vodafone Freebee Rewardz",
        //            "on Pay as you go.",
        //            "",
        //            "Every time you top up",
        //            "you can grab all sorts",
        //            "of fantastic instant rewards",
        //            "or grow pointz to save up",
        //            "for something bigger.",
        //            "",
        //            "Terms apply",
        //            "vodafone.co.uk/rewardz",
        //            "",
        //            ""
        //        ],
        //        "MERCHANT": [
        //            "* Merchant Copy *",
        //            "",
        //            "UP Test EV",
        //            "",
        //            "epay TXN ID: EP9110002000901281",
        //            "POS Date: 29/04/2022",
        //            "POS Time: 10:41:03",
        //            "Cashier: ",
        //            "StoreID: EP3RT1234567",
        //            "Serial Number",
        //            "00000000000002208847",
        //            "Expiry: 29/04/2022",
        //            "Amount: £50.00"
        //        ]
        //    },
        //    "RESULT": 0,
        //    "RESULTTEXT": "OK",
        //    "SERVERDATETIME": "2022-04-29 09:48:17",
        //    "TERMINALID": "37835326",
        //    "TXID": "1496310063895",
        //    "TXNERROR": 0,
        //    "TYPE": "SALE"
        //}

        public int amount { get; set; }
        public string currency { get; set; }
        public string hosttxId { get; set; }
        public string mode { get; set; }
        public ePayPINCREDENTIALS PINCREDENTIALS { get; set; }
        public ePayReceiptdata receipt { get; set; }
        public int result { get; set; }
        public string resulttext { get; set; }
        public string serverdatetime { get; set; }
        public string terminalId { get; set; }
        public string txId { get; set; }
        public int txnerror { get; set; }
        public string type { get; set; }
    }

    public class ePayPINCREDENTIALS
    {
        public string pin { get; set; }
        public string serial { get; set; }
        public string validfromstr { get; set; }
        public string validto { get; set; }
        public string validtostr { get; set; }
        
    }

    public class ePayReceiptdata
    {
        public List<string> customer { get; set; }
        public List<string> merchant { get; set; }
    }
}
