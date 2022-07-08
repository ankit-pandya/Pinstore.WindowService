using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace testAp
{
	class Program
	{
		private static readonly HttpClient client = new HttpClient();

		public static async Task Main(string[] args)
		{

				Console.WriteLine($"Starting ");
                await Task.Run(() =>
                {
                    PinRequest a = new PinRequest();
                    a.mid = 288442;
                    a.tid = "01234567";
                    a.txnNO = 1234;
                    a.supplierCode = "100000";
                    a.productCode = "4300000";
                    a.value = 0;

                    var json = JsonSerializer.Serialize(a);
                    byte[] byteArray = Encoding.UTF8.GetBytes(json);

                    var url = "https://localhost:44395/api/voucher";

                    var request = WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.ContentLength = byteArray.Length;

                    using var reqStream = request.GetRequestStream();
                    reqStream.Write(byteArray, 0, byteArray.Length);

                    using var response = request.GetResponse();
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    using var respStream = response.GetResponseStream();

                    using var reader = new StreamReader(respStream);
                    string data = reader.ReadToEnd();
                    Console.WriteLine(data);
                    Console.ReadKey();
                }
               );
  
			

		}
		public class PinRequest
		{
			public int mid { get; set; }
			public string tid { get; set; }
			public int txnNO { get; set; }
			public string supplierCode { get; set; }
			public string productCode { get; set; }
			public decimal value { get; set; }
		}
	}

	}
