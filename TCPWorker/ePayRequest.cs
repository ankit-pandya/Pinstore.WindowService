using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TCPWorker
{

    public class Request
    {

        public int mid { get; set; }
        public string tid { get; set; }
        public string txnNO { get; set; }
        public ePayRequest epayrequest { get; set; }
    }


    public class ePayRequest
    {
        //{
        //"TYPE": "SALE",
        //"MODE": "DIRECT",
        //"AUTHORIZATION": {"USERNAME": "3R_PINBANK","PASSWORD": "T3$t_001"},
        //"LOCALDATETIME": "2022-04-29 10:41:03",
        //"TERMINALID": "37835326",
        //"TXID": "1496310063895",
        //"RETAILERID": "935074",
        //"SHOPID": 1234567,
        //"CURRENCY": "GBP",
        //"AMOUNT": 5000,
        //"PRODUCTID": "UKPIN10300001",
        //"RECEIPT": {"LANGUAGE": "ENG"}
        //}

        public string type { get; set; }
        public string mode { get; set; }
        public ePayAuth authorization { get; set; }
        public string localdatetime { get; set; }
        public string terminalId { get; set; }
        public string txid { get; set; }
        public string retailerId { get; set; }
        public int shopId { get; set; }
        public string currency { get; set; }
        public int amount { get; set; }
        public string productId { get; set; }
        public ePayReceipt receipt { get; set; }        
        public Card card { get; set; }
    }

    public class ePayAuth
    {
        public string userName { get; set; }
        public string password { get; set; }
    }

    public class ePayReceipt
    {
        public string language { get; set; }
    }
    public class Card
    {
        public string PAN { get; set; }
    }
}

