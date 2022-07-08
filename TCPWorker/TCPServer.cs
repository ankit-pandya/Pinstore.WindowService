using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPWorker
{
    public class TCPServer
    {
        public ILogger<Worker> log { get; }

        public TCPServer(ILogger<Worker> ilog)
        {
            log = ilog;
        }

        public async Task HandleClientsAsync()
        {

            //TcpClient client = new TcpClient();            
            TcpListener server = null;
            log.LogInformation("Starting Server");

            Int32 port = 62588;
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, port);
            log.LogInformation($"Listning on ...IP: {localAddr} Port: {port}");
            server.Start();
            log.LogInformation("Waiting for a connection...");

            // Buffer for reading data
            Byte[] bytes = new Byte[256 * 6];
            bool loop = true;
            bool callApis = false;
            //Enter the listening loop.
            while (loop)
            {
                String str = System.DateTime.Now.ToString("h:mm tt");
                if (str.Equals("1:01 AM"))
                {
                    callApis = false;
                }

                if (str.Equals("1:00 AM") && callApis == false)                
                {
                    callApis = true;
                    log.LogInformation($"Updating Balace...");
                    HttpClient httpclientub = new HttpClient();
                    StringContent httpContent = new StringContent("", System.Text.Encoding.UTF8, "application/json");
                    httpclientub.Timeout = System.TimeSpan.FromSeconds(20);                    

                    var response = await httpclientub.GetAsync("https://pins.3rtelecom.co.uk/api/api/UpdateBalance/");
                    log.LogInformation($"{response}");

                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    {
                        HttpClient httpclientubrep = new HttpClient();                        
                        StringContent httpContentrep = new StringContent("", System.Text.Encoding.UTF8, "text/plain");
                        httpclientubrep.Timeout = System.TimeSpan.FromSeconds(30);
                        string fromDate = DateTime.Now.AddDays(-7).ToShortDateString();
                        string toDate = DateTime.Now.AddDays(-1).ToShortDateString();

                        log.LogInformation($"Sending weekly MTU report...");
                        var responseSendingReport = await httpclientubrep.GetAsync($"https://pins.3rtelecom.co.uk/api/api/WeeklyTransactions?from={fromDate}&to={toDate}&type=MTU");                         
                        log.LogInformation($"{responseSendingReport}");
                                                
                        HttpClient httpclientubrepIcc = new HttpClient();
                        StringContent httpContentrepIcc = new StringContent("", System.Text.Encoding.UTF8, "text/plain");
                        httpclientubrepIcc.Timeout = System.TimeSpan.FromSeconds(30);

                        log.LogInformation($"Sending weekly ICC report...");
                        var responseSendingReportICC = await httpclientubrepIcc.GetAsync($"https://pins.3rtelecom.co.uk/api/api/WeeklyTransactions?from={fromDate}&to={toDate}&type=ICC");
                        log.LogInformation($"{responseSendingReportICC}");
                    }
                }
                    
                var client = await server.AcceptTcpClientAsync();

                try
                {

                    int threadID = Thread.CurrentThread.ManagedThreadId;

                    if (client != null)
                    {
                       _= Task.Run(async () =>
                        {
                            var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
                            log.LogInformation($"Terminal connected from {ip}:{((IPEndPoint)client.Client.RemoteEndPoint).Port} on Thread: {threadID}");

                            NetworkStream stream = client.GetStream();

                            int i;
                            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                string data = null;
                                log.LogInformation($"i= {i}");
                                // Translate data bytes to a ASCII string.
                                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                                i = 0;

                                if (data == null)
                                    data = "Blank";

                                // Process the data sent by the client.
                                data = data.ToUpper();

                                if (data.Contains("E3"))
                                {
                                    //log.LogInformation("Original E3: {0}", data);
                                    log.LogInformation($"*****      ICC Transaction Finished      *****");
                                    continue;
                                }

                                string[] fields = data.Split((char)28);
                                string msgno = "";
                                string TermId = "";
                                string productcode = "";
                                string topupcard = "";
                                bool lTopupcard = false;

                                log.LogInformation($"*****      Transaction Started      *****");

                                if (data != null)
                                {
                                    if (data.Contains("PPT42UK"))
                                    {
                                        if(fields[0].Contains(";") || fields[0].Contains("?"))
                                        {
                                            lTopupcard = true;                                            
                                        }

                                        if(lTopupcard)
                                        {
                                            int index = fields[0].IndexOf(';');
                                            int indexEnd = fields[0].IndexOf('=');
                                            productcode = fields[0].Substring(index + 1 , 7);
                                            topupcard = fields[0].Substring(index + 1, (indexEnd - index) - 1);
                                        }
                                        else
                                        {
                                            productcode = fields[0].Substring(39);
                                        }
                                        //msgno = data.Substring(76, 4);
                                        msgno = fields[2].Substring(10, 4);
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Merchant Code: {fields[1]}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Terminal Code: {data.Substring(4, 8)}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Product Code: {productcode}");
                                        if (lTopupcard)
                                            log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Topup Card: {topupcard}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Amount: {fields[0].Substring(23, 11)}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} TXN: {msgno}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Original: {data}");

                                        HttpClient httpclient = new HttpClient();
                                        ePayRequest pinrequest = new ePayRequest();
                                        pinrequest.amount = int.Parse(fields[0].Substring(23, 11));

                                        ePayAuth authData = new ePayAuth { userName = "3R_PINBANK", password = "T3$t_001" };
                                        pinrequest.authorization = authData;

                                        ePayReceipt recipetdata = new ePayReceipt { language = "ENG" };
                                        pinrequest.receipt = recipetdata;

                                        pinrequest.currency = "GBP";

                                        //2022-04-29 10:41:03
                                        pinrequest.localdatetime = $"{DateTime.Now.Year.ToString()}-{DateTime.Now.Month.ToString().PadLeft(2, '0')}-{DateTime.Now.Day.ToString().PadLeft(2, '0')} " +
                                                               $"{DateTime.Now.Hour.ToString()}:{DateTime.Now.Minute.ToString()}:{DateTime.Now.Second.ToString()}";

                                        pinrequest.mode = "DIRECT";
                                        pinrequest.productId = productcode;//fields[0].Substring(39, 13);
                                        if (lTopupcard)
                                        {
                                            Card cardPan = new Card { PAN = "8944111111111111094" };
                                            pinrequest.card = cardPan;
                                        }
                                        pinrequest.retailerId = "9350740";
                                        pinrequest.shopId = int.Parse(fields[1]);
                                        pinrequest.terminalId = "37835326";
                                        pinrequest.txid = msgno;
                                        pinrequest.type = "SALE";

                                        Request request = new Request
                                        {
                                            mid = int.Parse(fields[1]),
                                            tid = data.Substring(4, 8),
                                            txnNO = msgno,
                                            epayrequest = pinrequest
                                        };

                                        string json = JsonConvert.SerializeObject(request);

                                        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                        httpclient.Timeout = System.TimeSpan.FromSeconds(7);
                                        log.LogInformation($"Requesting Pin: {threadID}{fields[1]}{msgno}");

                                        var response = await httpclient.PostAsync("https://pins.3rtelecom.co.uk/api/api/epay/", httpContent);
                                        //var contents = response.Result.ToString();
                                        var contents = response.Content.ReadAsStringAsync().Result;
                                        ePayResponse resp = JsonConvert.DeserializeObject<ePayResponse>(contents);

                                        log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> {contents}");

                                        if (resp.resulttext != "OK")
                                        {
                                            log.LogInformation(resp.resulttext);
                                        }
                                        else
                                        {
                                            log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> OK");
                                        }
                                        string dataMT = "";
                                        byte[] msgMT = System.Text.Encoding.ASCII.GetBytes(dataMT);
                                        if (!lTopupcard)
                                        {
                                            if (resp.PINCREDENTIALS != null)
                                            {
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> We have Pin data");
                                                dataMT = $"{(char)0}{(char)89}T{data.Substring(4, 8)}{msgno}1200{resp.txId}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)31}{resp.PINCREDENTIALS.pin.Trim()}" +
                                                 $"{(char)31}{resp.PINCREDENTIALS.serial}{(char)28}0";
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> reply ready to sent: {dataMT}");
                                            }
                                            else
                                            {
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> We do not have Pin data");
                                                dataMT = $"{(char)0}{(char)89}T{data.Substring(4, 8)}{msgno}1205{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)31}{(char)31}{(char)28}0";
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> reply ready to sent: {dataMT}");
                                            }
                                        }
                                        else
                                        {
                                            if (resp.resulttext == "OK")
                                            {
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> We have topup data");
                                                dataMT = $"{(char)0}{(char)89}T{data.Substring(4, 8)}{msgno}1200{resp.txId}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)31}" +
                                                 $"{(char)31}{(char)28}0";
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> reply ready to sent: {dataMT}");
                                            }
                                            else
                                            {
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> We do not have data");
                                                dataMT = $"{(char)0}{(char)89}T{data.Substring(4, 8)}{msgno}1205{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)31}{(char)31}{(char)28}0";
                                                log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> reply ready to sent: {dataMT}");
                                            }
                                        }
                                        log.LogInformation($"Sending Data: {threadID}{fields[1]}{msgno} --> {dataMT}");

                                        msgMT = System.Text.Encoding.ASCII.GetBytes(dataMT);
                                        stream.Write(msgMT, 0, msgMT.Length);

                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Sent: {dataMT}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} ....................................................................");
                                        log.LogInformation($"{threadID}{fields[1]}{msgno} *****      Transaction Finished      *****");
                                    }
                                    else // ICC Transactions
                                    {
                                        if (data == null)
                                            continue;

                                        TermId = data.Substring(7, 8);
                                        msgno = data.Substring(15, 4);

                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Merchant Code: {fields[1]}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Terminal Code: {TermId}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Provider Code: {fields[7]}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Product Code: {fields[2]}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Amount: {fields[5]}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} TXN: {msgno}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Original: {data}");

                                        HttpClient httpclient = new HttpClient();
                                        PinRequest pinrequest = new PinRequest();
                                        pinrequest.mid = int.Parse(fields[1]);
                                        pinrequest.tid = TermId;
                                        pinrequest.supplierCode = fields[7];
                                        pinrequest.productCode = fields[2];
                                        pinrequest.value = int.Parse(fields[5]);
                                        pinrequest.txnNO = data.Substring(15, 4);

                                        string json = JsonConvert.SerializeObject(pinrequest);

                                        StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                                        log.LogInformation($"Requesting Pin: {threadID}{fields[1]}{msgno}");

                                        var response = await httpclient.PostAsync("https://pins.3rtelecom.co.uk/api/api/voucher/", httpContent);
                                        //var contents = response.Result.ToString();
                                        var contents = response.Content.ReadAsStringAsync().Result;
                                        PinResponse resp = JsonConvert.DeserializeObject<PinResponse>(contents);

                                        log.LogInformation($"Received Response: {threadID}{fields[1]}{msgno} --> {contents}");

                                        string dataICC = "";
                                        byte[] msgICC = System.Text.Encoding.ASCII.GetBytes(dataICC);
                                        if (resp != null)
                                        {
                                            if (resp.code == 10)
                                            {
                                                dataICC = $"{(char)0}{(char)86}{(char)96}{(char)0}{(char)52}{(char)0}{(char)0}{TermId}{msgno}E2{resp.transactionId}{(char)28}{resp.code}{(char)28}" +
                                                     $"{(char)28}{resp.serial.Trim()}{(char)28}{resp.helplineNumber.Trim()}{(char)28}{resp.pin.Trim()}{(char)28}{resp.helplineNumber.Trim()}{(char)28}{resp.helplineNumber.Trim()}{(char)28}{resp.helplineNumber.Trim()}{(char)28}{(char)28}{fields[5]}{(char)28}" +
                                                     $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                                     $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                                     $"{resp.info.Trim()}{(char)28}{(char)28}{(char)28}{fields[5]}{(char)28}{(char)28}";
                                            }
                                            else
                                            {
                                                dataICC = $"{(char)0}{(char)86}{(char)96}{(char)0}{(char)52}{(char)0}{(char)0}{TermId}{msgno}E2{(char)28}32{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{fields[5]}{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{fields[5]}{(char)28}{(char)28}";
                                            }

                                        }
                                        else
                                        {
                                            dataICC = $"{(char)0}{(char)86}{(char)96}{(char)0}{(char)52}{(char)0}{(char)0}{TermId}{msgno}E2{(char)28}32{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{fields[5]}{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                                 $"{(char)28}{(char)28}{(char)28}{fields[5]}{(char)28}{(char)28}";

                                            //dataICC = $"{(char)0}{(char)86}{(char)96}{(char)0}{(char)52}{(char)0}{(char)0}01234567{msgno}E2986532{(char)28}10{(char)28}" +
                                            //         $"{(char)28}Serial0126789{(char)28}01992574650{(char)28}Pin0123456789{(char)28}07908740331{(char)28}07903587007{(char)28}0245646165{(char)28}{(char)28}{fields[5]}{(char)28}" +
                                            //         $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                            //         $"{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}{(char)28}" +
                                            //         $"Messagebox{(char)28}Msg2{(char)28}Msg3{(char)28}{fields[5]}{(char)28}220425093101{(char)28}";
                                        }


                                        msgICC = System.Text.Encoding.ASCII.GetBytes(dataICC);
                                        stream.Write(msgICC, 0, msgICC.Length);

                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} Sent: {dataICC}");
                                        log.LogInformation($"ID: {threadID}{fields[1]}{msgno} .....................................................................");
                                    }
                                }
                                else
                                {
                                    log.LogInformation("Data is Null");
                                }
                            }

                            client.Close();
                            log.LogInformation($"Connection Closed {threadID}");
                        });
                    }
                    else
                    {
                        log.LogInformation("No Terminal connected!");
                    }
                }

                catch (Exception e)
                {
                    log.LogError("SocketException: {0}", e.Message);
                }
            }

        }
    }
}