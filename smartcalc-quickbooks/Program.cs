// See https://aka.ms/new-console-template for more information

using Interop.QBFC16;
using System.Net.Sockets;
using Interop.QBXMLRP2;
using System.Xml;
using System.Text;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using SubscribeAndHandleQBEvent;
using Newtonsoft.Json;
using RestSharp;

namespace smartcalc_quickbooks;

public class SmartCert
{
    
    private static string appID = "SC123";
    private static string appName = "SmartCalc";
    private static RequestProcessor2 rp;
    private static string maxVersion;
    private static string ticket;
    private static string companyFile = "";
    private static QBFileMode mode = QBFileMode.qbFileOpenDoNotCare;
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
        LogXmlData(@"C:\Temp\DataEvent.xml", strRetString);
        return strRetString;

    }
    private static void LogXmlData(string strFile, string strXML)
    {
        System.IO.StreamWriter sw = new System.IO.StreamWriter(strFile);
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
        LogXmlData(@"C:\Temp\UIExtension.xml", strRetString);
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
                    Console.WriteLine("Error while subscribing for events\n\terror Code - {0},\n\tSeverity - {1},\n\tError Message - {2}\n", retStatusCode, retStatusSeverity, retStatusMessage);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while registering for QB events - " + ex.Message);
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
            Console.WriteLine(e.Message);
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
            Console.WriteLine("An Exception has occurred: " + e.Message);
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
       Console.WriteLine(response,count);
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
                Console.WriteLine(e.Message);
            }
        }
    }
    public static async Task Main(string[] args)
    {
        //   try
        //   {
        //       key = Registry.ClassesRoot.CreateSubKey("CLSID\\" + Marshal.GenerateGuidForType(typeof(EventHandlerObj)).ToString("B"));
        //       key2 = key.CreateSubKey("LocalServer32");
        //       key2.SetValue(null, Application.ExecutablePath);

        //   }
        //   catch (Exception ex)
        //   {
        //       MessageBox.Show("Error while registering the server:\n" + ex.ToString());
        //   }
        //   finally
        //   {
        //       if (key != null)
        //           key.Close();
        //       if (key2 != null)
        //           key2.Close();
        //   }
        //   bRet = false;
        //   var qrp = new QBSessionManager();
        //   qrp.OpenConnection(appID, appName);

        //    qrp.BeginSession(companyFile, ENOpenMode.omDontCare);
        //   //string[] versions = qrp.get_QBXMLVersionsForSession(ticket);
        ////   maxVersion = versions[versions.Length - 1];
        //   SubscribeForEvents(QBSubscriptionType.Data, String.Empty);
        //   SubscribeForEvents(QBSubscriptionType.UIExtension, args[1]);

        //   rp.EndSession(ticket);

        using (var client = new HttpClient())
        {
            // Set the base address of the API
            client.BaseAddress = new Uri("http://mswsmartcalc.suchimsapps.com/");

            try
            {
                //var parameters = new
                //{
                //    method = "login",
                //    input_type = "JSON",
                //    response_type = "JSON",
                //    rest_Data = new
                //    {
                //        user_auth = new
                //        {
                //            user_name = "qbapi",
                //            password = "qbapi@2023"
                //        }
                //    ,
                //        application_name = "Smart-Calc",
                //    }
                //};


                ////string param3 = "param3_value";
                //// Make the API call using the specified endpoint and HTTP method
                //var json = JsonConvert.SerializeObject(parameters);
                //var content = new StringContent(json, Encoding.UTF8, "application/json");
                //HttpResponseMessage response = await client.PostAsync($"http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php",content);

                //// Check if the call was successful
                //if (response.IsSuccessStatusCode)
                //{
                //    // Read the response content
                //    string result = await response.Content.ReadAsStringAsync();
                //    Console.WriteLine(result);
                //}
                //else
                //{
                //    Console.WriteLine("Error: " + response.StatusCode);
                //}
                var rclient = new RestClient("http://mswsmartcalc.suchimsapps.com/service/v4_1/rest.php?method=auth_token&input_type=JSON&response_type=JSON&rest_data={\"user_auth\":{\"user_name\":\"qbapi\",\"password\":\"f15df058bee598625a2762554488d903\",\"version\":\"1\"},\"application_name\":\"Smart-Calc\",\"name_value_list\":[\"user_id\"]}");
                // rclient.Timeout = -1; 
                var request = new RestRequest();
              
                request.AddHeader("Content-Type", "application/json"); 
                request.AddHeader("Cookie", "PHPSESSID=9bf6bngk1aspih5op58pnqte45");
                var body = @"{" + "\n" + @"    ""method"":""login""," + "\n" + @"  ""input_type"":""JSON""," + "\n" + @"  ""response_type"":""JSON"", " + "\n" + @"}"; 
                request.AddParameter("application/json", body, ParameterType.RequestBody); 
                RestResponse response = rclient.Execute(request); 
                Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }



    }
} 