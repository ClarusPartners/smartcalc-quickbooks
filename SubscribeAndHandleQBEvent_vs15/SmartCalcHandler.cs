using Interop.QBFC16;
using Interop.QBXMLRP2;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SubscribeAndHandleQBEvent
{
    internal class SmartCalcHandler
    {
        

        public SmartCalcHandler()
        {
            if (File.Exists("config.txt"))
            {
                using (StreamReader reader = new StreamReader("config.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        switch (parts[0])
                        {
                            case "Username":
                                this.username = parts[1];
                                break;
                            case "Password":
                                this.password = parts[1];
                                break;
                            case "Endpoint":
                                this.endpoint = parts[1];
                                break;
                        }
                    }
                }
            }
        }
    

        private string username { get; set; }
        private string password { get; set; }
        private string endpoint { get; set; }
        
        
        private static void LogXmlData(string strFile, string strXML)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(strFile);
            sw.WriteLine(strXML);
            sw.Flush();
            sw.Close();
        }
        public static string QueryInvoice(string strData)
        {
            RequestProcessor2 qbRequestProcessor;
            try
            {
                // Get an instance of the qbXMLRP Request Processor and
                // call OpenConnection if that has not been done already.
                qbRequestProcessor = new RequestProcessor2();
                // string appName;
                qbRequestProcessor.OpenConnection("", "SmartCalc");
                string ticket = qbRequestProcessor.BeginSession("", QBFileMode.qbFileOpenDoNotCare);

                StringBuilder strRequest = new StringBuilder();
                strRequest = strRequest.Append(GetInvoiceQueryXML(strData));


                string strResponse = qbRequestProcessor.ProcessRequest(ticket, strRequest.ToString());
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\InvoiceQueryResponse.TXT"))
                {
                    writer.WriteLine(strResponse);
                }
                qbRequestProcessor.EndSession(ticket);
                qbRequestProcessor.CloseConnection();

                return strResponse;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error while registering for QB events - " + ex.Message);
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\InvoiceQueryResponse.TXT"))
                {
                    writer.WriteLine(ex.Message);
                }
                
                qbRequestProcessor = null;
                throw ex;
            }

        }
        public static string UpdateInvoice(string strData)
        {
            RequestProcessor2 qbRequestProcessor;
            try
            {
                // Get an instance of the qbXMLRP Request Processor and
                // call OpenConnection if that has not been done already.
                qbRequestProcessor = new RequestProcessor2();
                // string appName;
                qbRequestProcessor.OpenConnection("", "SmartCalc");
                string ticket = qbRequestProcessor.BeginSession("", QBFileMode.qbFileOpenDoNotCare);

                StringBuilder strRequest = new StringBuilder();
              //  strRequest = strRequest.Append(GetInvoiceQueryXML(strData));
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\InvoiceQueryUpdateXML.TXT"))
                {
                    writer.WriteLine(strData);
                }

                string strResponse = qbRequestProcessor.ProcessRequest(ticket, strData);
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\InvoiceQueryUpdateResponse.TXT"))
                {
                    writer.WriteLine(strResponse);
                }
                qbRequestProcessor.EndSession(ticket);
                qbRequestProcessor.CloseConnection();

                return strResponse;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error while registering for QB events - " + ex.Message);
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\InvoiceQueryUpdateResponse.TXT"))
                {
                    writer.WriteLine(ex.Message);
                }
                qbRequestProcessor = null;
                throw ex;
            }

        }
        public class InvoiceLine
        {
            public string invoice_line { get; set; }
            public string so_no { get; set; }
            public decimal quantity { get; set; }
            public decimal sales_amount { get; set; }
            public string discount_type { get; set; }
            public decimal discount_value { get; set; }
            public decimal sales_tax_invoice { get; set; }
            public string destination_street { get; set; }
            public string destination_city { get; set; }
            public string destination_state { get; set; }
            public string destination_zip { get; set; }
            public string destination_county { get; set; }
            public string destination_country { get; set; }
            public string location { get; set; }
            public string gl_account { get; set; }
            public string product { get; set; }
            public string product_code { get; set; }
            public string usage { get; set; }
            public string usage_code { get; set; }
        }


        public static List<InvoiceLine> ParseInvoiceResponse(string response)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Temp\PIR.TXT"))
            {
                writer.WriteLine(response);
            }
            List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
            XmlDocument xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(response);
            XmlNodeList invoiceNodes = xmlResponse.GetElementsByTagName("InvoiceRet");

            foreach (XmlNode invoiceNode in invoiceNodes)
            {
                XmlNodeList lineNodes = invoiceNode.SelectNodes("InvoiceLineRet");

                foreach (XmlNode lineNode in lineNodes)
                {
                    InvoiceLine invoiceLine = new InvoiceLine
                    {
                        invoice_line = lineNode.SelectSingleNode("ItemRef/ListID")?.InnerText ?? "",
                        so_no = invoiceNode.SelectSingleNode("PONumber")?.InnerText ?? "",
                        quantity = decimal.Parse(lineNode.SelectSingleNode("Quantity")?.InnerText ?? "0"),
                        sales_amount = decimal.Parse(lineNode.SelectSingleNode("Amount")?.InnerText ?? "0"),
                        discount_type = lineNode.SelectSingleNode("DiscountLineRet/DiscountLineType")?.InnerText ?? "",
                        discount_value = decimal.Parse(lineNode.SelectSingleNode("DiscountLineRet/DiscountLineAmount")?.InnerText ?? "0"),
                        sales_tax_invoice = decimal.Parse(lineNode.SelectSingleNode("SalesTaxLineRet/SalesTaxPercent")?.InnerText ?? "0"),
                        destination_street = invoiceNode.SelectSingleNode("ShipAddress/Addr1")?.InnerText ?? "",
                        destination_city = invoiceNode.SelectSingleNode("ShipAddress/City")?.InnerText ?? "",
                        destination_state = invoiceNode.SelectSingleNode("ShipAddress/State")?.InnerText ?? "",
                        destination_zip = invoiceNode.SelectSingleNode("ShipAddress/PostalCode")?.InnerText ?? "",
                        destination_county = invoiceNode.SelectSingleNode("ShipAddress/Country")?.InnerText ?? "",
                        destination_country = invoiceNode.SelectSingleNode("ShipAddress/County")?.InnerText ?? "",
                        location = invoiceNode.SelectSingleNode("ShipMethodRef/FullName")?.InnerText ?? "",
                        gl_account = lineNode.SelectSingleNode("ItemAccountRef/FullName")?.InnerText ?? "",
                        product = lineNode.SelectSingleNode("ItemRef/FullName")?.InnerText ?? "",
                        product_code = lineNode.SelectSingleNode("SalesItemLineRet/ItemRef/ListID")?.InnerText ?? "",
                        usage = lineNode.SelectSingleNode("SalesItemLineRet/ItemRef/FullName")?.InnerText ?? "",
                        usage_code = lineNode.SelectSingleNode("SalesItemLineRet/ItemRef/ListID")?.InnerText ?? "",
                    };

                    invoiceLines.Add(invoiceLine);
                }
            }

            return invoiceLines;
        }
        public async Task<decimal> GetSalesTaxFromSmartCalcAsync(string invoice)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Temp\hi.TXT"))
            {
                writer.WriteLine(invoice);
            }
            using (var client = new HttpClient())
            {
                // Set the base address of the API
                client.BaseAddress = new Uri(endpoint);
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\stc.TXT"))
                {
                    writer.WriteLine(invoice);
                }

                try
                {
                 

                    var baseURI = endpoint+"/service/v4_1/rest.php?method={0}&input_type={1}&response_type={2}&rest_data={3}";
                    var userJson = new { user_auth = new { user_name = username, password = this.password, version = "1", application_name = "Smart-Calc" } };
                    var authCredentials = JsonConvert.SerializeObject(userJson);
                    //   var auth = "{\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}";
                    var authURL = String.Format(baseURI, "auth_token", "JSON", "JSON", authCredentials);

                    // var rclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?method=auth_token&input_type=JSON&response_type=JSON&rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                    var rclient = new RestClient(authURL);
                    // rclient.Timeout = -1; 
                    var request = new RestRequest();

                   // request.AddHeader("Content-Type", "application/json");
                   // request.AddHeader("Cookie", "PHPSESSID=9bf6bngk1aspih5op58pnqte45");
                    //  var body = @"{" + "\n" + @"    ""method"":""login""," + "\n" + @"  ""input_type"":""JSON""," + "\n" + @"  ""response_type"":""JSON"", " + "\n" + @"}";
                    //   request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response =   await rclient.ExecuteAsync(request);
                    //Console.WriteLine(response.Content);
                    var result = response.Content;
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\stctRest.TXT"))
                    {
                        writer.WriteLine(authURL);
                        writer.WriteLine(response.StatusCode);
                        writer.WriteLine(response.Headers);
                        writer.WriteLine(response.Content);


                    }
                    // Deserialize the response content to a dynamic object
                    dynamic responseJson = JsonConvert.DeserializeObject(result);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\respJson.TXT"))
                    {
                        writer.WriteLine(result.ToString());
                    }
                    // Extract the authentication token from the response
                    //string authentication_token = responseJson.token_key;
                    string authentication_token = responseJson.token_key;
                    //Console.WriteLine("Authentication Token: " + authentication_token);

                    var smclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?" +
                        "method=auth_token&input_type=JSON&response_type=JSON&" +
                        "rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                    List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
                    XmlDocument xmlResponse = new XmlDocument();
                    xmlResponse.LoadXml(invoice);
                  
                    XmlNodeList invoiceNodes = xmlResponse.GetElementsByTagName("CustomerRef");
              

                    var jsonObject = new
                    {
                        session = authentication_token,
                        module_name = "T_Transaction_Test",
                        offset = "0",
                        invoice_general = new
                        {
                            taxpayer = "MSW Consulting LLC",
                            customer_no = invoiceNodes[0].SelectSingleNode("ListID")?.InnerText??"",
                            invoice_no = Guid.NewGuid().ToString(),
                            invoice_date = DateTime.Now.ToString("MM/dd/yyyy"),
                            transaction_date = DateTime.Now.ToString("MM/dd/yyyy"),
                            document_type = "SI",
                            origin_street = "5828 Zarley St Suite A",
                            origin_city = "New Albany",
                            origin_state = "OH",
                            origin_zip = "43054",
                            origin_county = "New Albany",
                            origin_country = "United States",
                            shipping_amt = "0"
                        },
                        invoice_line = JsonConvert.SerializeObject(ParseInvoiceResponse(invoice))
            
                    };

                    string invoiceJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
                    //Console.WriteLine(invoiceJson);
                    var invoicequeryURL = String.Format(baseURI, "tax_calculation", "JSON", "JSON", invoiceJson);
                    var invoiceClient = new RestClient(invoicequeryURL);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\invQueryUrl.TXT"))
                    {
                        writer.WriteLine(invoicequeryURL);
                    }
                    RestResponse INVresponse = invoiceClient.Execute(request);
                    //Console.WriteLine(INVresponse.Content);
                    var document = JsonDocument.Parse(INVresponse.Content);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\InvrespJson.TXT"))
                    {
                        writer.WriteLine(document.ToString());
                    }

                    var tax = document.RootElement.GetProperty("summary").GetProperty("total_sales_tax").GetDecimal();
                    return tax;


                }
                catch (Exception ex)
                {

                    //Console.WriteLine("Error: " + ex.Message + " GetSalesTaxFromSmartCalc(string invoice) ");
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\EXCEPTION.TXT"))
                    {
                        writer.WriteLine(ex.ToString());
                    }
                    return 0m;
                }
            }
        }

        public decimal GetSalesTaxFromSmartCalc(string invoice)
        {
            //Console.WriteLine("GetSalesTax");
            
            using (var client = new HttpClient())
            {
                // Set the base address of the API
                client.BaseAddress = new Uri("http://mswsmartcalc.suchimsapps.com/");
                using (StreamWriter writer = new StreamWriter(@"C:\Temp\stc.TXT"))
                {
                    writer.WriteLine(invoice);
                }

                try
                {


                    var baseURI = "http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?method={0}&input_type={1}&response_type={2}&rest_data={3}";
                    var userJson = new { user_auth = new { user_name = "qbapi", password = "f15df058bee598625a2762554488d903", version = "1", application_name = "Smart-Calc" } };
                    var authCredentials = JsonConvert.SerializeObject(userJson);
                    //  var auth = "{\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}";
                    var authURL = String.Format(baseURI, "auth_token", "JSON", "JSON", authCredentials);

                    // var rclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?method=auth_token&input_type=JSON&response_type=JSON&rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                    var rclient = new RestClient(authURL);
                    // rclient.Timeout = -1; 
                    var request = new RestRequest();

                    // request.AddHeader("Content-Type", "application/json");
                    // request.AddHeader("Cookie", "PHPSESSID=9bf6bngk1aspih5op58pnqte45");
                    //  var body = @"{" + "\n" + @"    ""method"":""login""," + "\n" + @"  ""input_type"":""JSON""," + "\n" + @"  ""response_type"":""JSON"", " + "\n" + @"}";
                    //   request.AddParameter("application/json", body, ParameterType.RequestBody);
                    var response = rclient.Execute(request);
                    //Console.WriteLine(response.Content);
                    var result = response.Content;
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\stctRest.TXT"))
                    {
                        writer.WriteLine(authURL);
                        writer.WriteLine(response.StatusCode);
                        writer.WriteLine(response.Headers);
                        writer.WriteLine(response.Content);


                    }
                    // Deserialize the response content to a dynamic object
                    dynamic responseJson = JsonConvert.DeserializeObject(result);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\respJson.TXT"))
                    {
                        writer.WriteLine(result.ToString());
                    }
                    // Extract the authentication token from the response
                    //string authentication_token = responseJson.token_key;
                    string authentication_token = responseJson.token_key;
                    //Console.WriteLine("Authentication Token: " + authentication_token);

                    var smclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?" +
                        "method=auth_token&input_type=JSON&response_type=JSON&" +
                        "rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                    List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
                    XmlDocument xmlResponse = new XmlDocument();
                    xmlResponse.LoadXml(invoice);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\sssTFC.TXT"))
                    {
                        writer.WriteLine(invoice);
                    }
                    XmlNodeList invoiceNodes = xmlResponse.GetElementsByTagName("CustomerRef");
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\TTFC.TXT"))
                    {
                        writer.WriteLine(invoice);
                    }

                    var jsonObject = new
                    {
                        session = authentication_token,
                        module_name = "T_Transaction_Test",
                        offset = "0",
                        invoice_general = new
                        {
                            taxpayer = "MSW Consulting LLC",
                            customer_no = invoiceNodes[0].SelectSingleNode("ListID")?.InnerText ?? "",
                            invoice_no = Guid.NewGuid().ToString(),
                            invoice_date = DateTime.Now.ToString("MM/dd/yyyy"),
                            transaction_date = DateTime.Now.ToString("MM/dd/yyyy"),
                            document_type = "SI",
                            origin_street = "5828 Zarley St Suite A",
                            origin_city = "New Albany",
                            origin_state = "OH",
                            origin_zip = "43054",
                            origin_county = "New Albany",
                            origin_country = "United States",
                            shipping_amt = "0"
                        },
                        invoice_line = ParseInvoiceResponse(invoice)

                    };

                    string invoiceJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
                    //Console.WriteLine(invoiceJson);
                    var invoicequeryURL = String.Format(baseURI, "tax_calculation", "JSON", "JSON", invoiceJson);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\SCRespURL.TXT"))
                    {
                        writer.WriteLine(invoicequeryURL);
                    }
                    // request.AddHeader("Content-Type", "application/json");
                    var invoiceClient = new RestClient(invoicequeryURL);
                    var invoiceRequest = new RestRequest();
                    request.AddHeader("Content-Type", "application/json");
                    RestResponse INVresponse = invoiceClient.Execute(invoiceRequest);
                    var document = JsonDocument.Parse(INVresponse.Content);
                    using (StreamWriter writer = new StreamWriter(@"C:\Temp\SCResp.TXT"))
                    {
                        writer.WriteLine(INVresponse.Content);
                    }
                    var tax = document.RootElement.GetProperty("summary").GetProperty("total_sales_tax").GetDecimal();
                    //Console.WriteLine(tax);
                    return tax;


                }
                catch (Exception ex)
                {

                    //Console.WriteLine("Error: " + ex.Message + " GetSalesTaxFromSmartCalc(string invoice) ");
                    return 0m;
                }
            }
        }

        public static string GetInvoiceTaxUpdateXML(string invoice, decimal tax)
        {
            throw new NotImplementedException();
        }

        public static string GetInvoiceQueryXML(string invoiceNumber)
        {
            XmlDocument qbXMLDoc = new XmlDocument();
            qbXMLDoc.AppendChild(qbXMLDoc.CreateXmlDeclaration("1.0", null, null));
            qbXMLDoc.AppendChild(qbXMLDoc.CreateProcessingInstruction("qbxml", "version=\"5.0\""));
            // Create the QBXML request
            XmlElement qbxml = qbXMLDoc.CreateElement("QBXML");
            qbXMLDoc.AppendChild(qbxml);

            // Create the QBXMLMsgsRq element
            XmlElement qbxmlMsgsRq = qbXMLDoc.CreateElement("QBXMLMsgsRq");
            qbxml.AppendChild(qbxmlMsgsRq);
            qbxmlMsgsRq.SetAttribute("onError", "stopOnError");

            // Create the InvoiceQueryRq element
            XmlElement invoiceQueryRq = qbXMLDoc.CreateElement("InvoiceQueryRq");
            qbxmlMsgsRq.AppendChild(invoiceQueryRq);
            invoiceQueryRq.SetAttribute("requestID", "1");

            // Create the RefNumber element
            XmlElement refNumber = qbXMLDoc.CreateElement("RefNumber");
            invoiceQueryRq.AppendChild(refNumber);
            refNumber.InnerText = invoiceNumber;

            XmlElement lineItem = qbXMLDoc.CreateElement("IncludeLineItems");
            invoiceQueryRq.AppendChild(lineItem);
            lineItem.InnerText = "true";
            LogXmlData(@"C:\Temp\InvoiceQuery.xml", qbXMLDoc.OuterXml);
            return qbXMLDoc.OuterXml;
        }

        internal static string GenerateInvoiceTaxUpdateXML(string invoice, decimal amount)
        {
            //Console.WriteLine("GenerateInvTaxUpd");
            try
            {
                XmlDocument xmlResponse = new XmlDocument();
                xmlResponse.LoadXml(invoice);
                XmlNode invoiceNode = xmlResponse.SelectSingleNode("//InvoiceRet");

                XmlNodeList lineNodes = invoiceNode.SelectNodes("InvoiceLineRet");

                //Console.WriteLine("lineretd");


                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
                doc.AppendChild(doc.CreateProcessingInstruction("qbxml", "version=\"5.0\""));
                XmlElement qbxml = doc.CreateElement("QBXML");
                doc.AppendChild(qbxml);

                XmlElement msgsRq = doc.CreateElement("QBXMLMsgsRq");
                msgsRq.SetAttribute("onError", "stopOnError");
                qbxml.AppendChild(msgsRq);
                msgsRq.SetAttribute("onError", "stopOnError");

                XmlElement invoiceModRq = doc.CreateElement("InvoiceModRq");
                invoiceModRq.SetAttribute("requestID", "1");
                msgsRq.AppendChild(invoiceModRq);

                XmlElement invoiceMod = doc.CreateElement("InvoiceMod");
                invoiceModRq.AppendChild(invoiceMod);

                XmlElement txnIDElement = doc.CreateElement("TxnID");
                txnIDElement.InnerText = invoiceNode.SelectSingleNode("TxnID").InnerText;
                invoiceMod.AppendChild(txnIDElement);

                XmlElement editSequence = doc.CreateElement("EditSequence");
                editSequence.InnerText = invoiceNode.SelectSingleNode("EditSequence").InnerText;
                invoiceMod.AppendChild(editSequence);

                XmlElement itemSalesTaxRef = doc.CreateElement("ItemSalesTaxRef");
                invoiceMod.AppendChild(itemSalesTaxRef);

                XmlElement fullName = doc.CreateElement("FullName");
                fullName.InnerText = "Tax Calculated on Invoice";
                itemSalesTaxRef.AppendChild(fullName);


                foreach (XmlNode lineNode in lineNodes)
                {
                    XmlElement invoiceLineMod1 = doc.CreateElement("InvoiceLineMod");
                    invoiceMod.AppendChild(invoiceLineMod1);
                    XmlElement txnLineID1 = doc.CreateElement("TxnLineID");
                    txnLineID1.InnerText = lineNode.SelectSingleNode("TxnLineID").InnerText;
                    invoiceLineMod1.AppendChild(txnLineID1);
                }

                XmlElement invoiceLineMod2 = doc.CreateElement("InvoiceLineMod");
                invoiceMod.AppendChild(invoiceLineMod2);

                XmlElement txnLineID2 = doc.CreateElement("TxnLineID");
                txnLineID2.InnerText = "-1";
                invoiceLineMod2.AppendChild(txnLineID2);

                XmlElement itemRef = doc.CreateElement("ItemRef");
                invoiceLineMod2.AppendChild(itemRef);

                XmlElement itemFullName = doc.CreateElement("FullName");
                itemFullName.InnerText = "SmartCalc";
                itemRef.AppendChild(itemFullName);

                XmlElement amountElement = doc.CreateElement("Amount");
                amountElement.InnerText = amount.ToString("F");
                invoiceLineMod2.AppendChild(amountElement);
                //Console.WriteLine(doc.OuterXml);
                LogXmlData(@"C:\Temp\InvoiceUpdateQuery.xml", doc.OuterXml);
                return doc.OuterXml;
            }
            catch( Exception ex)
            {
                //Console.WriteLine("Exception"+ex);
                throw ex;
            }

            
                
        }
    }
}
