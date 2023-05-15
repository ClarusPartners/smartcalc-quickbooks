using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;  // For using MessageBox.
using QBSDKEVENTLib; // In order to implement IQBEventCallback.
using System.Runtime.InteropServices;  // For use of the GuidAttribute, ProgIdAttribute and ClassInterfaceAttribute.
using System.Xml; //XML Parsing
using System.IO;
using SubscribeAndHandleQBEvent;
using System.Threading.Tasks;
using System.Threading;
using AkuCalcDesktop;

using System.Security.Cryptography;

namespace SmartCalc
{
    [
      Guid("62447F81-C195-446f-8201-94F0614E49D5"),  // We indicate a specific CLSID for "SmartCalc.EventHandlerObj" for convenience of searching the registry.
      ProgId("SmartCalc.EventHandlerObj"),  // This ProgId is used by default. Not 100% necessary.
      ClassInterface(ClassInterfaceType.None)
    ]
    public class EventHandlerObj :
        ReferenceCountedObjectBase, // EventHandlerObj is derived from ReferenceCountedObjectBase so that we can track its creation and destruction.
        IQBEventCallback  // this must implement the IQBEventCallback interface.
    {
        private StreamWriter log { get; set; }

        public EventHandlerObj()
        {
            MessageBox.Show("Mechanism");
            // ReferenceCountedObjectBase constructor will be invoked.
            //Console.WriteLine("EventHandlerObj constructor.");

            System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\Temp\log.txt");

        }
     
        ~EventHandlerObj()
        {
            // ReferenceCountedObjectBase destructor will be invoked.
            //Console.WriteLine("EventHandlerObj destructor.");
            log.Flush();
            log.Close();
        }
        public async Task ProcessInvoice(string refNumber)
        {

            var smartCalc = new SmartCalcHandler();

            var invoice = SmartCalcHandler.QueryInvoice(refNumber);

            if (!invoice.Contains("Clarus Partners"))
            {

                decimal taxRate = smartCalc.GetSalesTaxFromSmartCalc(invoice);
                MessageBox.Show(taxRate.ToString()); ;
                string updateXML = SmartCalcHandler.GenerateInvoiceTaxUpdateXML(invoice, taxRate);
                SmartCalcHandler.UpdateInvoice(updateXML);
            }
            else
            {

                MessageBox.Show(invoice);

                //QueueInvoiceForSmartCalcProcessing(refNumber);
            }
        }
        private void QueueInvoiceForSmartCalcProcessing(string invoiceNumber)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"QBGeneratedInvoices.txt");

            // Use buffered I/O and properly dispose of resources
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    try
                    {
                        // Write the string to the temporary file
                        streamWriter.Write(invoiceNumber);
                    }
                    catch (IOException ex)
                    {
                        // Handle IOException (e.g., disk full, access denied)
                        //Console.WriteLine($"Error writing to temporary file: {ex.Message}");
                        throw;
                    }

                }
            }
        }

        //Call back function which would be invoked from the QB
        public async void inform(string strMessage)
        {
            //MessageBox.Show("Starting Inform");

            //MessageBox.Show(strMessage);

            //MessageBox.Show("Starting Try");
            try
            {
               // MessageBox.Show("XMLDocumentProcessingStart");
                StringBuilder sb = new StringBuilder(strMessage);
                XmlDocument outputXMLDoc = new XmlDocument();
                outputXMLDoc.LoadXml(strMessage);
                //MessageBox.Show("XMLDocumentProcessingStop");
               // MessageBox.Show(outputXMLDoc.OuterXml);
                XmlNodeList qbXMLMsgsRsNodeList = outputXMLDoc.GetElementsByTagName("QBXMLEvents");
                XmlNode childNode = qbXMLMsgsRsNodeList.Item(0).FirstChild;
                
            

                // handle the event based on type of event
                switch (childNode.Name)
                {
                    
                    case "DataEvent":
                        MessageBox.Show(strMessage);
                        //Handle Data Event Here
                        if (!strMessage.Contains("Modify"))
                        {
                           
                            //InvoiceUI akuCalc = new InvoiceUI();
                          //  akuCalc.AddTextBox("A new Invoice has been created. Please click the button to update the Sales Tax");
                            //akuCalc.ShowDialog();
                            MessageBox.Show("A New Invoice has been generated. Initiating Call to AKUCalc");
                            //Console.WriteLine("A New Invoice has been generated. Initiating Call to AKUCalc");
                            using (StreamWriter writer = new StreamWriter(@"C:\Temp\Invoice.TXT"))
                            {
                                writer.WriteLine(strMessage);
                            }
                            Task.Run(() => ProcessInvoice(childNode["TxnEvent"]["RefNumber"].InnerText));
                        }
                        else
                        {
                            MessageBox.Show(strMessage);

                        }
                        break;

                    case "UIExtensionEvent":
                        //Handle UI Extension Event HERE
                        //MessageBox.Show(sb.ToString(), "UI EXTENSION EVENT - From QB");
                        


                        var dir = Application.StartupPath;
                        //Console.WriteLine(dir);
                        Process.Start(dir+@"\AkuCalcConfiguration.exe"); 
                        //browser.Document.Window.Open(new Uri("http://mswsmartcalc.suchimsapps.com/index.php?module=T_Transaction_Test&action=index&parentTab=Transaction%20Tax%20Calculation"), "displayWindow", "status=yes,width=200,height=400", false);
                        break;

                    default:
                        MessageBox.Show(sb.ToString(), "Response From QB");
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                //Console.WriteLine("Unexpected error in processing the response from QB - " + ex.Message);
            }
        }
        
    }

    class EventHandlerObjClassFactory : ClassFactoryBase
    {
        public EventHandlerObjClassFactory()
        {
            //Console.WriteLine("EventHandlerObjClassFactory constructor.");



        }
        public override void virtual_CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
        {
            //Console.WriteLine("EventHandlerObjClassFactory.CreateInstance().");
            //Console.WriteLine("Requesting Interface : " + riid.ToString());

            if (riid == Marshal.GenerateGuidForType(typeof(IQBEventCallback)) ||
                riid == SmartCalc.IID_IDispatch ||
                riid == SmartCalc.IID_IUnknown)
            {
                EventHandlerObj EventHandlerObj_New = new EventHandlerObj();

                ppvObject = Marshal.GetComInterfaceForObject(EventHandlerObj_New, typeof(IQBEventCallback));
            }
            else
            {
                throw new COMException("No interface", unchecked((int)0x80004002));
            }
        }
    }
}