using RMS.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using HelperManager;

namespace RMS.Infrastructure
{
    public class MMSReportsRepository : IMMSReports
    {
        #region User Declaration

        private string ManageQuote(string strMessage)
        {
            try
            {
                if (strMessage != null && strMessage != "")
                {
                    strMessage = strMessage.Replace("'", "''");
                }
            }
            catch (Exception)
            {

                return strMessage;
            }
            return strMessage;
        }

        public static string FormatDate(string strDate)
        {
            string Date = null;
            string[] dat = null;
            if (strDate != null)
            {
                if (strDate.Contains("/"))
                {
                    dat = strDate.Split('/');
                }
                else if (strDate.Contains("-"))
                {
                    dat = strDate.Split('-');
                }
                if (dat[2].Length > 4)
                {
                    dat[2] = dat[2].Substring(0, 4);
                }
                Date = dat[2] + "-" + dat[1] + "-" + dat[0];
            }
            return Date;
        }

        #endregion

        public DataSet GetPurchaseOrderDetailsByID(string PurchaseOrderNo, string vendorname)
        {
            DataSet ds = new DataSet();
            try
            {
                //using (NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from vwmmspurchaseorder where vchpurchaseorderno='" + ManageQuote(PurchaseOrderNo) + "' and vchvendorname='" + ManageQuote(vendorname) + "'", NPGSqlHelper.SQLConnString))
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter("select * from tabmmspurchaseorder tp  left join tabmmspurchaseorderdetails tpd on tp.recordid=tpd.orderid  where tp.vchpurchaseorderno='" + ManageQuote(PurchaseOrderNo) + "'", NPGSqlHelper.SQLConnString))
                {
                    da.Fill(ds);
                }
            }
            catch
            {


            }
            return ds;
        }

        public DataSet GetPurchaseOrderVendorsDetailsByID(string PurchaseOrderNo)
        {
            DataSet ds = new DataSet();
            try
            {
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter("select distinct vchvendorname,vchpurchaseorderno from vwmmspurchaseorder where vchpurchaseorderno='" + ManageQuote(PurchaseOrderNo) + "'", NPGSqlHelper.SQLConnString))
                {
                    da.Fill(ds);
                }
            }
            catch
            {


            }
            return ds;
        }


        public DataSet Getstockbystore(string fromdate, string todate, string store, string strReportType)
        {
            DataSet ds = new DataSet();
            try
            {
                string strQuery = string.Empty;
                if (strReportType == "Closing Stock")
                {
                    if (store == "" || store == "All" || store == "0")
                    {
                        strQuery = "select productid,productname,vchorderuom as vchuom,sum(purchaseqty) as availableqty,storagelocation from vwproductpurchaseconsumereturnqty where datgrndate <= '" + FormatDate(fromdate) + "'  group by productid,productname,vchorderuom,storagelocation";
                    }
                    else
                    {
                        strQuery = "select productid,productname,vchorderuom as vchuom,sum(purchaseqty) as availableqty,storagelocation from vwproductpurchaseconsumereturnqty where datgrndate <= '" + FormatDate(fromdate) + "' and  storagelocation='" + ManageQuote(store) + "' group by productid,productname,vchorderuom,storagelocation ";
                    }
                }
                else if (strReportType == "Stock Ledger")
                {
                    if (store == "" || store == "All" || store == "0")
                    {
                        strQuery = "select * from (select t1.productid,t1.productname,uomname,t3.storagelocation,t3.shelfname,coalesce(openingqty,0) as openingqty,purchaseqty,consumptionqty from tabmmsproductmst t1 left join (select productid,productname,vchorderuom  ,sum(purchaseqty) as openingqty,storagelocation,shelfname from vwproductpurchaseconsumereturnqty where datgrndate<'" + FormatDate(fromdate) + "'  group by productid,productname,vchorderuom ,storagelocation,shelfname)t2 on t1.productid=t2.productid and t1.productname=t2.productname and t1.uomname=t2.vchorderuom join (select productid,productname,vchorderuom,storagelocation,shelfname,case when type!='ISSUE' then availableqty else 0 end as purchaseqty, case when type='ISSUE' then availableqty else 0 end as consumptionqty from(select productid,productname,vchorderuom,sum(purchaseqty) as availableqty,storagelocation,shelfname,type from vwproductpurchaseconsumereturnqty where datgrndate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' group by productid,productname,vchorderuom,storagelocation,shelfname,type) g)t3 on t1.productid=t3.productid and t1.productname=t3.productname and t1.uomname=t3.vchorderuom ) t4 ";
                    }
                    else
                    {
                        strQuery = "select * from (select t1.productid,t1.productname,uomname,t3.storagelocation,t3.shelfname,coalesce(openingqty,0) as openingqty,purchaseqty,consumptionqty from tabmmsproductmst t1 left join (select productid,productname,vchorderuom ,sum(purchaseqty) as openingqty,storagelocation,shelfname from vwproductpurchaseconsumereturnqty where datgrndate<'" + FormatDate(fromdate) + "'  group by productid,productname,vchorderuom,storagelocation,shelfname)t2 on t1.productid=t2.productid and t1.productname=t2.productname and t1.uomname=t2.vchorderuom  join (select productid,productname,vchorderuom,storagelocation,shelfname,case when type!='ISSUE' then availableqty else 0 end as purchaseqty, case when type='ISSUE' then availableqty else 0 end as consumptionqty from(select productid,productname,vchorderuom,sum(purchaseqty) as availableqty,storagelocation,shelfname,type from vwproductpurchaseconsumereturnqty where datgrndate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' group by productid,productname,vchorderuom,storagelocation,shelfname,type) g)t3 on t1.productid=t3.productid and t1.productname=t3.productname and t1.uomname=t3.vchorderuom ) t4 where storagelocation='" + store + "'";
                    }
                }
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(strQuery, NPGSqlHelper.SQLConnString))
                {
                    da.Fill(ds, strReportType);
                }
            }
            catch
            {


            }
            return ds;
        }

        #region Purchase Report

        public DataSet GetPurchaseData(string fromdate, string todate, string strReportType)
        {
            DataSet ds = new DataSet();
            try
            {
                string strQuery = string.Empty;
                if (strReportType == "Purchse Statement")
                {
                    strQuery = "select distinct vchinvoiceno,cast(datinvoicedate as date)as datinvoicedate,vchvendorname,totalbasic,totaldiscount,servicetaxamount,transportcharges from vwmmspurchasestmt where datgrndate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' order by vchinvoiceno;";
                }
                else if (strReportType == "Purchase Bill Statement")
                {
                    strQuery = "select vchgrnno,vchinvoiceno,datinvoicedate,vchvendorname,productname,vchorderuom,purchaseqty,numgrnrate,basic,itemdiscount,itemtaxamount from vwmmspurchasestmt where datgrndate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' order by vchinvoiceno;";
                }
                else if (strReportType == "Purchase Item Statement")
                {
                    //strQuery = "select productname,purchaseqty,vchorderuom,numgrnrate,vchinvoiceno,datinvoicedate,vchvendorname from vwmmspurchasestmt where datgrndate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' order by productname;";
                    strQuery = "select productname,sum(purchaseqty) as purchaseqty,vchorderuom,numgrnrate,vchinvoiceno,datinvoicedate,vchvendorname from vwmmspurchasestmt where datgrndate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' group by productname,vchorderuom,numgrnrate,vchinvoiceno,datinvoicedate,vchvendorname order by productname;";
                }
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(strQuery, NPGSqlHelper.SQLConnString))
                {
                    da.Fill(ds, strReportType);
                }
            }
            catch
            {


            }
            return ds;
        }

        #endregion

        #region Outward Report

        public DataSet GetOutwardData(string fromdate, string todate, string store, string category)
        {
            DataSet ds = new DataSet();
            try
            {
                string strQuery = string.Empty;


                if (store == "" || store == "All" || store == "0" || category == "" || category == "0")
                {
                    strQuery = "select  productname,numissueconvertionqty,vchuom,numrate,(numissueconvertionqty*numrate) as amount,vchdepartment,categoryname,storagelocation from vwmmsproductsoutwardstmt where  datissuedate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' order by productname;";
                }
                else if (store != "" && category == "" || category == "0" || category == "undefined")
                {
                    strQuery = "select  productname,numissueconvertionqty,vchuom,numrate,(numissueconvertionqty*numrate) as amount,vchdepartment,categoryname,storagelocation from vwmmsproductsoutwardstmt where storagelocation='" + ManageQuote(store) + "' and datissuedate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' order by productname;";
                }
                else
                {
                    strQuery = "select  productname,numissueconvertionqty,vchuom,numrate,(numissueconvertionqty*numrate) as amount,vchdepartment,categoryname,storagelocation from vwmmsproductsoutwardstmt where storagelocation='" + ManageQuote(store) + "' and categoryname='" + ManageQuote(category) + "' and datissuedate between '" + FormatDate(fromdate) + "' and '" + FormatDate(todate) + "' order by productname;";
                }
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(strQuery, NPGSqlHelper.SQLConnString))
                {
                    da.Fill(ds);
                }
            }
            catch
            {

            }
            return ds;
        }

        #endregion


        #region Product Indent By Vikram Chathilla

        public DataSet GetProductIndentReport(string indent)
        {
            DataSet ds = new DataSet();
            try
            {
                //  string sxfk = "select TA.recordid,TA.vchindentno,TA.datindentdate,TA.vchindenttype,TA.vchrequestedby,TA.vchapprovalby,TA.vchdepartment,TA.statusid,TA.createdby,TA.createddate,TA.modifiedby,TA.modifieddate,TB.recordid as recordid2,TB.indentno as indentno2,TB.vchindentno as vchindentno2,TB.productid as productid2,TB.productcode as productcode2,TB.productcategoryid as productcategoryid2,TB.categoryname as categoryname2,TB.productsubcategoryid as productsubcategoryid2,TB.subcategoryname as subcategoryname2,TB.vchuom as vchuom2,TB.vchindentuom as vchindentuom2,TB.numindentqty as numindentqty2,TB.numindentconvertionqty as numindentconvertionqty2,TB.statusid as statusid2,TB.createdby as createdby2,TB.AvailableQty as AvailableQty2,TB.createddate as createddate2,TB.modifiedby as modifiedby2,TB.modifieddate as modifieddate2,TB.productname as productname2 ,TB.storagelocationname as storagelocationname2,TB.storagelocationcode as storagelocationcode2,TB.shelfname as shelfname2,TB.availableqty as  availableqty2,TB.vchmaxstock as vchmaxstock2,TB.vchminstock as vchminstock2 ,TB.vchproductype as vchproductype2 from tabmmsmaterialindent TA   join  tabmmsmaterialindentdetails TB on TA.recordid= TB.indentno where TB.vchindentno='" + indent + "'and  TB.statusid=1  ";
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter("select TA.recordid,TA.vchindentno,to_char(TA.datindentdate,'dd-MM-yyyy') as datindentdate,TA.vchindenttype,TA.vchrequestedby,TA.vchapprovalby,TA.vchdepartment,TA.statusid,TA.createdby,TA.createddate,TA.modifiedby,TA.modifieddate,TB.recordid as recordid2,TB.indentno as indentno2,TB.vchindentno as vchindentno2,TB.productid as productid2,TB.productcode as productcode2,TB.productcategoryid as productcategoryid2,TB.categoryname as categoryname2,TB.productsubcategoryid as productsubcategoryid2,TB.subcategoryname as subcategoryname2,TB.vchuom as vchuom2,TB.vchindentuom as vchindentuom2,TB.numindentqty as numindentqty2,TB.numindentconvertionqty as numindentconvertionqty2,TB.statusid as statusid2,TB.createdby as createdby2,TB.AvailableQty as AvailableQty2,TB.createddate as createddate2,TB.modifiedby as modifiedby2,TB.modifieddate as modifieddate2,TB.productname as productname2 ,TB.storagelocationname as storagelocationname2,TB.storagelocationcode as storagelocationcode2,TB.shelfname as shelfname2,TB.availableqty as  availableqty2,TB.vchmaxstock as vchmaxstock2,TB.vchminstock as vchminstock2 ,TB.vchproductype as vchproductype2 from tabmmsmaterialindent TA   join  tabmmsmaterialindentdetails TB on TA.recordid= TB.indentno where TB.vchindentno='" + indent + "' and  TB.statusid=1 ", NPGSqlHelper.SQLConnString))
                {
                    da.Fill(ds);
                }
            }
            catch
            {


            }
            return ds;
        }

        #endregion


    }
}
