using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using RMS.Core.Interfaces;
using RMS.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace RMS.Controllers
{
    public class MMSReportsController : Controller
    {
        private IMMSReports mmsReports = new MMSReportsRepository();
        private IPOSReports posTransactions = new POSReportsRepository();


        //
        // GET: /MMSReports/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult PurchaseOrderReport(string pono, string vendorname)
        {
            System.Data.DataSet ds = mmsReports.GetPurchaseOrderDetailsByID(pono, vendorname);
            System.Data.DataSet ds1 = posTransactions.companyname();
            string strcompanyname = ds1.Tables[0].Rows[0]["vchcompanyname"].ToString();
            string strcompanyAddress = ds1.Tables[0].Rows[0]["address"].ToString();

            ReportClass rptH = new ReportClass();
            rptH.FileName = Server.MapPath("/Reports/PurchaseOrder.rpt");
            rptH.Load();
            rptH.SetDataSource(ds.Tables[0]);
            rptH.SetParameterValue("CompanyName", strcompanyname);
            rptH.SetParameterValue("Address", strcompanyAddress);

            Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");
        }

        public ActionResult StockReport()
        {
            //System.Data.DataSet ds = mmsReports.GetPurchaseOrderDetailsByID(pono, vendorname);
            //System.Data.DataSet ds1 = posTransactions.companyname();
            //string strcompanyname = ds1.Tables[0].Rows[0]["vchcompanyname"].ToString();
            //string strcompanyAddress = ds1.Tables[0].Rows[0]["address"].ToString();

            //ReportClass rptH = new ReportClass();
            //rptH.FileName = Server.MapPath("/Reports/PurchaseOrder.rpt");
            //rptH.Load();
            //rptH.SetDataSource(ds.Tables[0]);
            //rptH.SetParameterValue("CompanyName", strcompanyname);
            //rptH.SetParameterValue("Address", strcompanyAddress);

            //Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
            //return File(stream, "application/pdf");
            return View();
        }

        public ActionResult StockLedgerReport(string fromdate, string todate, string store, string Reporttype)
        {
            System.Data.DataSet ds = mmsReports.Getstockbystore(fromdate, todate, store, Reporttype);
            if (ds != null)
            {
                System.Data.DataSet ds1 = posTransactions.companyname();
                string strcompanyname = ds1.Tables[0].Rows[0]["vchcompanyname"].ToString();
                string strcompanyAddress = ds1.Tables[0].Rows[0]["address"].ToString();

                ReportClass rptH = new ReportClass();

                if (Reporttype == "Closing Stock")
                {
                    rptH.FileName = Server.MapPath("/Reports/StockReport.rpt");
                }
                if (Reporttype == "Stock Ledger")
                {
                    rptH.FileName = Server.MapPath("/Reports/StockLedgerReport.rpt");
                }
                rptH.Load();
                rptH.SetDataSource(ds);


                rptH.SetParameterValue("CompanyName", strcompanyname);
                rptH.SetParameterValue("Address", strcompanyAddress);
                rptH.SetParameterValue("FromDate", fromdate);
                rptH.SetParameterValue("ToDate", todate);

                Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
            else
            {
                return View("StockReport");
            }

        }

        public ActionResult PurchaseReport()
        {
            return View();
        }

        public ActionResult PurchaseReports(string fromdate, string todate, string Reporttype)
        {
            System.Data.DataSet ds = mmsReports.GetPurchaseData(fromdate, todate, Reporttype);
            if (ds != null)
            {
                System.Data.DataSet ds1 = posTransactions.companyname();
                string strcompanyname = ds1.Tables[0].Rows[0]["vchcompanyname"].ToString();
                string strcompanyAddress = ds1.Tables[0].Rows[0]["address"].ToString();

                ReportClass rptH = new ReportClass();

                if (Reporttype == "Purchse Statement")
                {
                    rptH.FileName = Server.MapPath("/Reports/PurchaseStatement.rpt");
                }
                if (Reporttype == "Purchase Bill Statement")
                {
                    rptH.FileName = Server.MapPath("/Reports/PurchaseBillStatement.rpt");
                }
                if (Reporttype == "Purchase Item Statement")
                {
                    rptH.FileName = Server.MapPath("/Reports/PurchaseItemReport.rpt");
                }
                rptH.Load();
                rptH.SetDataSource(ds.Tables[0]);


                rptH.SetParameterValue("CompanyName", strcompanyname);
                rptH.SetParameterValue("Address", strcompanyAddress);
                rptH.SetParameterValue("FromDate", fromdate);
                rptH.SetParameterValue("ToDate", todate);

                Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
            else
            {
                return View("StockReport");
            }
        }

        public ActionResult OutwardReport()
        {
            return View();
        }

        public ActionResult OutWardReports(string fromdate, string todate, string Store, string Category)
        {
            System.Data.DataSet ds = mmsReports.GetOutwardData(fromdate, todate, Store, Category);
            if (ds != null)
            {
                System.Data.DataSet ds1 = posTransactions.companyname();
                string strcompanyname = ds1.Tables[0].Rows[0]["vchcompanyname"].ToString();
                string strcompanyAddress = ds1.Tables[0].Rows[0]["address"].ToString();

                ReportClass rptH = new ReportClass();

                rptH.FileName = Server.MapPath("/Reports/OuwardItemStatement.rpt");

                rptH.Load();
                rptH.SetDataSource(ds.Tables[0]);


                rptH.SetParameterValue("CompanyName", strcompanyname);
                rptH.SetParameterValue("Address", strcompanyAddress);
                rptH.SetParameterValue("FromDate", fromdate);
                rptH.SetParameterValue("ToDate", todate);
                rptH.SetParameterValue("Store", Store);
                rptH.SetParameterValue("Category", Category);

                Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
            else
            {
                return View("StockReport");
            }
        }


        public ActionResult ProductIndentReports(string indent)
        {
            DataSet ds = new DataSet();
            DataSet ds1 = new DataSet();
            try
            {
                ds = mmsReports.GetProductIndentReport(indent);
                ds1 = posTransactions.companyname();
                string strcompanyname = ds1.Tables[0].Rows[0]["vchcompanyname"].ToString();
                string strcompanyAddress = ds1.Tables[0].Rows[0]["address"].ToString();
                ReportClass rptH = new ReportClass();
                rptH.FileName = Server.MapPath("/Reports/ProductIndentReport.rpt");
                rptH.Load();
                rptH.SetDataSource(ds.Tables[0]);
                rptH.SetParameterValue("CompanyName", strcompanyname);
                rptH.SetParameterValue("Address", strcompanyAddress);
                Stream stream = rptH.ExportToStream(ExportFormatType.PortableDocFormat);
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}