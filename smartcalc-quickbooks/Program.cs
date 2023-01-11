// See https://aka.ms/new-console-template for more information

using Interop.QBFC16;

namespace smartcalc_quickbooks;

public class SmartCert
{
    public static void Main(string[] args)
    {
        QBSessionManager sessionManager = new QBSessionManager();
        sessionManager.OpenConnection("", "SmartCalc");
        sessionManager.BeginSession("", ENOpenMode.omDontCare);
        sessionManager.EndSession();
        sessionManager.CloseConnection();
    }
}