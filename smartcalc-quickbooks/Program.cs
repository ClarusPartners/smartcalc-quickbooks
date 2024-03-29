﻿// See https://aka.ms/new-console-template for more information

using Interop.QBFC15;
using System.Net.Sockets;
using Interop.QBXMLRP2;
using System.Xml;
using System.Text;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using smartcalc_quickbooks;
using Newtonsoft.Json;
using RestSharp;
using System.Text.Json;
using smartcalc_quickbooks;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace smartcalc_quickbooks;

public class SmartCalcConnect
{

    private static string appID = "SC123";
    private static string appName = "SmartCalc";
    private static RequestProcessor2 rp;
    private static string maxVersion;
    private static string ticket;
    private static string companyFile = "";
    private static QBFileMode mode = QBFileMode.qbFileOpenDoNotCare;
   // private static ConcurrentQueue<SmartCalcEvent> eventQueue = new ConcurrentQueue<SmartCalcEvent>();
    private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    enum QBSubscriptionType { Data, UI, UIExtension };


    private void connectToQB()
    {
        rp = new RequestProcessor2Class();
        rp.OpenConnection(appID, appName);
        ticket = rp.BeginSession(companyFile, mode);
        string[] versions = rp.get_QBXMLVersionsForSession(ticket);
        maxVersion = versions[versions.Length - 1];

        rp.EndSession(ticket);
    }
    public virtual string buildDataCountQuery(string request)
    {
        string input = "";
        XmlDocument inputXMLDoc = new XmlDocument();
        XmlElement qbXMLMsgsRq = buildRqEnvelope(inputXMLDoc, maxVersion);
        XmlElement queryRq = inputXMLDoc.CreateElement(request);
        queryRq.SetAttribute("metaData", "MetaDataOnly");
        qbXMLMsgsRq.AppendChild(queryRq);
        input = inputXMLDoc.OuterXml;
        return input;
    }
    private static string GetDataEventSubscriptionAddXML()
    {
        //Create the qbXML request
        XmlDocument requestXMLDoc = new XmlDocument();
        requestXMLDoc.AppendChild(requestXMLDoc.CreateXmlDeclaration("1.0", null, null));
        requestXMLDoc.AppendChild(requestXMLDoc.CreateProcessingInstruction("qbxml", "version=\"5.0\""));
        XmlElement qbXML = requestXMLDoc.CreateElement("QBXML");
        requestXMLDoc.AppendChild(qbXML);

        //subscription Message request
        XmlElement qbXMLMsgsRq = requestXMLDoc.CreateElement("QBXMLSubscriptionMsgsRq");
        qbXML.AppendChild(qbXMLMsgsRq);

        //Data Event Subscription ADD request
        XmlElement dataEventSubscriptionAddRq = requestXMLDoc.CreateElement("DataEventSubscriptionAddRq");
        qbXMLMsgsRq.AppendChild(dataEventSubscriptionAddRq);


        //Data Event Subscription ADD
        XmlElement dataEventSubscriptionAdd = requestXMLDoc.CreateElement("DataEventSubscriptionAdd");
        dataEventSubscriptionAddRq.AppendChild(dataEventSubscriptionAdd);

        //Add Subscription ID
        dataEventSubscriptionAdd.AppendChild(requestXMLDoc.CreateElement("SubscriberID")).InnerText = "{8327c7fc-7f05-41ed-a5b4-b6618bb27bf1}";

        //Add COM CallbackInfo
        XmlElement comCallbackInfo = requestXMLDoc.CreateElement("COMCallbackInfo");
        dataEventSubscriptionAdd.AppendChild(comCallbackInfo);

        //Appname and CLSID
        comCallbackInfo.AppendChild(requestXMLDoc.CreateElement("AppName")).InnerText = appName;
        comCallbackInfo.AppendChild(requestXMLDoc.CreateElement("CLSID")).InnerText = "{62447F81-C195-446f-8201-94F0614E49D5}";

        //Delivery Policy
        dataEventSubscriptionAdd.AppendChild(requestXMLDoc.CreateElement("DeliveryPolicy")).InnerText = "DeliverAlways";

        //track lost events
        dataEventSubscriptionAdd.AppendChild(requestXMLDoc.CreateElement("TrackLostEvents")).InnerText = "All";


        //  ListEventSubscription
        XmlElement listEventSubscription = requestXMLDoc.CreateElement("ListEventSubscription");
        dataEventSubscriptionAdd.AppendChild(listEventSubscription);

        //Add Customer List and operations
        listEventSubscription.AppendChild(requestXMLDoc.CreateElement("ListEventType")).InnerText = "Invoice";
        listEventSubscription.AppendChild(requestXMLDoc.CreateElement("ListEventOperation")).InnerText = "Add";
        listEventSubscription.AppendChild(requestXMLDoc.CreateElement("ListEventOperation")).InnerText = "Modify";
        listEventSubscription.AppendChild(requestXMLDoc.CreateElement("ListEventOperation")).InnerText = "Delete";

        string strRetString = requestXMLDoc.OuterXml;
        LogXmlData(@"DataEvent.xml", strRetString);
        return strRetString;

    }
    private static void LogXmlData(string strFile, string strXML)
    {
        var dir = System.IO.Directory.GetCurrentDirectory();
        var filepath = Path.Combine(dir, strFile);
        System.IO.StreamWriter sw = new System.IO.StreamWriter(filepath);
        sw.WriteLine(strXML);
        sw.Flush();
        sw.Close();
    }
    private static string GetUIExtensionSubscriptionAddXML(string strMenuName)
    {
        //Create the qbXML request
        XmlDocument requestXMLDoc = new XmlDocument();
        requestXMLDoc.AppendChild(requestXMLDoc.CreateXmlDeclaration("1.0", null, null));
        requestXMLDoc.AppendChild(requestXMLDoc.CreateProcessingInstruction("qbxml", "version=\"5.0\""));
        XmlElement qbXML = requestXMLDoc.CreateElement("QBXML");
        requestXMLDoc.AppendChild(qbXML);

        //subscription Message request
        XmlElement qbXMLMsgsRq = requestXMLDoc.CreateElement("QBXMLSubscriptionMsgsRq");
        qbXML.AppendChild(qbXMLMsgsRq);

        //UI Extension Subscription ADD request
        XmlElement uiExtSubscriptionAddRq = requestXMLDoc.CreateElement("UIExtensionSubscriptionAddRq");
        qbXMLMsgsRq.AppendChild(uiExtSubscriptionAddRq);


        //UI Extension Subscription ADD
        XmlElement uiExtEventSubscriptionAdd = requestXMLDoc.CreateElement("UIExtensionSubscriptionAdd");
        uiExtSubscriptionAddRq.AppendChild(uiExtEventSubscriptionAdd);

        //Add Subscription ID
        uiExtEventSubscriptionAdd.AppendChild(requestXMLDoc.CreateElement("SubscriberID")).InnerText = "{8327c7fc-7f05-41ed-a5b4-b6618bb27bf1}";

        //Add COM CallbackInfo
        XmlElement comCallbackInfo = requestXMLDoc.CreateElement("COMCallbackInfo");
        uiExtEventSubscriptionAdd.AppendChild(comCallbackInfo);

        //Appname and CLSID
        comCallbackInfo.AppendChild(requestXMLDoc.CreateElement("AppName")).InnerText = appName;
        comCallbackInfo.AppendChild(requestXMLDoc.CreateElement("CLSID")).InnerText = "{62447F81-C195-446f-8201-94F0614E49D5}";


        //  MenuEventSubscription
        XmlElement menuExtensionSubscription = requestXMLDoc.CreateElement("MenuExtensionSubscription");
        uiExtEventSubscriptionAdd.AppendChild(menuExtensionSubscription);

        //Add To menu Item // To Cusomter Menu
        menuExtensionSubscription.AppendChild(requestXMLDoc.CreateElement("AddToMenu")).InnerText = "Customers";


        XmlElement menuItem = requestXMLDoc.CreateElement("MenuItem");
        menuExtensionSubscription.AppendChild(menuItem);

        //Add Menu Name
        menuItem.AppendChild(requestXMLDoc.CreateElement("MenuText")).InnerText = strMenuName;
        menuItem.AppendChild(requestXMLDoc.CreateElement("EventTag")).InnerText = "menu_" + strMenuName;


        XmlElement displayCondition = requestXMLDoc.CreateElement("DisplayCondition");
        menuItem.AppendChild(displayCondition);

        displayCondition.AppendChild(requestXMLDoc.CreateElement("VisibleIf")).InnerText = "HasCustomers";
        displayCondition.AppendChild(requestXMLDoc.CreateElement("EnabledIf")).InnerText = "HasCustomers";


        string strRetString = requestXMLDoc.OuterXml;
        LogXmlData(@"UIExtension.xml", strRetString);
        return strRetString;

    }

    private static void SubscribeForEvents(QBSubscriptionType strType, string strData)
    {
        RequestProcessor2 qbRequestProcessor;
        try
        {
            // Get an instance of the qbXMLRP Request Processor and
            // call OpenConnection if that has not been done already.
            qbRequestProcessor = new RequestProcessor2();
            // string appName;
            qbRequestProcessor.OpenConnection("", appName);

            StringBuilder strRequest = new StringBuilder();
            switch (strType)
            {
                case QBSubscriptionType.Data:
                    strRequest = new StringBuilder(GetDataEventSubscriptionAddXML());
                    break;

                case QBSubscriptionType.UIExtension:
                    strRequest = new StringBuilder(GetUIExtensionSubscriptionAddXML(strData));
                    break;

                default:
                    return;
            }

            string strResponse = qbRequestProcessor.ProcessSubscription(strRequest.ToString());

            XmlDocument outputXMLDoc = new XmlDocument();
            outputXMLDoc.LoadXml(strResponse);
            XmlNodeList qbXMLMsgsRsNodeList = outputXMLDoc.GetElementsByTagName("DataEventSubscriptionAddRs");
            if (qbXMLMsgsRsNodeList.Count == 1)
            {
                XmlAttributeCollection rsAttributes = qbXMLMsgsRsNodeList.Item(0).Attributes;
                //get the status Code, info and Severity
                string retStatusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string retStatusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string retStatusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                if ((retStatusCode != "0") && (retStatusCode != "3180"))// 3180 : if subscription already subscribed. NOT A NEAT WAY TO DO THIS, NEED TO EXPLORE THIS
                {
                    //Console.WriteLine("Error while subscribing for events\n\terror Code - {0},\n\tSeverity - {1},\n\tError Message - {2}\n", retStatusCode, retStatusSeverity, retStatusMessage);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine("Error while registering for QB events - " + ex.Message);
            qbRequestProcessor = null;
            return;
        }
    }
    private string[] parseCustomerQueryRs(string xml, int count)
    {
        /*
         <?xml version="1.0" ?> 
         <QBXML>
         <QBXMLMsgsRs>
         <CustomerQueryRs requestID="1" statusCode="0" statusSeverity="Info" statusMessage="Status OK">
             <CustomerRet>
                 <FullName>Abercrombie, Kristy</FullName> 
             </CustomerRet>
         </CustomerQueryRs>
         </QBXMLMsgsRs>
         </QBXML>    
        */

        string[] retVal = new string[count];
        System.IO.StringReader rdr = new System.IO.StringReader(xml);
        System.Xml.XPath.XPathDocument doc = new System.Xml.XPath.XPathDocument(rdr);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();

        if (nav != null)
        {
            nav.MoveToFirstChild();
        }
        bool more = true;
        int x = 0;
        while (more)
        {
            switch (nav.LocalName)
            {
                case "QBXML":
                    more = nav.MoveToFirstChild();
                    continue;
                case "QBXMLMsgsRs":
                    more = nav.MoveToFirstChild();
                    continue;
                case "CustomerQueryRs":
                    more = nav.MoveToFirstChild();
                    continue;
                case "CustomerRet":
                    more = nav.MoveToFirstChild();
                    continue;
                case "FullName":
                    retVal[x] = nav.Value.Trim();
                    x++;
                    more = nav.MoveToParent();
                    more = nav.MoveToNext();
                    continue;
                case "BillAddress":
                case "ShipAddress":
                case "CurrencyRef":
                    more = nav.MoveToFirstChild();
                    continue;
                case "Addr1":
                case "Addr2":
                case "Addr3":
                case "Addr4":
                case "Addr5":
                case "City":
                case "State":
                case "PostalCode":
                    retVal[x] = retVal[x] + "\r\n" + nav.Value.Trim();
                    more = nav.MoveToNext();
                    continue;
                default:
                    more = nav.MoveToNext();
                    continue;
            }
        }
        return retVal;
    }

    private XmlElement buildRqEnvelope(XmlDocument doc, string maxVer)
    {
        doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
        doc.AppendChild(doc.CreateProcessingInstruction("qbxml", "version=\"" + maxVer + "\""));
        XmlElement qbXML = doc.CreateElement("QBXML");
        doc.AppendChild(qbXML);
        XmlElement qbXMLMsgsRq = doc.CreateElement("QBXMLMsgsRq");
        qbXML.AppendChild(qbXMLMsgsRq);
        qbXMLMsgsRq.SetAttribute("onError", "stopOnError");
        return qbXMLMsgsRq;
    }
    public virtual int parseRsForCount(string xml, string request)
    {
        int ret = -1;
        try
        {
            XmlNodeList RsNodeList = null;
            XmlDocument Doc = new XmlDocument();
            Doc.LoadXml(xml);
            string tagname = request.Replace("Rq", "Rs");
            RsNodeList = Doc.GetElementsByTagName(tagname);
            System.Text.StringBuilder popupMessage = new System.Text.StringBuilder();
            XmlAttributeCollection rsAttributes = RsNodeList.Item(0).Attributes;
            XmlNode retCount = rsAttributes.GetNamedItem("retCount");
            ret = Convert.ToInt32(retCount.Value);
        }
        catch (Exception e)
        {
            //Console.WriteLine(e.Message);
            ret = -1;
        }
        return ret;
    }
    private string buildCustomerQueryRqXML(string[] includeRetElement, string fullName)
    {
        string xml = "";
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement qbXMLMsgsRq = buildRqEnvelope(xmlDoc, maxVersion);
        qbXMLMsgsRq.SetAttribute("onError", "stopOnError");
        XmlElement CustomerQueryRq = xmlDoc.CreateElement("CustomerQueryRq");
        qbXMLMsgsRq.AppendChild(CustomerQueryRq);
        if (fullName != null)
        {
            XmlElement fullNameElement = xmlDoc.CreateElement("FullName");
            CustomerQueryRq.AppendChild(fullNameElement).InnerText = fullName;
        }
        for (int x = 0; x < includeRetElement.Length; x++)
        {
            XmlElement includeRet = xmlDoc.CreateElement("IncludeRetElement");
            CustomerQueryRq.AppendChild(includeRet).InnerText = includeRetElement[x];
        }
        CustomerQueryRq.SetAttribute("requestID", "1");
        xml = xmlDoc.OuterXml;
        return xml;
    }
    private string[] parseCustomerMsgQueryRs(string xml, int count)
    {
        string[] retVal = new string[count];
        System.IO.StringReader rdr = new System.IO.StringReader(xml);
        System.Xml.XPath.XPathDocument doc = new System.Xml.XPath.XPathDocument(rdr);
        System.Xml.XPath.XPathNavigator nav = doc.CreateNavigator();

        if (nav != null)
        {
            nav.MoveToFirstChild();
        }
        bool more = true;
        int x = 0;
        while (more)
        {
            switch (nav.LocalName)
            {
                case "QBXML":
                    more = nav.MoveToFirstChild();
                    continue;
                case "QBXMLMsgsRs":
                    more = nav.MoveToFirstChild();
                    continue;
                case "CustomerMsgQueryRs":
                    more = nav.MoveToFirstChild();
                    continue;
                case "CustomerMsgRet":
                    more = nav.MoveToFirstChild();
                    continue;
                case "Name":
                    retVal[x] = nav.Value.Trim();
                    x++;
                    more = nav.MoveToParent();
                    more = nav.MoveToNext();
                    continue;
                default:
                    more = nav.MoveToNext();
                    continue;
            }
        }
        return retVal;
    }

    private string processRequestFromQB(string request)
    {
        try
        {
            return rp.ProcessRequest(ticket, request);
        }
        catch (Exception e)
        {
            //Console.WriteLine("An Exception has occurred: " + e.Message);
            return null;
        }
    }
    private int getCount(string request)
    {
        string response = processRequestFromQB(buildDataCountQuery(request));
        int count = parseRsForCount(response, request);
        return count;
    }
    private void loadCustomers()
    {
        string request = "CustomerQueryRq";
        connectToQB();
        int count = getCount(request);
        string response = processRequestFromQB(buildCustomerQueryRqXML(new string[] { "FullName" }, null));
        string[] customerList = parseCustomerQueryRs(response, count);
        disconnectFromQB();
        //Console.WriteLine(response, count);
    }
    private void disconnectFromQB()
    {
        if (ticket != null)
        {
            try
            {
                rp.EndSession(ticket);
                ticket = null;
                rp.CloseConnection();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }
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
                    product_code = lineNode.SelectSingleNode("ItemRef/ListID")?.InnerText ?? "",
                    usage = "",
                    usage_code =  "",
                };

                invoiceLines.Add(invoiceLine);
            }
        }

        return invoiceLines;
    }
    public static async Task Main(string[] args)
    {
        List<Dictionary<string, string>> csvData = new List<Dictionary<string, string>>();
         
        using (var tclient = new HttpClient())
        {
            // Set the base address of the API
            tclient.BaseAddress = new Uri("http://mswsmartcalc.suchimsapps.com/");
            using (StreamReader reader = new StreamReader("transactiondata.csv"))
            {
                // Read the header line
                string[] headers = reader.ReadLine().Split(',');

                while (!reader.EndOfStream)
                {
                    string[] values = reader.ReadLine().Split(',');
                    Dictionary<string, string> row = new Dictionary<string, string>();

                    for (int i = 0; i < headers.Length; i++)
                    {
                        row.Add(headers[i], values[i]);
                    }

                    csvData.Add(row);
                }
            }
            var xml = @"<?xml version=""1.0"" ?>
<QBXML>
<QBXMLMsgsRs>
<InvoiceQueryRs requestID=""1"" statusCode=""0"" statusSeverity=""Info"" statusMessage=""Status OK"">
<InvoiceRet>
<TxnID>3379E-1797338703</TxnID>
<TimeCreated>2026-12-15T07:45:03-05:00</TimeCreated>
<TimeModified>2026-12-15T07:45:03-05:00</TimeModified>
<EditSequence>1797338703</EditSequence>
<TxnNumber>2578</TxnNumber>
<CustomerRef>
<ListID>80000-852380792</ListID>
<FullName>Campbell, Heather</FullName>
</CustomerRef>
<ARAccountRef>
<ListID>4A0000-852369473</ListID>
<FullName>Accounts Receivable</FullName>
</ARAccountRef>
<TemplateRef>
<ListID>10000-852029220</ListID>
<FullName>Intuit Product Invoice</FullName>
</TemplateRef>
<TxnDate>2026-12-15</TxnDate>
<RefNumber>TEST1224</RefNumber>
<BillAddress>
<Addr1>Heather Campbell</Addr1>
<Addr2>2950 Harley Ave.</Addr2>
<City>Middlefield</City>
<State>CA</State>
<PostalCode>94482</PostalCode>
</BillAddress>
<ShipAddress>
<Addr1>Heather Campbell</Addr1>
<Addr2>2950 Harley Ave.</Addr2>
<City>Middlefield</City>
<State>CA</State>
<PostalCode>94482</PostalCode>
</ShipAddress>
<IsPending>false</IsPending>
<IsFinanceCharge>false</IsFinanceCharge>
<TermsRef>
<ListID>40000-852029282</ListID>
<FullName>Due on receipt</FullName>
</TermsRef>
<DueDate>2026-12-15</DueDate>
<ShipDate>2026-12-15</ShipDate>
<Subtotal>0.00</Subtotal>
<ItemSalesTaxRef>
<ListID>8000003D-1797361823</ListID>
<FullName>Tax Calculated on Invoice</FullName>
</ItemSalesTaxRef>
<SalesTaxPercentage>0.00</SalesTaxPercentage>
<SalesTaxTotal>0.00</SalesTaxTotal>
<AppliedAmount>0.00</AppliedAmount>
<BalanceRemaining>0.00</BalanceRemaining>
<IsPaid>true</IsPaid>
<IsToBePrinted>false</IsToBePrinted>
<CustomerSalesTaxCodeRef>
<ListID>10000-1004660652</ListID>
<FullName>Tax</FullName>
</CustomerSalesTaxCodeRef>
<InvoiceLineRet>
<TxnLineID>337A0-1797338703</TxnLineID>
<ItemRef>
<ListID>320001-973085832</ListID>
<FullName>01 Plans &amp; Permits  .:01.3 City &amp; Co. Lic&apos;s &amp; Fees</FullName>
</ItemRef>
<Desc>City &amp; County Licenses &amp; Fees</Desc>
<Quantity>3</Quantity>
<Rate>0.00</Rate>
<Amount>0.00</Amount>
<SalesTaxCodeRef>
<ListID>20000-1004660652</ListID>
<FullName>Non</FullName>
</SalesTaxCodeRef>
</InvoiceLineRet>
</InvoiceRet>
</InvoiceQueryRs>
</QBXMLMsgsRs>
</QBXML>
";

            XmlDocument xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(xml);
            var invoice_no = xmlResponse.GetElementsByTagName("RefNumber")[0].InnerText;
            try
            {

              

                using (var client = new HttpClient())
                {
                    // Set the base address of the API
                    client.BaseAddress = new Uri("http://mswsmartcalc.suchimsapps.com/");

                    try
                    {
                        var baseURI = "http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?method={0}&input_type={1}&response_type={2}&rest_data={3}";
                        var userJson = new { user_auth = new { user_name = "qbapi", password = "f15df058bee598625a2762554488d903", version = "1", application_name = "Smart-Calc" } };
                        var authCredentials = JsonConvert.SerializeObject(userJson);
                        //   var auth = "{\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}";
                        var authURL = String.Format(baseURI, "auth_token", "JSON", "JSON", authCredentials);

                        // var rclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?method=auth_token&input_type=JSON&response_type=JSON&rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                        var rclient = new RestClient(authURL);
                        //  rclient.Timeout = -1; 
                        var request = new RestRequest();

                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Cookie", "PHPSESSID=9bf6bngk1aspih5op58pnqte45");
                        //  var body = @"{" + "\n" + @"    ""method"":""login""," + "\n" + @"  ""input_type"":""JSON""," + "\n" + @"  ""response_type"":""JSON"", " + "\n" + @"}";
                        //   request.AddParameter("application/json", body, ParameterType.RequestBody);
                        RestResponse response = rclient.Execute(request);
                        //Console.WriteLine(response.Content);
                        using (StreamWriter writer = new StreamWriter(Path.Combine(Path.GetTempPath(), $"TESTRESPONSE.txt")))
                        {
                            writer.WriteLine(authURL);
                            writer.WriteLine(response.StatusCode);
                            writer.WriteLine(response.Headers);
                            writer.WriteLine(response.Content);

                        }
                        string? result = response.Content;

                        // Deserialize the response content to a dynamic object
                        dynamic responseJson = JsonConvert.DeserializeObject(result);
                                                    
                        // Extract the authentication token from the response
                        string authentication_token = responseJson.token_key;

                        //Console.WriteLine("Authentication Token: " + authentication_token);

                        var smclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?" +
                            "method=auth_token&input_type=JSON&response_type=JSON&" +
                            "rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                        var backup = new[] {
                new {
                    invoice_line = "1",
                    so_no = "123",
                    quantity = "1",
                    sales_amount = "200",
                    discount_type = "%",
                    discount_value = "20",
                    sales_tax_invoice = "0",
                    destination_street = "1734 Richmond Ave",
                    destination_city = "Columbus",
                    destination_state = "OHIO",
                    destination_zip = "43203",
                    destination_county = "",
                    destination_country = "",
                    location = "0",
                    gl_account = "1000- Accounts Receivable",
                    product = "",
                    product_code = "",
                    usage = "",
                    usage_code = ""
                }
            };
                        List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
                       
                        var jsonObject = new
                        {
                            session = authentication_token,
                            module_name = "T_Transaction_Test",
                            offset = "0",
                            invoice_general = new
                            {
                                taxpayer = "MSW Consulting LLC",
                                customer_no ="3333",
                                invoice_no ="1234567" ,
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
                            invoice_line = backup
                        };

                        string invoiceJson = JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);
                        //Console.WriteLine(invoiceJson);
                        var invoicequeryURL = String.Format(baseURI, "tax_calculation", "JSON", "JSON", invoiceJson);
                        var invoiceClient = new RestClient(invoicequeryURL);
                        RestResponse INVresponse = invoiceClient.Execute(request);
                        //Console.WriteLine(INVresponse.Content);
                        var document = JsonDocument.Parse(INVresponse.Content);
                        var tax = document.RootElement.GetProperty("summary").GetProperty("total_sales_tax").GetDecimal();
                        Console.WriteLine(tax);
                      

                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error while registering for QB events - " + ex.Message);
  
              

                return;
            }


        }
          static string GenerateInvoiceTaxUpdateXML(string invoice, decimal amount)
        {

            XmlDocument xmlResponse = new XmlDocument();
            xmlResponse.LoadXml(invoice);
            XmlNode invoiceNode = xmlResponse.SelectSingleNode("//InvoiceRet");

            XmlNodeList lineNodes = invoiceNode.SelectNodes("InvoiceLineRet");



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


            LogXmlData(@"InvoiceUpdateQuery.xml", doc.OuterXml);
            return doc.OuterXml;
        }
         static string GetInvoiceTaxUpdateXML(string invoiceNumber, decimal tax)
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
            LogXmlData(@"InvoiceQuery.xml", qbXMLDoc.OuterXml);
            return qbXMLDoc.OuterXml;
        }
    } }