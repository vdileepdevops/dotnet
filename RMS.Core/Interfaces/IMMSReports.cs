using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMS.Core.Interfaces
{
    public interface IMMSReports
    {
        DataSet GetPurchaseOrderDetailsByID(string PurchaseOrderNo, string vendorname);
        DataSet GetPurchaseOrderVendorsDetailsByID(string PurchaseOrderNo);
        DataSet Getstockbystore(string fromdate, string todate, string store, string strReportType);
        DataSet GetPurchaseData(string fromdate, string todate, string strReportType);
        DataSet GetOutwardData(string fromdate, string todate, string store, string category);

        DataSet GetProductIndentReport(string indent);
    }
}
