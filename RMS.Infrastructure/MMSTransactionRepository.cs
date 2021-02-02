using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperManager;
using System.Data;
using RMS.Core;
using RMS.Core.Interfaces;
using Npgsql;
using System.Web.Script.Serialization;
using HRMS.Infrastructure;

namespace RMS.Infrastructure
{
    public class MMSTransactionRepository : IMMSTransaction
    {
        #region Global Declarations
        NpgsqlConnection con = null;
        NpgsqlDataReader npgdr = null;
        NpgsqlTransaction trans = null;
        string GNextID = string.Empty;
        #endregion

        #region Global Methods
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
        private void CloseCon()
        {
            con.Close();
            con.Dispose();
            con.ClearPool();
        }
        public int CheckCount(string strCheckValue, string strTableName, string strColumnName)
        {
            string strcount = "SELECT COUNT(*)  FROM " + strTableName + " WHERE upper(" + strColumnName + ")='" + ManageQuote(strCheckValue.Trim().ToUpper()) + "'and statusid=1;";
            int res = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strcount));
            return res;
        }
        public string GenerateNextID(string Table, string Column, string FormName)
        {

            string prefix = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, "select trim(upper(vchprefix)) from tabposcode  where upper(vchmastername)='" + FormName.ToUpper().Trim() + "'"));
            string res = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, "SELECT (coalesce(MAX(coalesce(cast(LTRIM(" + Column + ",'" + prefix.Trim() + "')as numeric),0)),0))+1 as nextno FROM  " + Table + " where  " + Column + " like '%" + prefix.Trim() + "%';"));
            return prefix + res;
        }
        #endregion

        #region Goods Received Note
        public DataTable getponumbers(string ID)
        {
            DataTable dt = new DataTable();
            try
            {
                if (ID != "ALL")
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select  distinct poid,vchpurchaseorderno as pono from vwponumbersforgrn where vendorid=" + ID + ";").Tables[0];
                }
                else
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select  distinct poid,vchpurchaseorderno as pono from vwponumbersforgrn").Tables[0];
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getponumbers");
            }
            return dt;
        }
        public DataTable getgrngriddetails(string VID, string POID)
        {
            DataTable dt = new DataTable();
            try
            {
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select  distinct null as grnuom,null as uomconversionvalue, productid, productcode,productname, productcategoryid, categoryname, productsubcategoryid, subcategoryname, orderuom,orderqty as  orderedqty,PRERECEIVEDQTY as previousqty,0 as returnqty, porate,porate as grnrate, storagelocationid as  storagelocationid, storagelocation as  storagelocation,shelfid as  shelfid,shelfname as  shelfname from  vwponumbersforgrn where poid=" + POID + ";").Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getgrngriddetails");
            }
            return dt;
        }
        public DataTable Getstoragelocations()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1;").Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "Getstoragelocations");
            }
            return dt;
        }
        public DataTable getuomlist()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select vchuomid as uomid,vchuomdescription as uom from tabinvuommst where intstatus=1;").Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getuomlist");
            }
            return dt;
        }
        public DataTable getshelfs(string ID)
        {
            DataTable dt = new DataTable();
            try
            {
                string Code = string.Empty;
                Code = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, "select storagelocationcode from tabmmsstoragelocationmst where statusid=1 and storagelocationid=" + ID + ";"));
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select shelfid,shelfname from tabmmsshelfmst  where statusid=1 and storagecode='" + Code + "';").Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getponumbers");
            }
            return dt;
        }
        public DataTable GetVendorProducts(string ID)
        {
            DataTable dt = new DataTable();
            string strData = string.Empty;
            try
            {
                strData = "select distinct  tp.productid,tp.productname from tabmmsproductvendors  tv join tabmmsproductmst tp on tv.productid=tp.productid  where  tv.statusid=1 and tp.statusid=1 and tv.vendorid=" + ID + "";
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, strData).Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "GetVendorProducts");
            }

            return dt;
        }
        public DataTable BinduomConversionValues()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select productid,productcode,vchstandarduom, numstandardqty,vchconvertionuom,numconvertionqty from tabmmsproductwiseuomconvertion;").Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "BinduomConversionValues");
            }
            return dt;
        }
        public bool SaveGRN(GoodsReceivedNoteDTO GoodsReceivedNoteDTO, List<GoodsReceivedNoteDTO> lstGoodsReceivedNoteDTO, GoodsReceivedNoteTAXDTO TAX)
        {
            bool IsSave = false;
            int Count = 0;
            con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            trans = con.BeginTransaction();
            try
            {
                string StrCode = string.Empty;
                string StrID = string.Empty;
                string strInsert = string.Empty;
                StrCode = GenerateNextID("tabmmsgoodsreceivednote", "vchgrnno", "GOODS RECEIVED NOTE");
                //strInsert = "discountamount, transportcharges, excisetaxpercentage, excisetaxamount, cesstaxpercentage, cesstaxamount, shcesstaxpercentage, shcesstaxamount, taxtype, vatorcsttaxpercentage, vatorcsttaxamount, basicamount, totalamount";
                //strInsert = "(," + TAX.DiscountValue + ", " + TAX.TransportCharges + ", " + TAX.TaxExcisePercentage + ", " + TAX.TaxExciseAmount + ", " + TAX.TaxCESSPercentage + ", " + TAX.TaxCESSAmount + ", " + TAX.TaxSHCESSPercentage + ", " + TAX.TaxSHCESSAmount + ", " + TAX.taxtype + ", " + TAX.vatorcst + ", " + TAX.TaxvatorcstAmount + ", " + TAX.BasicAmount + ", " + TAX.TotalAmount + ")";
                //strInsert = "SELECT recordid, vchgrnno, vchpurchaseorderno, vchgrntype, datgrndate, vchreceivedby, vendorid, vchvendorid, vchinvoiceno, datinvoicedate, vchterms, vchgrnstatus, statusid, createdby, createddate, modifiedby, vchvendorname FROM tabmmsgoodsreceivednote;";
                if (TAX.TransportCharges == null || TAX.TransportCharges == string.Empty)
                {
                    TAX.TransportCharges = "0";
                }
                if (TAX.DiscountType != null && TAX.DiscountValue != string.Empty)
                {
                    if (TAX.DiscountValue == null || TAX.DiscountValue == string.Empty)
                    {
                        TAX.DiscountValue = "0";
                    }
                }
                else
                {
                    TAX.DiscountValue = "0";
                }
                if (TAX.vatorcst == null || TAX.DiscountValue == string.Empty || TAX.DiscountValue == "")
                {
                    TAX.TaxvatorcstAmount = "0";
                }
                if (TAX.TaxExcisePercentage == null || TAX.TaxExcisePercentage == string.Empty || TAX.TaxExcisePercentage == "")
                {
                    TAX.TaxExciseAmount = "0";
                    TAX.TaxSHCESSAmount = "0";
                    TAX.TaxSHCESSPercentage = "0";
                    TAX.TaxCESSAmount = "0";
                    TAX.TaxCESSPercentage = "0";
                }
                if (GoodsReceivedNoteDTO.invoicedate == null || GoodsReceivedNoteDTO.invoicedate == "")
                {
                    if (GoodsReceivedNoteDTO.grntype == "DIRECT")
                    {
                        strInsert = "INSERT INTO tabmmsgoodsreceivednote(vchgrnno, vchgrntype, datgrndate, vchreceivedby, vendorid, vchvendorname, vchinvoiceno, vchterms, vchgrnstatus, statusid, createdby, createddate,discountamount, transportcharges, excisetaxpercentage, excisetaxamount, cesstaxpercentage, cesstaxamount, shcesstaxpercentage, shcesstaxamount, taxtype, vatorcsttaxpercentage, vatorcsttaxamount, basicamount, totalamount,numdueamount) values ('" + StrCode + "','" + ManageQuote(GoodsReceivedNoteDTO.grntype) + "','" + FormatDate(GoodsReceivedNoteDTO.grndate) + "', '" + ManageQuote(GoodsReceivedNoteDTO.requestedperson) + "', '" + GoodsReceivedNoteDTO.vendorid + "', '" + ManageQuote(GoodsReceivedNoteDTO.vendorname) + "', '" + ManageQuote(GoodsReceivedNoteDTO.invoiceno) + "', '" + ManageQuote(GoodsReceivedNoteDTO.TermsandConditions) + "', 'N', 1, " + GoodsReceivedNoteDTO.CreatedBy + ", current_timestamp," + TAX.DiscountValue + ", " + TAX.TransportCharges + ", " + TAX.TaxExcisePercentage + ", " + TAX.TaxExciseAmount + ", " + TAX.TaxCESSPercentage + ", " + TAX.TaxCESSAmount + ", " + TAX.TaxSHCESSPercentage + ", " + TAX.TaxSHCESSAmount + ", '" + TAX.vatorcst + "', " + TAX.taxvatcst + ", " + TAX.TaxvatorcstAmount + ", " + TAX.BasicAmount + ", " + TAX.TotalAmount + ", " + TAX.TotalAmount + ") returning recordid;";
                    }
                    else
                    {
                        strInsert = "INSERT INTO tabmmsgoodsreceivednote(vchgrnno, vchpurchaseorderno, vchgrntype, datgrndate, vchreceivedby, vendorid, vchvendorname, vchinvoiceno, vchterms, vchgrnstatus, statusid, createdby, createddate,discountamount, transportcharges, excisetaxpercentage, excisetaxamount, cesstaxpercentage, cesstaxamount, shcesstaxpercentage, shcesstaxamount, taxtype, vatorcsttaxpercentage, vatorcsttaxamount, basicamount, totalamount,numdueamount) values ('" + StrCode + "', '" + ManageQuote(GoodsReceivedNoteDTO.pono) + "','" + ManageQuote(GoodsReceivedNoteDTO.grntype) + "','" + FormatDate(GoodsReceivedNoteDTO.grndate) + "', '" + ManageQuote(GoodsReceivedNoteDTO.requestedperson) + "', '" + GoodsReceivedNoteDTO.vendorid + "', '" + ManageQuote(GoodsReceivedNoteDTO.vendorname) + "', '" + ManageQuote(GoodsReceivedNoteDTO.invoiceno) + "', '" + ManageQuote(GoodsReceivedNoteDTO.TermsandConditions) + "', 'N', 1, " + GoodsReceivedNoteDTO.CreatedBy + ", current_timestamp," + TAX.DiscountValue + ", " + TAX.TransportCharges + ", " + TAX.TaxExcisePercentage + ", " + TAX.TaxExciseAmount + ", " + TAX.TaxCESSPercentage + ", " + TAX.TaxCESSAmount + ", " + TAX.TaxSHCESSPercentage + ", " + TAX.TaxSHCESSAmount + ", '" + TAX.vatorcst + "', " + TAX.taxvatcst + ", " + TAX.TaxvatorcstAmount + ", " + TAX.BasicAmount + ", " + TAX.TotalAmount + ", " + TAX.TotalAmount + ") returning recordid;";
                    }
                }
                else
                {
                    if (GoodsReceivedNoteDTO.grntype == "DIRECT")
                    {
                        strInsert = "INSERT INTO tabmmsgoodsreceivednote(vchgrnno, vchgrntype, datgrndate, vchreceivedby, vendorid, vchvendorname, vchinvoiceno, datinvoicedate, vchterms, vchgrnstatus, statusid, createdby, createddate,discountamount, transportcharges, excisetaxpercentage, excisetaxamount, cesstaxpercentage, cesstaxamount, shcesstaxpercentage, shcesstaxamount, taxtype, vatorcsttaxpercentage, vatorcsttaxamount, basicamount, totalamount,numdueamount) values ('" + StrCode + "','" + ManageQuote(GoodsReceivedNoteDTO.grntype) + "','" + FormatDate(GoodsReceivedNoteDTO.grndate) + "', '" + ManageQuote(GoodsReceivedNoteDTO.requestedperson) + "', '" + GoodsReceivedNoteDTO.vendorid + "', '" + ManageQuote(GoodsReceivedNoteDTO.vendorname) + "', '" + ManageQuote(GoodsReceivedNoteDTO.invoiceno) + "', '" + FormatDate(GoodsReceivedNoteDTO.invoicedate) + "', '" + ManageQuote(GoodsReceivedNoteDTO.TermsandConditions) + "', 'N', 1, " + GoodsReceivedNoteDTO.CreatedBy + ", current_timestamp," + TAX.DiscountValue + ", " + TAX.TransportCharges + ", " + TAX.TaxExcisePercentage + ", " + TAX.TaxExciseAmount + ", " + TAX.TaxCESSPercentage + ", " + TAX.TaxCESSAmount + ", " + TAX.TaxSHCESSPercentage + ", " + TAX.TaxSHCESSAmount + ", '" + TAX.vatorcst + "', " + TAX.taxvatcst + ", " + TAX.TaxvatorcstAmount + ", " + TAX.BasicAmount + ", " + TAX.TotalAmount + ", " + TAX.TotalAmount + ") returning recordid;";
                    }
                    else
                    {
                        strInsert = "INSERT INTO tabmmsgoodsreceivednote(vchgrnno, vchpurchaseorderno, vchgrntype, datgrndate, vchreceivedby, vendorid, vchvendorname, vchinvoiceno, datinvoicedate, vchterms, vchgrnstatus, statusid, createdby, createddate,discountamount, transportcharges, excisetaxpercentage, excisetaxamount, cesstaxpercentage, cesstaxamount, shcesstaxpercentage, shcesstaxamount, taxtype, vatorcsttaxpercentage, vatorcsttaxamount, basicamount, totalamount,numdueamount) values ('" + StrCode + "', '" + ManageQuote(GoodsReceivedNoteDTO.pono) + "','" + ManageQuote(GoodsReceivedNoteDTO.grntype) + "','" + FormatDate(GoodsReceivedNoteDTO.grndate) + "', '" + ManageQuote(GoodsReceivedNoteDTO.requestedperson) + "', '" + GoodsReceivedNoteDTO.vendorid + "', '" + ManageQuote(GoodsReceivedNoteDTO.vendorname) + "', '" + ManageQuote(GoodsReceivedNoteDTO.invoiceno) + "', '" + FormatDate(GoodsReceivedNoteDTO.invoicedate) + "', '" + ManageQuote(GoodsReceivedNoteDTO.TermsandConditions) + "', 'N', 1, " + GoodsReceivedNoteDTO.CreatedBy + ", current_timestamp," + TAX.DiscountValue + ", " + TAX.TransportCharges + ", " + TAX.TaxExcisePercentage + ", " + TAX.TaxExciseAmount + ", " + TAX.TaxCESSPercentage + ", " + TAX.TaxCESSAmount + ", " + TAX.TaxSHCESSPercentage + ", " + TAX.TaxSHCESSAmount + ",'" + TAX.vatorcst + "', " + TAX.taxvatcst + ", " + TAX.TaxvatorcstAmount + ", " + TAX.BasicAmount + ", " + TAX.TotalAmount + ", " + TAX.TotalAmount + ") returning recordid;";
                    }
                }
                StrID = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));
                string Items = string.Empty;
                decimal ConversionReceivedQty = 0;
                decimal ConversionReturnQty = 0;
                for (int i = 0; i < lstGoodsReceivedNoteDTO.Count; i++)
                {
                    ConversionReceivedQty = 0;
                    ConversionReturnQty = 0;
                    if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                    {
                        if (lstGoodsReceivedNoteDTO[i].orderuom != lstGoodsReceivedNoteDTO[i].grnuom)
                        {
                            ConversionReceivedQty = Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) * Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].uomconversionvalue);
                            ConversionReturnQty = Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].returnqty) * Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].uomconversionvalue);
                        }
                        else
                        {
                            ConversionReceivedQty = Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) * 1;
                            ConversionReturnQty = Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].returnqty) * 1;
                            lstGoodsReceivedNoteDTO[i].uomconversionvalue = "1";
                        }
                        if (Convert.ToString(lstGoodsReceivedNoteDTO[i].shelfid) == "" || Convert.ToString(lstGoodsReceivedNoteDTO[i].shelfid) == string.Empty || lstGoodsReceivedNoteDTO[i].shelfid == null)
                        {
                            lstGoodsReceivedNoteDTO[i].shelfname = "";
                            lstGoodsReceivedNoteDTO[i].shelfid = "null";
                        }
                        if (Convert.ToString(lstGoodsReceivedNoteDTO[i].shelfname) == "" || Convert.ToString(lstGoodsReceivedNoteDTO[i].shelfname) == string.Empty || lstGoodsReceivedNoteDTO[i].shelfname == null || lstGoodsReceivedNoteDTO[i].shelfname == "SELECT")
                        {
                            lstGoodsReceivedNoteDTO[i].shelfname = "";
                            lstGoodsReceivedNoteDTO[i].shelfid = "null";
                        }
                        ///////////////////////////////
                        if (Convert.ToString(lstGoodsReceivedNoteDTO[i].productsubcategoryid) == "" || Convert.ToString(lstGoodsReceivedNoteDTO[i].productsubcategoryid) == string.Empty || lstGoodsReceivedNoteDTO[i].productsubcategoryid == null)
                        {
                            lstGoodsReceivedNoteDTO[i].productsubcategoryid = "null";
                            lstGoodsReceivedNoteDTO[i].subcategoryname = "";
                        }
                        if (Convert.ToString(lstGoodsReceivedNoteDTO[i].subcategoryname) == "" || Convert.ToString(lstGoodsReceivedNoteDTO[i].subcategoryname) == string.Empty || lstGoodsReceivedNoteDTO[i].subcategoryname == null || lstGoodsReceivedNoteDTO[i].subcategoryname == "SELECT")
                        {
                            lstGoodsReceivedNoteDTO[i].subcategoryname = "";
                            lstGoodsReceivedNoteDTO[i].productsubcategoryid = "null";
                        }
                    }
                    Items = string.Empty;
                    //Items = "select distinct productid, productcode,productname, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchorderuom, numorderedqty, numreceivedqty,numreturnqty,numporate, numgrnrate, storagelocationid,storagelocation,shelfid, shelfname from tabmmsgoodsreceivednotedetails;";
                    if (GoodsReceivedNoteDTO.grntype == "DIRECT")
                    {
                        if (lstGoodsReceivedNoteDTO[i].returnqty == null || lstGoodsReceivedNoteDTO[i].returnqty == "")
                        {
                            lstGoodsReceivedNoteDTO[i].returnqty = "0";
                        }
                        if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                        {
                            Items = "INSERT INTO tabmmsgoodsreceivednotedetails(grnno,vchgrnno,productid, productcode,productname, productcategoryid, categoryname, productsubcategoryid, subcategoryname,vchuom,numorderuomconversion,numorderconversionqty ,numreturnconversionqty,vchorderuom, numreceivedqty,numreturnqty, numgrnrate, storagelocationid,storagelocation,shelfid, shelfname, statusid, createdby, createddate)";
                            Items = Items + " VALUES (" + StrID + ",'" + StrCode + "'," + lstGoodsReceivedNoteDTO[i].productid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].productcode) + "','" + ManageQuote(lstGoodsReceivedNoteDTO[i].productname) + "', " + lstGoodsReceivedNoteDTO[i].productcategoryid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].categoryname) + "', " + lstGoodsReceivedNoteDTO[i].productsubcategoryid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].subcategoryname) + "','" + ManageQuote(lstGoodsReceivedNoteDTO[i].grnuom) + "'," + ManageQuote(lstGoodsReceivedNoteDTO[i].uomconversionvalue) + "," + ConversionReceivedQty + "," + ConversionReturnQty + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].orderuom) + "', " + lstGoodsReceivedNoteDTO[i].receivedqty + "," + lstGoodsReceivedNoteDTO[i].returnqty + ", " + lstGoodsReceivedNoteDTO[i].grnrate + ", " + lstGoodsReceivedNoteDTO[i].storagelocationid + ",'" + ManageQuote(lstGoodsReceivedNoteDTO[i].storagelocation) + "'," + lstGoodsReceivedNoteDTO[i].shelfid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].shelfname) + "', '1', '" + GoodsReceivedNoteDTO.CreatedBy + "', current_timestamp);";
                        }
                    }
                    else
                    {
                        if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                        {
                            Items = "INSERT INTO tabmmsgoodsreceivednotedetails(grnno,vchgrnno,productid, productcode,productname, productcategoryid, categoryname, productsubcategoryid, subcategoryname,vchuom,numorderuomconversion,numorderconversionqty ,numreturnconversionqty,vchorderuom, numorderedqty, numreceivedqty,numreturnqty,numporate, numgrnrate, storagelocationid,storagelocation,shelfid, shelfname, statusid, createdby, createddate)";
                            Items = Items + " VALUES (" + StrID + ",'" + StrCode + "'," + lstGoodsReceivedNoteDTO[i].productid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].productcode) + "','" + ManageQuote(lstGoodsReceivedNoteDTO[i].productname) + "', " + lstGoodsReceivedNoteDTO[i].productcategoryid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].categoryname) + "', " + lstGoodsReceivedNoteDTO[i].productsubcategoryid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].subcategoryname) + "','" + ManageQuote(lstGoodsReceivedNoteDTO[i].grnuom) + "'," + ManageQuote(lstGoodsReceivedNoteDTO[i].uomconversionvalue) + "," + ConversionReceivedQty + ", " + ConversionReturnQty + ",'" + ManageQuote(lstGoodsReceivedNoteDTO[i].orderuom) + "', " + lstGoodsReceivedNoteDTO[i].orderedqty + ", " + lstGoodsReceivedNoteDTO[i].receivedqty + "," + lstGoodsReceivedNoteDTO[i].returnqty + "," + lstGoodsReceivedNoteDTO[i].porate + ", " + lstGoodsReceivedNoteDTO[i].grnrate + ", " + lstGoodsReceivedNoteDTO[i].storagelocationid + ",'" + ManageQuote(lstGoodsReceivedNoteDTO[i].storagelocation) + "'," + lstGoodsReceivedNoteDTO[i].shelfid + ", '" + ManageQuote(lstGoodsReceivedNoteDTO[i].shelfname) + "', '1', '" + GoodsReceivedNoteDTO.CreatedBy + "', current_timestamp);";
                        }
                    }
                    if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                    {
                        NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                    }
                    if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                    {
                        Count = Convert.ToInt16(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, "SELECT count(*)  FROM tabmmsstockmaster where productid=" + lstGoodsReceivedNoteDTO[i].productid + " and  productname='" + ManageQuote(lstGoodsReceivedNoteDTO[i].productname) + "' and  productcode='" + ManageQuote(lstGoodsReceivedNoteDTO[i].productcode) + "' and  storagelocationid=" + lstGoodsReceivedNoteDTO[i].storagelocationid + " and  storagelocation='" + ManageQuote(lstGoodsReceivedNoteDTO[i].storagelocation) + "' and  shelfid=" + lstGoodsReceivedNoteDTO[i].shelfid + " and  shelfname='" + ManageQuote(lstGoodsReceivedNoteDTO[i].shelfname) + "' and vchmeasureduomname='" + ManageQuote(lstGoodsReceivedNoteDTO[i].orderuom) + "';"));
                    }
                    if (Count > 0)
                    {
                        if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                        {
                            Items = "Update tabmmsstockmaster set  nummeasuredqty=coalesce(nummeasuredqty,0)+" + ConversionReceivedQty + " where productid=" + lstGoodsReceivedNoteDTO[i].productid + " and  productname='" + ManageQuote(lstGoodsReceivedNoteDTO[i].productname) + "' and  productcode='" + ManageQuote(lstGoodsReceivedNoteDTO[i].productcode) + "' and  storagelocationid=" + lstGoodsReceivedNoteDTO[i].storagelocationid + " and  storagelocation='" + ManageQuote(lstGoodsReceivedNoteDTO[i].storagelocation) + "' and  shelfid=" + lstGoodsReceivedNoteDTO[i].shelfid + " and  shelfname='" + ManageQuote(lstGoodsReceivedNoteDTO[i].shelfname) + "' and vchmeasureduomname='" + ManageQuote(lstGoodsReceivedNoteDTO[i].orderuom) + "';";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                        }
                    }
                    else
                    {
                        if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                        {
                            Items = "INSERT INTO tabmmsstockmaster(productid, productname, productcode, storagelocationid, storagelocation, shelfid, shelfname, vchmeasureduomname, nummeasuredqty)";
                            Items = Items + " VALUES (" + lstGoodsReceivedNoteDTO[i].productid + ",'" + ManageQuote(lstGoodsReceivedNoteDTO[i].productname) + "','" + ManageQuote(lstGoodsReceivedNoteDTO[i].productcode) + "'," + lstGoodsReceivedNoteDTO[i].storagelocationid + ",'" + ManageQuote(lstGoodsReceivedNoteDTO[i].storagelocation) + "'," + lstGoodsReceivedNoteDTO[i].shelfid + ",'" + ManageQuote(lstGoodsReceivedNoteDTO[i].shelfname) + "','" + ManageQuote(lstGoodsReceivedNoteDTO[i].orderuom) + "'," + ConversionReceivedQty + ");";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                        }
                    }
                    // UOM CONVEERSION select productid,productcode,vchstandarduom, numstandardqty,vchconvertionuom,numconvertionqty from tabmmsproductwiseuomconvertion;
                    if (Convert.ToDecimal(lstGoodsReceivedNoteDTO[i].receivedqty) > 0)
                    {
                        if (Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, "SELECT count(*) FROM tabmmsproductwiseuomconvertion where vchstandarduom='" + lstGoodsReceivedNoteDTO[i].orderuom + "' and  vchconvertionuom='" + lstGoodsReceivedNoteDTO[i].grnuom + "' and productid=" + lstGoodsReceivedNoteDTO[i].productid + " and productname='" + lstGoodsReceivedNoteDTO[i].productname + "' and numconvertionqty=" + lstGoodsReceivedNoteDTO[i].uomconversionvalue + ";")) == 0)
                        {
                            Items = "INSERT INTO tabmmsproductwiseuomconvertion(productid,productcode,productname,vchstandarduom, numstandardqty, vchconvertionuom, numconvertionqty, createdby, createddate) values (" + lstGoodsReceivedNoteDTO[i].productid + ",'" + lstGoodsReceivedNoteDTO[i].productcode + "','" + lstGoodsReceivedNoteDTO[i].productname + "','" + lstGoodsReceivedNoteDTO[i].orderuom + "', 1, '" + lstGoodsReceivedNoteDTO[i].grnuom + "', " + lstGoodsReceivedNoteDTO[i].uomconversionvalue + "," + GoodsReceivedNoteDTO.CreatedBy + ",current_timestamp);";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                        }
                        //else
                        //{
                        //    Items = "UPDATE tabmmsproductwiseuomconvertion   SET numconvertionqty=" + lstGoodsReceivedNoteDTO[i].uomconversionvalue + "  where vchstandarduom='" + lstGoodsReceivedNoteDTO[i].orderuom + "' and  vchconvertionuom='" + lstGoodsReceivedNoteDTO[i].grnuom + "' and productid=" + lstGoodsReceivedNoteDTO[i].productid + " and productname='" + lstGoodsReceivedNoteDTO[i].productname + "';";
                        //    NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                        //}
                    }
                }
                NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, "select fntabmmsgrnaccounting('" + StrCode + "','" + ManageQuote(GoodsReceivedNoteDTO.vendorname) + "');");
                if (StrID != "")
                {
                    IsSave = true;
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                EventLogger.WriteToErrorLog(ex, "SaveGRN");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                }
            }
            return IsSave;
        }
        public DataTable getRequestPersons()
        {
            DataTable dt = new DataTable();
            try
            {
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select recordid,vchname||' '||VCHsurname as empname from tabhrmsemployeepersonaldetails where statusid=1;").Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getponumbers");
            }
            return dt;
        }
        #endregion

        #region Physical Stock Update...

        public List<PhysicalStockUpdateDTO> ShowStockusers()
        {
            List<PhysicalStockUpdateDTO> lstUsers = new List<PhysicalStockUpdateDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("select userid,username from tabuserinfo where statusid=1 order by username;", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            PhysicalStockUpdateDTO objUser = new PhysicalStockUpdateDTO();
                            objUser.Userid = Convert.ToInt32(npgdr["userid"]);
                            objUser.Username = npgdr["username"].ToString();

                            lstUsers.Add(objUser);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowStockusers");
                throw ex;

            }
            finally
            {
                npgdr.Dispose();
            }
            return lstUsers;

        }

        public List<PhysicalStockUpdateDTO> ShowStockupdate(string locationid)
        {
            List<PhysicalStockUpdateDTO> lstStockUpdate = new List<PhysicalStockUpdateDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    //using (NpgsqlCommand cmd = new NpgsqlCommand(" select productid,productname,categoryname,subcategoryname,uomname,availableqty,storagelocation,shelfname from vwgetstockdetails where storagelocationid='" + locationid + "' order by productname;", con))
                    //using (NpgsqlCommand cmd = new NpgsqlCommand("select vw.productid,vw.productname,vw.categoryname,vw.subcategoryname,vw.uomname,vw.availableqty,vw.storagelocation,vw.shelfname,vw.storagelocationid,coalesce(tbm.shelfid,'0') as shelfid,coalesce(tbm.productsubcategoryid,'0') AS  productsubcategoryid ,tbm.subcategoryname,coalesce(tbm.productcategoryid,'0') AS productcategoryid from vwgetstockdetails vw join tabmmsproductmst tbm on vw.productid=tbm.productid where vw.storagelocationid='" + locationid + "' order by productname;", con))
                    using (NpgsqlCommand cmd = new NpgsqlCommand("select vw.productid,vw.productname,vw.categoryname,vw.subcategoryname,vw.uomname,vw.availableqty,vw.storagelocation,vw.shelfname,vw.storagelocationid,case when coalesce(tbm.shelfid,'0')!='' then coalesce(tbm.shelfid,'0') else '0' end as shelfid,coalesce(tbm.productsubcategoryid,'0') AS  productsubcategoryid ,tbm.subcategoryname,coalesce(tbm.productcategoryid,'0') AS productcategoryid from vwgetstockdetails vw join tabmmsproductmst tbm on vw.productid=tbm.productid where vw.storagelocationid='" + locationid + "' order by productname;", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            PhysicalStockUpdateDTO objStockUpdate = new PhysicalStockUpdateDTO();

                            objStockUpdate.productname = npgdr["productname"].ToString();
                            objStockUpdate.productid = Convert.ToInt32(npgdr["productid"].ToString());
                            objStockUpdate.categoryname = npgdr["categoryname"].ToString();
                            objStockUpdate.subcategoryname = npgdr["subcategoryname"].ToString();
                            objStockUpdate.uom = npgdr["uomname"].ToString();
                            objStockUpdate.Storagelocation = npgdr["storagelocation"].ToString();
                            objStockUpdate.Shelfname = npgdr["shelfname"].ToString();
                            objStockUpdate.Stockinstore = Convert.ToDecimal(npgdr["availableqty"].ToString());
                            objStockUpdate.Storagelocationid = Convert.ToInt32(npgdr["storagelocationid"].ToString());
                            objStockUpdate.productcategoryid = Convert.ToInt32(npgdr["productcategoryid"].ToString());
                            objStockUpdate.productsubcategoryid = Convert.ToInt32(npgdr["productsubcategoryid"].ToString());
                            objStockUpdate.Shelfid = Convert.ToInt32(npgdr["shelfid"].ToString());
                            // objStockUpdate.PhysicalStock = Convert.ToDecimal(npgdr["physicalstock"].ToString());
                            lstStockUpdate.Add(objStockUpdate);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowItemSubCategory");
                throw ex;

            }
            finally
            {
                npgdr.Dispose();
            }
            return lstStockUpdate;

        }

        public bool SaveStock(PhysicalStockUpdateDTO PhysicalStockUpdateDTO, List<PhysicalStockUpdateDTO> lstStockupdate)
        {
            bool IsSave = false;
            con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            trans = con.BeginTransaction();
            try
            {
                string StrCode = string.Empty;
                string StrID = string.Empty;
                string strInsert = string.Empty;
                string strIndentEdit = "select substring(vchissueno,4)::int +1 from tabmmsmaterialissue order by recordid desc limit 1;";
                int vchKOTID = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit));
                PhysicalStockUpdateDTO.vchissueno = "MIS" + vchKOTID;

                string strIndentEdit3 = "select substring(vchreturnno,3)::int +1 from tabmmsmaterialreturn order by recordid desc limit 1;";
                int vchKOTID3 = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit3));
                PhysicalStockUpdateDTO.issuereturnno = "MR" + vchKOTID3;


                string strIndentEdit1 = "select substring(vchindentno,4)::int +1 from tabmmsmaterialindent order by recordid desc limit 1;";
                int indentvchKOTID = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit1));
                PhysicalStockUpdateDTO.vchindentno = "IND" + indentvchKOTID;

                for (int i = 0; i < lstStockupdate.Count; i++)
                {
                    if (lstStockupdate[i].Stockinstore >= lstStockupdate[i].PhysicalStock)
                    {
                        strInsert = "INSERT INTO tabmmsstockreconcilation(datreconcilationdate, productid, productname, vchuom,storagelocation, shelfname, numstockinsystem, numstockinstore,numvarianceqty, statusid, createdby, createddate)VALUES ('" + PhysicalStockUpdateDTO.Date + "','" + lstStockupdate[i].productid + "','" + lstStockupdate[i].productname + "','" + lstStockupdate[i].uom + "','" + lstStockupdate[i].Storagelocation + "','" + lstStockupdate[i].Shelfname + "','" + lstStockupdate[i].Stockinstore + "','" + lstStockupdate[i].PhysicalStock + "','" + lstStockupdate[i].closingstock + "',1,1,current_date) returning recordid;";
                        StrID = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));
                        string insertissue = "INSERT INTO tabmmsmaterialissue(vchissueno, vchindentno, datissuedate, vchissuetype,vchrequestedby, vchapprovalby, vchissuedby, statusid, createdby,createddate) VALUES ('" + PhysicalStockUpdateDTO.vchissueno + "','" + PhysicalStockUpdateDTO.vchindentno + "','" + PhysicalStockUpdateDTO.Date + "','PSU','" + PhysicalStockUpdateDTO.Userid + "','" + PhysicalStockUpdateDTO.Userid + "','" + PhysicalStockUpdateDTO.Userid + "',1,1,current_date) returning recordid;";
                        int issueno = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, insertissue));
                        string insertissuedetails = " INSERT INTO tabmmsmaterialissuedetails(issueno, vchissueno, productid, productcategoryid,categoryname, productsubcategoryid, subcategoryname, vchuom, vchissueuom, numissueqty, numissueconvertionqty, storagelocationid, storagelocation, shelfid, shelfname, statusid, createdby, createddate, productname) VALUES (" + issueno + ",'" + PhysicalStockUpdateDTO.vchissueno + "','" + lstStockupdate[i].productid + "','" + lstStockupdate[i].productcategoryid + "','" + lstStockupdate[i].categoryname + "','" + lstStockupdate[i].productsubcategoryid + "','" + lstStockupdate[i].subcategoryname + "','" + lstStockupdate[i].uom + "','" + lstStockupdate[i].uom + "','" + Math.Abs(lstStockupdate[i].closingstock) + "','" + Math.Abs(lstStockupdate[i].closingstock) + "','" + lstStockupdate[i].Storagelocationid + "','" + lstStockupdate[i].Storagelocation + "','" + lstStockupdate[i].Shelfid + "','" + lstStockupdate[i].Shelfname + "',1,1,current_date,'" + lstStockupdate[i].productname + "');";
                        string StrID3 = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, insertissuedetails));
                    }
                    else
                    {
                        decimal closingstock = Math.Abs(lstStockupdate[i].closingstock);
                        strInsert = "INSERT INTO tabmmsstockreconcilation(datreconcilationdate, productid, productname, vchuom,storagelocation, shelfname, numstockinsystem, numstockinstore,numvarianceqty, statusid, createdby, createddate)VALUES ('" + PhysicalStockUpdateDTO.Date + "','" + lstStockupdate[i].productid + "','" + lstStockupdate[i].productname + "','" + lstStockupdate[i].uom + "','" + lstStockupdate[i].Storagelocation + "','" + lstStockupdate[i].Shelfname + "','" + lstStockupdate[i].Stockinstore + "','" + lstStockupdate[i].PhysicalStock + "','" + lstStockupdate[i].closingstock + "',1,1,current_date) returning recordid;";
                        StrID = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));
                        string insertissuertn = " INSERT INTO tabmmsmaterialreturn( vchreturnno, datreturndate, vchreturnby, vchreceivedby,statusid, createdby, createddate) VALUES ('" + PhysicalStockUpdateDTO.issuereturnno + "','" + PhysicalStockUpdateDTO.Date + "','" + PhysicalStockUpdateDTO.Userid + "','" + PhysicalStockUpdateDTO.Userid + "',1,1,current_date) returning recordid;";
                        int isureturn = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, insertissuertn));
                        string insertissuertndetails = "INSERT INTO tabmmsmaterialreturndetails( returnno, vchreturnno, productid, productname,vchuom, vchreturnuom, vchreturnqty, vchreturnconvertionqty,storagelocation,shelfname) VALUES (" + isureturn + ",'" + PhysicalStockUpdateDTO.issuereturnno + "','" + lstStockupdate[i].productid + "','" + lstStockupdate[i].productname + "','" + lstStockupdate[i].uom + "','" + lstStockupdate[i].uom + "','" + Math.Abs(lstStockupdate[i].closingstock) + "','" + Math.Abs(lstStockupdate[i].closingstock) + "','" + lstStockupdate[i].Storagelocation + "','" + lstStockupdate[i].Shelfname + "');";
                        string StrID1 = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, insertissuertndetails));

                    }

                }
                if (StrID != "")
                {
                    IsSave = true;
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                EventLogger.WriteToErrorLog(ex, "SaveStock");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                }
            }
            return IsSave;
        }

        public List<PhysicalStockUpdateDTO> ShowStoragelocation()
        {
            List<PhysicalStockUpdateDTO> lstStore = new List<PhysicalStockUpdateDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    // string bindstorages = "select storagelocationid,storagelocationname,storagelocationcode from tabmmsstoragelocationmst  where statusid=1 and storagelocationid='" + storageareavalue + "';";
                    using (NpgsqlCommand cmd = new NpgsqlCommand("select storagelocationid,storagelocationname,storagelocationcode from tabmmsstoragelocationmst  where statusid=1;", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            PhysicalStockUpdateDTO objstore = new PhysicalStockUpdateDTO();
                            objstore.Storagelocationid = Convert.ToInt32(npgdr["storagelocationid"]);
                            objstore.Storagelocation = npgdr["storagelocationname"].ToString();
                            lstStore.Add(objstore);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowStoragelocation");
                throw ex;

            }
            finally
            {
                npgdr.Dispose();
            }
            return lstStore;

        }

        #endregion

        #region Indent Details


        public List<MaterialIndentDTO> ShowIndentProducts()
        {
            List<MaterialIndentDTO> lstIndent = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    // string strInsert = "select productid,productname,productcode,productcategoryid,categoryname,subcategoryname,productsubcategoryid,producttype,uomname,vchuomid,coalesce(minqty,0) as minqty,coalesce(maxqty,0) as maxqty,coalesce( maxqty-minqty ,0)as availaibleqnty ,storagelocationid,storagelocation,shelfid,shelfname from tabmmsproductmst  where statusid=1 ;";
                    // string strInsert = "select A.productid,A.productname,A.productcode,A.productcategoryid,A.categoryname,A.subcategoryname,A.productsubcategoryid,A.producttype,A.uomname,A.vchuomid,coalesce(A.minqty,0) as minqty,coalesce(A.maxqty,0) as maxqty,A.storagelocationid,A.storagelocation,A.shelfid,A.shelfname,B.storagelocationid,B.storagelocationcode from tabmmsproductmst A  left join tabmmsstoragelocationmst B on B.storagelocationid::text=A.storagelocationid where A.statusid=1;";
                    string strindentprodct = "select distinct A.* ,B.storagelocationid,B.storagelocationcode from (select A.productid,A.productname,A.productcode,A.productcategoryid,A.categoryname,A.subcategoryname,A.productsubcategoryid,A.producttype,A.uomname,A.vchuomid,coalesce(A.minqty,0) as minqty,coalesce(A.maxqty,0) as maxqty,(split_to_rows(A.storagelocationid,',')) as storagelocationid,A.storagelocation,A.shelfid,A.shelfname from tabmmsproductmst A where  A.statusid=1) A left join tabmmsstoragelocationmst B on A.storagelocationid=B.storagelocationid::text;";
                    npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strindentprodct);
                    while (npgdr.Read())
                    {
                        MaterialIndentDTO objIndentGeneration = new MaterialIndentDTO();
                        objIndentGeneration.productname = Convert.ToString(npgdr["productname"]);
                        objIndentGeneration.productid = npgdr["productid"] == null ? 0 : Convert.ToInt16(npgdr["productid"]);
                        objIndentGeneration.productcategoryid = npgdr["productcategoryid"] == null ? 0 : Convert.ToInt16(npgdr["productcategoryid"]);
                        objIndentGeneration.productsubcategoryid = npgdr["productsubcategoryid"].ToString() == null || npgdr["productsubcategoryid"].ToString() == string.Empty ? 0 : Convert.ToInt16(npgdr["productsubcategoryid"]);
                        objIndentGeneration.productcode = Convert.ToString(npgdr["productcode"]);
                        objIndentGeneration.vchuomid = Convert.ToString(npgdr["vchuomid"]);
                        objIndentGeneration.categoryname = Convert.ToString(npgdr["categoryname"]);
                        objIndentGeneration.subcategoryname = Convert.ToString(npgdr["subcategoryname"]);
                        objIndentGeneration.producttype = Convert.ToString(npgdr["producttype"]);
                        objIndentGeneration.uomname = Convert.ToString(npgdr["uomname"]);
                        // objIndentGeneration.minqty = Convert.ToString(npgdr["minqty"]) == null ? 0 : Convert.ToDecimal(npgdr["minqty"]);
                        objIndentGeneration.minqty = Convert.ToDecimal(npgdr["minqty"]);
                        objIndentGeneration.maxqty = Convert.ToDecimal(npgdr["maxqty"]);
                        //    objIndentGeneration.AvailableQty = Convert.ToDecimal(npgdr["availaibleqnty"]);
                        objIndentGeneration.shelfname = Convert.ToString(npgdr["shelfname"]);
                        objIndentGeneration.storagelocationcode = Convert.ToString(npgdr["storagelocationid"]);
                        objIndentGeneration.storageareavalue = Convert.ToString(npgdr["storagelocationcode"]);
                        objIndentGeneration.storagelocationname = Convert.ToString(npgdr["storagelocation"]);

                        lstIndent.Add(objIndentGeneration);
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowProducts");
                throw ex;
            }
            return lstIndent;

        }

        public List<MaterialIndentDTO> ShowStorage(string storageareavalue)
        {
            List<MaterialIndentDTO> lstStorages = new List<MaterialIndentDTO>();
            string strstaorage = string.Empty;
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    //   con.Open();
                    strstaorage = "select storagelocationid,storagelocationname,storagelocationcode from tabmmsstoragelocationmst  where statusid=1 and storagelocationcode='" + storageareavalue + "'";
                    using (npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strstaorage))
                    {
                        while (npgdr.Read())
                        {
                            MaterialIndentDTO objIndentStorages = new MaterialIndentDTO();

                            objIndentStorages.storagelocationname = Convert.ToString(npgdr["storagelocationname"]);
                            objIndentStorages.storagelocationcode = Convert.ToString(npgdr["storagelocationcode"]);
                            lstStorages.Add(objIndentStorages);
                        }
                    }
                    // con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstStorages;
        }

        public List<MaterialIndentDTO> GetSelfdetails(string showroomselected)
        {
            string strselfdata = string.Empty;
            // showroomselected = showroomselected.Split(':')[1].ToString();
            // string:SL27 'SL11'
            // showroomselected = "SL11";
            List<MaterialIndentDTO> lstSelfdata = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    // con.Open();
                    strselfdata = "select shelfname,storagearea,storagecode from tabmmsshelfmst where storagecode='" + showroomselected + "' and statusid=1 ;";
                    using (npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strselfdata))
                    {
                        while (npgdr.Read())
                        {
                            MaterialIndentDTO objIndentSelfnames = new MaterialIndentDTO();
                            objIndentSelfnames.shelfname = Convert.ToString(npgdr["shelfname"]);
                            objIndentSelfnames.storagelocationname = Convert.ToString(npgdr["storagearea"]);
                            objIndentSelfnames.storagelocationcode = Convert.ToString(npgdr["storagecode"]);
                            lstSelfdata.Add(objIndentSelfnames);
                        }
                    }
                    //con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstSelfdata;
        }

        public List<MaterialIndentDTO> showrequestedby()
        {
            string strrequestedby = string.Empty;
            List<MaterialIndentDTO> lstrequesteby = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    // strrequestedby = "select vchindentno,vchrequestedby from tabmmsmaterialindent where statusid=1";
                    strrequestedby = "SELECT vchemployeeid,vchname||' '||vchsurname as employeefullname FROM tabhrmsemployeepersonaldetails where statusid=1  ;";
                    npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strrequestedby);
                    while (npgdr.Read())
                    {
                        MaterialIndentDTO objIndentRequesteby = new MaterialIndentDTO();
                        objIndentRequesteby.vchindentno = Convert.ToString(npgdr["vchemployeeid"]);
                        objIndentRequesteby.RequestedBy = Convert.ToString(npgdr["employeefullname"]);
                        lstrequesteby.Add(objIndentRequesteby);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstrequesteby;
        }

        //public List<MaterialIndentDTO> ShowUOM1()
        //{
        //    List<MaterialIndentDTO> lstUOM = new List<MaterialIndentDTO>();
        //    try
        //    {
        //        using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
        //        {
        //            con.Open();
        //            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT VCHUOMID,VCHUOMDESCRIPTION FROM TABINVUOMMST  WHERE INTSTATUS=1 ORDER BY VCHUOMDESCRIPTION;", con))
        //            {
        //                npgdr = cmd.ExecuteReader();
        //                while (npgdr.Read())
        //                {
        //                    MaterialIndentDTO objUOM = new MaterialIndentDTO();
        //                    objUOM.uomid = Convert.ToString(npgdr["VCHUOMID"]);
        //                    objUOM.uom = npgdr["VCHUOMDESCRIPTION"].ToString();
        //                    // objUOM.uomabbreviation = npgdr["uomabbreviation"].ToString();

        //                    lstUOM.Add(objUOM);
        //                }
        //            }
        //            con.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.WriteToErrorLog(ex, "ShowUOM");
        //        throw ex;
        //    }
        //    finally
        //    {
        //        npgdr.Dispose();
        //    }
        //    return lstUOM;
        //}


        public bool SaveIndent(List<MaterialIndentDTO> griddata, MaterialIndentDTO BE, out string indentno)
        {
            bool issaved = false;
            indentno = "";
            string strsave = string.Empty;
            string strsavedetails = string.Empty;
            try
            {
                con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                trans = con.BeginTransaction();
                int count = 0;
                if (BE.Indenttypenew == "New" || BE.Indenttypenew == "reorder")
                {
                    string strIndentEdit = "select substring(vchindentno,4)::int +1 from tabmmsmaterialindent order by recordid desc limit 1;";
                    int vchKOTID = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit));
                    BE.vchindentno = "IND" + vchKOTID;

                    strsave = "INSERT INTO tabmmsmaterialindent(vchindentno,datindentdate,vchindenttype,vchrequestedby, vchapprovalby, statusid, createdby, createddate,vchdepartment)VALUES('" + ManageQuote(BE.vchindentno) + "', CURRENT_TIMESTAMP, ";
                    if (BE.Indenttypenew == "New" || BE.Indenttypenew == "reorder")
                    {
                        strsave = strsave + " '" + ManageQuote(BE.Indenttypenew) + "', '" + ManageQuote(BE.RequestedBy) + "', '" + ManageQuote(BE.ApprovedBy) + "', '1', '" + BE.createdby + "', CURRENT_TIMESTAMP,'" + ManageQuote(BE.DeparmentName) + "' ) returning recordid;";
                        count = Convert.ToInt16(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strsave));

                        int j = griddata.Count;
                        for (int i = 0; i < griddata.Count; i++)
                        {
                            if (count != 0)
                            {
                                string AvailableQty = Convert.ToString(griddata[i].AvailableQty);
                                if (AvailableQty == "")
                                {
                                    griddata[i].AvailableQty = 0;
                                }
                                string strproductsubcategoryid = string.Empty;
                                string strproductsubcategoryname = string.Empty;
                                if (griddata[i].productsubcategoryid == null)
                                {
                                    strproductsubcategoryid = "null";
                                    strproductsubcategoryname = "null";
                                }
                                else
                                {
                                    strproductsubcategoryid = Convert.ToString(griddata[i].productsubcategoryid);
                                }
                                strsavedetails = "INSERT INTO tabmmsmaterialindentdetails(indentno, vchindentno, productid, productcode, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchuom,  numindentqty, numindentconvertionqty, statusid, createdby, createddate, productname,storagelocationname,storagelocationcode,shelfname,AvailableQty,vchmaxstock,vchminstock,vchproductype)VALUES ('" + count + "', '" + ManageQuote(BE.vchindentno) + "', " + griddata[i].productid + ", '" + ManageQuote(griddata[i].productcode) + "', " + griddata[i].productcategoryid + ", '" + ManageQuote(griddata[i].categoryname) + "', " + strproductsubcategoryid + ", '" + ManageQuote(griddata[i].subcategoryname) + "', '" + ManageQuote(griddata[i].uomname) + "',  '" + griddata[i].indentqty + "', '0', '1', '" + BE.createdby + "',CURRENT_TIMESTAMP, '" + griddata[i].productname + "','" + griddata[i].storagetext + "','" + griddata[i].storagelocationname + "','" + griddata[i].shelfname + "','" + griddata[i].AvailableQty + "','" + griddata[i].maxqty + "','" + griddata[i].minqty + "','" + griddata[i].producttype + "') returning vchindentno ;";
                                indentno = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strsavedetails));
                                issaved = true;
                            }
                            else
                            {
                                issaved = false;
                                indentno = "";
                            }
                        }
                    }
                }
                #region Old Updadate Code


                //if (BE.Indenttypenew == "modify")
                //{

                //    strsave = "update tabmmsmaterialindent set vchindentno='" + griddata[0].IndentNo + "',datindentdate='" + BE.vchdate.ToString("dd-MM-yyyy") + "',vchindenttype='" + BE.Indenttypenew + "',vchrequestedby='" + BE.RequestedBy + "', vchapprovalby='" + BE.ApprovedBy + "', statusid='1',modifiedby='" + BE.createdby + "',modifieddate=CURRENT_TIMESTAMP,vchdepartment='" + BE.DeparmentName + "' where vchindentno='" + griddata[0].IndentNo + "' ;";
                //    //  strsave = strsave + " '" + BE.Indenttypeexisting + "', '" + BE.RequestedBy + "', '" + BE.ApprovedBy + "', '1', '" + BE.createdby + "', CURRENT_TIMESTAMP,'" + BE.DeparmentName + "') returning recordid;";
                //    count = Convert.ToInt16(NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strsave));

                //    int j = griddata.Count;
                //    for (int i = 0; i < griddata.Count; i++)
                //    {
                //        if (count != 0)
                //        {
                //            string AvailableQty = Convert.ToString(griddata[i].AvailableQty);
                //            if (AvailableQty == "")
                //            {
                //                griddata[i].AvailableQty = 0;
                //            }

                //            // strsavedetails = "INSERT INTO tabmmsmaterialindentdetails(indentno, vchindentno, productid, productcode, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchuom,  numindentqty, numindentconvertionqty, statusid, createdby, createddate, productname,storagelocationname,storagelocationcode,shelfname,AvailableQty)VALUES 
                //            //  ('" + count + "', '" + BE.vchindentno + "', '" + griddata[i].productid + "', '" + griddata[i].productcode + "', '" + griddata[i].productcategoryid + "', '" + griddata[i].categoryname.ToString() + "', '" + griddata[i].productsubcategoryid + "', '" + griddata[i].subcategoryname.ToString() + "', '" + griddata[i].uomname.ToString() + "',  '" + griddata[i].indentqty + "', '22', '1', '" + BE.createdby + "',CURRENT_TIMESTAMP, '" + griddata[i].productname + "','" + griddata[i].storagetext + "','" + griddata[i].storagelocationname + "','" + griddata[i].shelfname + "','" + griddata[i].AvailableQty + "');";
                //            strsavedetails = "update tabmmsmaterialindentdetails set  productid='" + griddata[i].productid + "', productcode='" + griddata[i].productcode + "', productcategoryid='" + griddata[i].productcategoryid + "', categoryname='" + griddata[i].categoryname.ToString() + "', productsubcategoryid='" + griddata[i].productsubcategoryid + "', subcategoryname='" + griddata[i].subcategoryname.ToString() + "', vchuom='" + griddata[i].uomname.ToString() + "',  numindentqty='" + griddata[i].indentqty + "', numindentconvertionqty='22', statusid='1', modifiedby='" + BE.createdby + "',modifieddate=CURRENT_TIMESTAMP, productname='" + griddata[i].productname + "',storagelocationname='" + griddata[i].storagetext + "',storagelocationcode='" + griddata[i].storagelocationname + "',shelfname='" + griddata[i].shelfname + "',AvailableQty='" + griddata[i].AvailableQty + "' where vchindentno='" + griddata[i].IndentNo + "' and recordid='" + griddata[i].recordid2 + "'";
                //            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strsavedetails);
                //            issaved = true;
                //        }
                //        else
                //        {
                //            issaved = false;
                //        }
                //    }
                //}
                #endregion
                #region Old Saving code


                //count = Convert.ToInt16(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strsave));

                //int j = griddata.Count;
                //for (int i = 0; i < griddata.Count; i++)
                //{
                //    if (count != 0)
                //    {
                //        string AvailableQty = Convert.ToString(griddata[i].AvailableQty);
                //        if (AvailableQty == "")
                //        {
                //            griddata[i].AvailableQty = 0;
                //        }

                //        strsavedetails = "INSERT INTO tabmmsmaterialindentdetails(indentno, vchindentno, productid, productcode, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchuom,  numindentqty, numindentconvertionqty, statusid, createdby, createddate, productname,storagelocationname,storagelocationcode,shelfname,AvailableQty)VALUES ('" + count + "', '" + BE.vchindentno + "', '" + griddata[i].productid + "', '" + griddata[i].productcode + "', '" + griddata[i].productcategoryid + "', '" + griddata[i].categoryname.ToString() + "', '" + griddata[i].productsubcategoryid + "', '" + griddata[i].subcategoryname.ToString() + "', '" + griddata[i].uomname.ToString() + "',  '" + griddata[i].indentqty + "', '22', '1', '" + BE.createdby + "',CURRENT_TIMESTAMP, '" + griddata[i].productname + "','" + griddata[i].storagetext + "','" + griddata[i].storagelocationname + "','" + griddata[i].shelfname + "','" + griddata[i].AvailableQty + "');";
                //        NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strsavedetails);
                //        issaved = true;
                //    }
                //    else
                //    {
                //        issaved = false;
                //    }
                //}
                #endregion

                if (issaved == true)
                {
                    trans.Commit();
                }
                return issaved;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                    trans.Dispose();
                }
            }
            return issaved;
        }

        public bool UpdateIndentDetails(List<MaterialIndentDTO> griddata, MaterialIndentDTO BE)
        {

            bool isupdated = false;
            string strupdate = string.Empty;
            string strupdatedetails = string.Empty;
            string strdelete = string.Empty;
            try
            {
                con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                trans = con.BeginTransaction();
                if (BE.Indenttypenew == "modify")
                {
                    string indentnumber = griddata[0].IndentNo.ToString();
                    int count2 = 0;
                    int count = 0;
                    strupdate = "update tabmmsmaterialindent  set datindentdate=CURRENT_TIMESTAMP,vchindenttype='" + BE.Indenttypenew + "',vchrequestedby='" + BE.RequestedBy + "',vchapprovalby='" + BE.ApprovedBy + "',modifiedby='" + BE.createdby + "',modifieddate=CURRENT_TIMESTAMP,vchdepartment='" + BE.DeparmentName + "' where vchindentno='" + griddata[0].IndentNo + "'  returning recordid;";
                    count = Convert.ToInt16(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strupdate));
                    if (count != 0)
                    {
                        strdelete = "delete from tabmmsmaterialindentdetails where vchindentno='" + griddata[0].IndentNo + "';";
                        count2 = Convert.ToInt16(NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strdelete));
                    }
                    if (count2 != 0)
                    {
                        int j = griddata.Count;
                        for (int i = 0; i < griddata.Count; i++)
                        {
                            string AvailableQty = Convert.ToString(griddata[i].AvailableQty);
                            if (AvailableQty == "")
                            {
                                griddata[i].AvailableQty = 0;
                            }
                            string strproductsubcategoryid = string.Empty;
                            string strproductsubcategoryname = string.Empty;
                            if (griddata[i].productsubcategoryid == null)
                            {
                                strproductsubcategoryid = "null";
                                strproductsubcategoryname = "null";
                            }
                            else
                            {
                                strproductsubcategoryid = Convert.ToString(griddata[i].productsubcategoryid);
                            }
                            strupdatedetails = "INSERT INTO tabmmsmaterialindentdetails(indentno, vchindentno, productid, productcode, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchuom,  numindentqty, numindentconvertionqty, statusid, createdby, createddate, productname,storagelocationname,storagelocationcode,shelfname,AvailableQty,vchmaxstock,vchminstock,vchproductype)VALUES ('" + count + "', '" + griddata[i].IndentNo + "', '" + griddata[i].productid + "', '" + griddata[i].productcode + "', '" + griddata[i].productcategoryid + "', '" + griddata[i].categoryname + "', " + strproductsubcategoryid + ", '" + griddata[i].subcategoryname + "', '" + griddata[i].uomname + "',  '" + griddata[i].indentqty + "', '22', '1', '" + BE.createdby + "',CURRENT_TIMESTAMP, '" + griddata[i].productname + "','" + griddata[i].storagetext + "','" + griddata[i].storagelocationname + "','" + griddata[i].shelfname + "','" + griddata[i].AvailableQty + "','" + griddata[i].maxqty + "','" + griddata[i].minqty + "','" + griddata[i].producttype + "');";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strupdatedetails);
                            isupdated = true;
                        }
                    }
                    else
                    {
                        isupdated = false;
                    }
                }
                if (isupdated == true)
                {
                    trans.Commit();
                }
                return isupdated;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                    trans.Dispose();
                }
            }


        }

        public bool Deleteproductdetails(MaterialIndentDTO gridrowdata)
        {
            bool isdeleted = false;
            string strdeleted = string.Empty;
            try
            {
                con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                trans = con.BeginTransaction();
                con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                trans = con.BeginTransaction();

                strdeleted = "update tabmmsmaterialindentdetails set statusid=2 where  recordid='" + gridrowdata.recordid2 + "'";
                NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strdeleted);
                isdeleted = true;

                if (isdeleted == true)
                {
                    trans.Commit();
                }

            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                    trans.Dispose();
                }
            }
            return isdeleted;
        }

        public List<MaterialIndentDTO> GetExistingIndentNo(string IndentType)
        {
            List<MaterialIndentDTO> lstIndentDetails = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {

                    string strindentdetails = "select TA.recordid,TA.vchindentno,TA.datindentdate,TA.vchindenttype,TA.vchrequestedby,TA.vchapprovalby,TA.vchdepartment,TA.statusid,TA.createdby,TA.createddate,TA.modifiedby,TA.modifieddate,TB.recordid as recordid2,TB.indentno as indentno2,TB.vchindentno as vchindentno2,TB.productid as productid2,TB.productcode as productcode2,TB.productcategoryid as productcategoryid2,TB.categoryname as categoryname2,TB.productsubcategoryid as productsubcategoryid2,TB.subcategoryname as subcategoryname2,TB.vchuom as vchuom2,TB.vchindentuom as vchindentuom2,TB.numindentqty as numindentqty2,TB.numindentconvertionqty as numindentconvertionqty2,TB.statusid as statusid2,TB.createdby as createdby2,TB.AvailableQty as AvailableQty2,TB.createddate as createddate2,TB.modifiedby as modifiedby2,TB.modifieddate as modifieddate2,TB.productname as productname2 ,TB.storagelocationname as storagelocationname2,TB.storagelocationcode as storagelocationcode2,TB.shelfname as shelfname2,vw.availableqty as  availableqty2,TB.vchmaxstock as vchmaxstock2,TB.vchminstock as vchminstock2,TB.vchproductype as vchproductype2 from tabmmsmaterialindent TA   join  tabmmsmaterialindentdetails TB on TA.recordid= TB.indentno left join vwgetstockdetails vw on tb.productid=vw.productid and tb.storagelocationname=vw.storagelocation and coalesce(tb.shelfname,'')=coalesce(vw.shelfname,'') where TB.vchindentno='" + IndentType + "'and  TB.statusid=1  ;";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(strindentdetails, con))
                    {
                        con.Open();
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            MaterialIndentDTO objIndentDetails = new MaterialIndentDTO();
                            objIndentDetails.RequestedBy = Convert.ToString(npgdr["vchrequestedby"]);
                            objIndentDetails.ApprovedBy = Convert.ToString(npgdr["vchapprovalby"]);

                            objIndentDetails.recordid = Convert.ToInt16(npgdr["indentno2"]);
                            objIndentDetails.IndentNo = Convert.ToString(npgdr["vchindentno2"]);
                            objIndentDetails.productid = Convert.ToInt16(npgdr["productid2"]);
                            objIndentDetails.productcode = Convert.ToString(npgdr["productcode2"]);
                            //productcategoryid2,categoryname2,productsubcategoryid2,subcategoryname2,vchuom2,vchindentuom2
                            //numindentqty2,numindentconvertionqty2,productname2

                            objIndentDetails.categoryname = Convert.ToString(npgdr["categoryname2"]);


                            objIndentDetails.uomname = Convert.ToString(npgdr["vchuom2"]);
                            objIndentDetails.vchuomid = Convert.ToString(npgdr["vchindentuom2"]);
                            objIndentDetails.indentqty = Convert.ToDecimal(npgdr["numindentqty2"]);
                            // objIndentDetails.maxqty = Convert.ToDecimal(npgdr["numindentconvertionqty2"]);
                            objIndentDetails.productname = Convert.ToString(npgdr["productname2"]);
                            if (npgdr["AvailableQty2"] != DBNull.Value)
                            {
                                objIndentDetails.AvailableQty = Convert.ToDecimal(npgdr["AvailableQty2"]);
                            }
                            objIndentDetails.shelfname = Convert.ToString(npgdr["shelfname2"]);
                            objIndentDetails.storagetext = Convert.ToString(npgdr["storagelocationname2"]);
                            objIndentDetails.recordid2 = Convert.ToInt16(npgdr["recordid2"]);
                            objIndentDetails.storagelocationcode = Convert.ToString(npgdr["storagelocationcode2"]);

                            //  objIndentGeneration.productsubcategoryid = npgdr["productsubcategoryid"].ToString() == null || npgdr["productsubcategoryid"].ToString() == string.Empty ? 0 : Convert.ToInt16(npgdr["productsubcategoryid"]);

                            objIndentDetails.maxqty = npgdr["vchmaxstock2"] == DBNull.Value ? 0 : Convert.ToDecimal(npgdr["vchmaxstock2"]);
                            objIndentDetails.minqty = npgdr["vchminstock2"] == DBNull.Value ? 0 : Convert.ToDecimal(npgdr["vchminstock2"]);

                            if (npgdr["productcategoryid2"] != DBNull.Value)
                            {
                                objIndentDetails.productcategoryid = Convert.ToInt16(npgdr["productcategoryid2"]);
                            }
                            if (npgdr["productsubcategoryid2"] != DBNull.Value)
                            {
                                objIndentDetails.productsubcategoryid = Convert.ToInt16(npgdr["productsubcategoryid2"]);
                            }
                            if (npgdr["subcategoryname2"] != DBNull.Value)
                            {
                                objIndentDetails.subcategoryname = Convert.ToString(npgdr["subcategoryname2"]);
                            }
                            if (npgdr["storagelocationcode2"] != DBNull.Value)
                            {
                                objIndentDetails.storagelocationname = Convert.ToString(npgdr["storagelocationcode2"]);
                            }

                            // objIndentDetails.subcategoryname = Convert.ToString(npgdr["subcategoryname2"]);
                            // objIndentDetails.productcategoryid = npgdr["productcategoryid2"] == DBNull.Value ? 0 : Convert.ToInt16(npgdr["productsubcategoryid2"]);
                            // objIndentDetails.productsubcategoryid = npgdr["productsubcategoryid2"] == DBNull.Value ? 0 : Convert.ToInt16(npgdr["productsubcategoryid2"]);

                            objIndentDetails.producttype = Convert.ToString(npgdr["vchproductype2"]);
                            objIndentDetails.DeparmentName = Convert.ToString(npgdr["vchdepartment"]);
                            //  missing this AvailableQty
                            lstIndentDetails.Add(objIndentDetails);

                        }
                        con.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndentDetails");
                throw ex;
            }
            finally
            {


            }
            return lstIndentDetails;
        }

        public List<MaterialIndentDTO> GetExistingIndents(string Indents)
        {
            List<MaterialIndentDTO> lstIndents = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    //  using (NpgsqlCommand cmd = new NpgsqlCommand("select TA.recordid,TA.vchindentno,TA.statusid  from tabmmsmaterialindent TA join tabmmsmaterialissue TB on TB.vchindentno!=TA.vchindentno ;", con))
                    using (NpgsqlCommand cmd = new NpgsqlCommand("select recordid,vchindentno from tabmmsmaterialindent where statusid=1 and vchindentno not in (select vchindentno from tabmmsmaterialissue ) order by coalesce(replace(vchindentno,'IND',''),'0')::int;", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            MaterialIndentDTO objIndents = new MaterialIndentDTO();
                            objIndents.recordid = Convert.ToInt16(npgdr["recordid"]);
                            objIndents.IndentNo = Convert.ToString(npgdr["vchindentno"]);
                            lstIndents.Add(objIndents);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndentDetails");
                throw ex;
            }
            finally
            {

                //     trans.Dispose();

            }
            return lstIndents;

        }

        //public List<MaterialIndentDTO> GetProductAvailability(string productid, string productname, string showroomselected, string storagelocationname, string uomname)
        //{
        //    List<MaterialIndentDTO> lstIndents = new List<MaterialIndentDTO>();
        //    try
        //    {
        //        using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
        //        {
        //            con.Open();
        //            string asdflaksdf = "select productid,productname,categoryname,subcategoryname,uomname,storagelocation,shelfname,availableqty from vwgetstockdetails where productid='" + productid + "' and productname='" + productname + "' and uomname='" + uomname + "' and storagelocation='" + storagelocationname + "'; ";
        //            using (NpgsqlCommand cmd = new NpgsqlCommand("select productid,productname,categoryname,subcategoryname,uomname,storagelocation,shelfname,availableqty from vwgetstockdetails where productid='" + productid + "' and productname='" + productname + "' and uomname='" + uomname + "' and storagelocation='" + storagelocationname + "' ;", con))
        //            {
        //                npgdr = cmd.ExecuteReader();
        //                while (npgdr.Read())
        //                {
        //                    MaterialIndentDTO objprdctavilable = new MaterialIndentDTO();
        //                    objprdctavilable.AvailableQty = Convert.ToInt16(npgdr["availableqty"]);

        //                    if (objprdctavilable.AvailableQty == null)
        //                    {
        //                        objprdctavilable.AvailableQty = 0;
        //                    }
        //                    // objprdctavilable.IndentNo = Convert.ToString(npgdr["vchindentno"]);
        //                    lstIndents.Add(objprdctavilable);
        //                }
        //            }
        //            con.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.WriteToErrorLog(ex, "ShowIndentDetails");
        //        throw ex;
        //    }
        //    finally
        //    {
        //        npgdr.Dispose();
        //    }
        //    return lstIndents;
        //}


        //public decimal GetProductAvailabilityQty(string productid, string productname, string uomname, string storagearea, string shelfname)
        //{
        //    List<MaterialIndentDTO> lstIndents = new List<MaterialIndentDTO>();
        //    string stravailqty = string.Empty;
        //    decimal availableqty = 0;
        //    try
        //    {
        //        using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
        //        {
        //            con.Open();
        //            if (shelfname != "")
        //            {
        //                stravailqty = "select coalesce(availableqty,0) as availableqty from vwgetstockdetails where productid='" + productid + "' and productname='" + productname + "' and uomname='" + uomname + "' and storagelocation='" + storagearea + "' and shelfname='" + shelfname + "'; ";
        //            }
        //            else
        //            {

        //                stravailqty = "select coalesce(availableqty,0) as availableqty from vwgetstockdetails where productid='" + productid + "' and productname='" + productname + "' and uomname='" + uomname + "' and storagelocation='" + storagearea + "'; ";
        //            }
        //            using (NpgsqlCommand cmd = new NpgsqlCommand(stravailqty, con))
        //            {
        //                //npgdr = cmd.ExecuteReader();
        //                //while (npgdr.Read())
        //                //{
        //                //    MaterialIndentDTO objprdctavilable = new MaterialIndentDTO();
        //                //    objprdctavilable.AvailableQty = Convert.ToDecimal(npgdr["availableqty"]);

        //                //    if (objprdctavilable.AvailableQty == null)
        //                //    {
        //                //        objprdctavilable.AvailableQty = 0;
        //                //    }
        //                //    // objprdctavilable.IndentNo = Convert.ToString(npgdr["vchindentno"]);
        //                //    lstIndents.Add(objprdctavilable);
        //                //}

        //                availableqty = Convert.ToDecimal(cmd.ExecuteScalar());

        //            }

        //            con.Close();


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLogger.WriteToErrorLog(ex, "ShowIndentDetails");
        //        throw ex;
        //    }
        //    finally
        //    {
        //        //     trans.Dispose();

        //    }
        //    return availableqty;
        //}
        public List<MaterialIndentDTO> ShowDepartment()
        {
            List<MaterialIndentDTO> lstDepartmentdetails = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {

                    string strInsert = "SELECT DEPARTMENTID,DEPARTMENTNAME,DEPARTMENTCODE FROM TABPOSDEPARTMENTMST WHERE STATUSID=1 ORDER BY DEPARTMENTID desc;";
                    npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strInsert);
                    while (npgdr.Read())
                    {
                        MaterialIndentDTO objDepartmentDTO = new MaterialIndentDTO();
                        objDepartmentDTO.deptid = Convert.ToInt32(npgdr["DEPARTMENTID"]);
                        objDepartmentDTO.DeparmentName = npgdr["DEPARTMENTNAME"].ToString();
                        objDepartmentDTO.DeparmentCode = Convert.ToString(npgdr["DEPARTMENTCODE"]);

                        lstDepartmentdetails.Add(objDepartmentDTO);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowDepartment");
                throw ex;
                // ErrorMessage(ex);
            }
            finally
            {
                npgdr.Dispose();
            }
            return lstDepartmentdetails;

        }
        //public List<MaterialIndentDTO> GetProductAvailability(string productid, string productname, string showroomselected, string storagelocationname, string uomname)
        //{
        //    throw new NotImplementedException();
        //}


        public List<MaterialIndentDTO> GetReorderIndents(string indenttype)
        {
            List<MaterialIndentDTO> lstreorderindents = new List<MaterialIndentDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(" select recordid,vchindentno from tabmmsmaterialindent order by coalesce( replace(vchindentno,'IND',''),'0')::int ;", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            MaterialIndentDTO objreorderindent = new MaterialIndentDTO();
                            objreorderindent.recordid = Convert.ToInt16(npgdr["recordid"]);
                            objreorderindent.IndentNo = Convert.ToString(npgdr["vchindentno"]);
                            lstreorderindents.Add(objreorderindent);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowReorderIndentDetails");
                throw ex;
            }
            finally
            {

                //     trans.Dispose();

            }
            return lstreorderindents;
        }

        public List<MaterialIndentDTO> GetAvailablestock()
        {
            List<MaterialIndentDTO> lstvwavailble = new List<MaterialIndentDTO>();
            try
            {
                string strgetavailble = string.Empty;

                strgetavailble = "select productid,productname,categoryname,uomname,storagelocation,shelfname,availableqty,storagelocationid from vwgetstockdetails ;";
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(strgetavailble, con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            MaterialIndentDTO objavaible = new MaterialIndentDTO();
                            objavaible.productid = Convert.ToInt16(npgdr["productid"]);
                            objavaible.productname = Convert.ToString(npgdr["productname"]);
                            objavaible.categoryname = Convert.ToString(npgdr["categoryname"]);
                            objavaible.uomname = Convert.ToString(npgdr["uomname"]);
                            objavaible.storagelocationname = Convert.ToString(npgdr["storagelocation"]);
                            objavaible.shelfname = Convert.ToString(npgdr["shelfname"]);
                            objavaible.AvailableQty = Convert.ToDecimal(npgdr["availableqty"]);
                            lstvwavailble.Add(objavaible);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstvwavailble;
        }

        #endregion

        #region Purchase Order
        public DataTable getexistponumbers(string Type, string Vendorid)
        {
            DataTable dt = new DataTable();
            try
            {
                if (Type == "REORDER")
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select recordid as poid,vchpurchaseorderno as pono from tabmmspurchaseorder where vendorid=" + Vendorid + ";").Tables[0];
                }
                else if (Type == "MODIFY")
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select recordid as poid,vchpurchaseorderno as pono from tabmmspurchaseorder where vchpurchaseorderno not in (select vchpurchaseorderno from tabmmsgoodsreceivednote where vchgrntype='PO' ) and  statusid =1 and vendorid=" + Vendorid + ";").Tables[0];
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getexistponumbers");
            }
            return dt;
        }
        public DataTable getpodetails(string poid, string Vendorid)
        {
            DataTable dt = new DataTable();
            try
            {
                if (poid != "" && poid != null && poid != string.Empty && Vendorid != "" && Vendorid != null && Vendorid != string.Empty)
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select  orderid,vchpurchaseorderno,productid,productname,productcategoryid,categoryname,productsubcategoryid,subcategoryname,vchuom,vchorderuom,numorderedqty,datdeliverybefore,numrate,productcode from tabmmspurchaseorderdetails  where orderid=" + poid + " and productid in (select distinct  productid from tabmmsproductvendors  where vendorid=" + Vendorid + ");").Tables[0];
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getpodetails");
            }
            return dt;
        }
        public PurchaseOrderTAXDTO getpotaxdetails(string poid, string Vendorid)
        {
            DataTable dt = new DataTable();
            PurchaseOrderTAXDTO TX = new PurchaseOrderTAXDTO();
            try
            {

                if (poid != "" && poid != null && poid != string.Empty && Vendorid != "" && Vendorid != null && Vendorid != string.Empty)
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select numtransportcharges, discountamount, excisetaxpercentage, excisetaxamount, cesstaxpercentage, cesstaxamount, shcesstaxpercentage, shcesstaxamount, taxtype, vatorcsttaxpercentage, vatorcsttaxamount, basicamount, totalamount, numdueamount,vchdiscounttype,numdiscountvalue,vchtaxtype,vchplaceofdelivery,vchterms FROM tabmmspurchaseorder  where recordid=" + poid + ";").Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        TX.TransportCharges = Convert.ToString(dt.Rows[0]["numtransportcharges"]);
                        TX.DiscountValue = Convert.ToString(dt.Rows[0]["discountamount"]);
                        TX.TaxExcisePercentage = Convert.ToString(dt.Rows[0]["excisetaxpercentage"]);
                        TX.TaxExciseAmount = Convert.ToString(dt.Rows[0]["excisetaxamount"]);
                        TX.TaxCESSPercentage = Convert.ToString(dt.Rows[0]["cesstaxpercentage"]);
                        TX.TaxCESSAmount = Convert.ToString(dt.Rows[0]["cesstaxamount"]);
                        TX.TaxSHCESSPercentage = Convert.ToString(dt.Rows[0]["shcesstaxpercentage"]);
                        TX.TaxSHCESSAmount = Convert.ToString(dt.Rows[0]["shcesstaxamount"]);
                        TX.vatorcst = Convert.ToString(dt.Rows[0]["taxtype"]);
                        TX.taxvatcst = Convert.ToString(dt.Rows[0]["vatorcsttaxpercentage"]);
                        TX.TaxvatorcstAmount = Convert.ToString(dt.Rows[0]["vatorcsttaxamount"]);
                        TX.BasicAmount = Convert.ToString(dt.Rows[0]["basicamount"]);
                        TX.TotalAmount = Convert.ToString(dt.Rows[0]["totalamount"]);
                        //TX.du = Convert.ToString(dt.Rows[0]["numdueamount"]);
                        TX.DiscountType = Convert.ToString(dt.Rows[0]["vchdiscounttype"]);
                        TX.DiscountFlatPercentage = Convert.ToString(dt.Rows[0]["numdiscountvalue"]);
                        TX.taxtype = Convert.ToString(dt.Rows[0]["vchtaxtype"]);
                        TX.PlaceofDelivery = Convert.ToString(dt.Rows[0]["vchplaceofdelivery"]);
                        TX.TermsandConditions = Convert.ToString(dt.Rows[0]["vchterms"]);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getpotaxdetails");
            }
            return TX;
        }
        #endregion



        /// <summary>
        /// Vikram Chathilla
        /// </summary>
        /// <returns></returns>
        #region IndentRelease

        public List<IndentDTO> ShowProducts()
        {
            List<IndentDTO> lstIndentProducts = new List<IndentDTO>();
            try
            {
                string strSelect = "select productid,productname,productcode from tabmmsproductmst where statusid=1;";
                //string strSelect = "select productid,productname from tabmmsproductmst where statusid=1;";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect);
                while (npgdr.Read())
                {
                    IndentDTO objProduct = new IndentDTO();
                    objProduct.productname = npgdr["productname"].ToString();
                    objProduct.productid = (npgdr["productid"]).ToString();
                    objProduct.productcode = Convert.ToString(npgdr["productcode"]);
                    lstIndentProducts.Add(objProduct);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowProducts");
            }
            finally
            {
                npgdr.Dispose();
            }
            return lstIndentProducts;
        }

        public List<IndentDTO> ShowIndentNumbers()
        {
            List<IndentDTO> lstIndennos = new List<IndentDTO>();
            try
            {
                // string strSelect = " select vchindentno,vchindenttype from tabmmsmaterialindent where statusid =1;";
                // string strSelect = "select distinct vchindentno,* from vwbindgridmaterialissuedetails where coalesce(numissueqty,0) != coalesce(previssueqty,0);";
                string strSelect = "select distinct vchindentno ,coalesce(replace(vchindentno,'IND',''),'0')::int as orderno from vwbindgridmaterialissuedetails   where pendingqty >0 order by orderno;";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect);
                while (npgdr.Read())
                {
                    IndentDTO objProduct = new IndentDTO();
                    // objProduct.vchindenttype = npgdr["vchindenttype"].ToString();
                    objProduct.vchindentno = Convert.ToString(npgdr["vchindentno"]);
                    lstIndennos.Add(objProduct);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndents");
            }
            finally
            {
                npgdr.Dispose();
            }
            return lstIndennos;
        }


        public List<IndentDTO> ShowIndentGridDetails(string objindentdto)
        {
            List<IndentDTO> objlstindentgrid = new List<IndentDTO>();
            try
            {
                string StrSelect = string.Empty;
                StrSelect = "select * from (select sum(coalesce(vw1.previssueqty,0)) as previssueqty,vw1.vchdepartment,vw1.productid,vw1.productid,vw1.vchindentno,vw1.vchrequestedby,vw1.vchapprovalby,vw1.productcode,vw1.categoryname,vw1.subcategoryname,vw1.producttype,vw1.vchuom,vw1.minqty,vw1.maxqty,vw1.storagelocation,vw1.numindentqty,coalesce(vw2.availableqty,0) as availableqty,vw1.productcategoryid,vw1.productsubcategoryid,vw2.storagelocation,vw2.shelfname,vw2.storagelocationid from vwbindgridmaterialissuedetails vw1 left join vwgetstockdetails vw2 on vw1.productid=vw2.productid and vw1.vchuom=vw2.uomname and vw2.storagelocation=vw1.storagelocation  where vchindentno='" + objindentdto + "' and vw1.numindentqty != vw1.previssueqty   group by vw1.productid,vw1.productid,vw1.vchindentno,vw1.vchrequestedby,vw1.vchapprovalby,vw1.productcode,vw1.categoryname,vw1.subcategoryname,vw1.producttype,vw1.vchuom,vw1.minqty,vw1.maxqty,vw1.storagelocation,vw1.numindentqty,coalesce(vw2.availableqty,0),vw1.productcategoryid,vw1.productsubcategoryid,vw2.storagelocation,vw2.shelfname,vw2.storagelocationid,vw1.vchdepartment) x where x.previssueqty != x.numindentqty order by 1;";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, StrSelect);
                while (npgdr.Read())
                {
                    IndentDTO objProduct = new IndentDTO();
                    objProduct.vchrequestedby = npgdr["vchrequestedby"].ToString();
                    objProduct.vchapprovalby = Convert.ToString(npgdr["vchapprovalby"]);
                    string strNewQuery = "SELECT PRODUCTNAME from tabmmsproductmst where productcode='" + ManageQuote(npgdr["productcode"].ToString()) + "';";
                    string productname = NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strNewQuery).ToString();
                    objProduct.productname = productname;
                    objProduct.productcode = npgdr["productcode"].ToString();
                    objProduct.categoryname = Convert.ToString(npgdr["categoryname"]);
                    objProduct.subcategoryname = npgdr["subcategoryname"].ToString();
                    objProduct.producttype = npgdr["producttype"].ToString();
                    objProduct.uom = npgdr["vchuom"].ToString();
                    //string strQuery = "select vchuomdescription from TABINVUOMMST where vchuomid='" + ManageQuote(npgdr["vchmeasureduomid"].ToString()) + "';";
                    //string vchuomdescription = NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strQuery).ToString();
                    //  objProduct.issueduom = npgdr["vchissueuom"].ToString();
                    objProduct.issueduom = npgdr["vchuom"].ToString();
                    //objProduct.previssueqty = npgdr["previssueqty"].ToString();
                    objProduct.previssueqty = npgdr["previssueqty"].ToString();
                    objProduct.numindentqty = (npgdr["numindentqty"].ToString());
                    objProduct.productid = (npgdr["productid"].ToString());
                    objProduct.productcategoryid = npgdr["productcategoryid"].ToString();
                    objProduct.productsubcategoryid = Convert.ToString(npgdr["productsubcategoryid"].ToString());
                    //  objProduct.numindentconvertionqty = Convert.ToString(npgdr["numindentconvertionqty"].ToString());
                    //  objProduct.shelfid = Convert.ToString(npgdr["shelfid"].ToString());
                    objProduct.shelfname = Convert.ToString(npgdr["shelfname"]);
                    //  objProduct.vchsaleuomid = npgdr["vchmeasureduomid"].ToString();
                    //    objProduct.numsaleuomconvertion = Convert.ToString(npgdr["numsaleuomconvertion"].ToString());
                    objProduct.storagelocationid = Convert.ToString(npgdr["storagelocationid"].ToString());
                    objProduct.storagelocationname = Convert.ToString(npgdr["storagelocation"]);
                    objProduct.DeparmentName = Convert.ToString(npgdr["vchdepartment"]);
                    objProduct.issuedqy = null;
                    objProduct.numavailabilityqty = npgdr["availableqty"].ToString();
                    objlstindentgrid.Add(objProduct);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndentGrid");
            }
            finally
            {
                npgdr.Dispose();
            }
            return objlstindentgrid;
        }

        public List<IndentDTO> StorageLocationBind()
        {
            List<IndentDTO> lststorage = new List<IndentDTO>();
            try
            {
                string strSelect = "select storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1;";
                // string strSelect = "select storagelocationid||'-'||storagelocationcode as storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1;";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect);
                while (npgdr.Read())
                {
                    IndentDTO objindentstorage = new IndentDTO();
                    objindentstorage.storagelocationid = Convert.ToString(npgdr["storagelocationid"]);
                    objindentstorage.storagelocationname = Convert.ToString(npgdr["storagelocationname"]);
                    // objindentstorage.storagelocationcode = Convert.ToString(npgdr["storagelocationcode"]);

                    lststorage.Add(objindentstorage);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndentStorages");
            }
            finally
            {
                npgdr.Dispose();
            }
            return lststorage;

        }

        public int SaveIndentIssueDetails(IndentDTO ID, List<IndentDTO> ID2)
        {
            string strmaterialissue = string.Empty;
            string strmaterialissuedetails = string.Empty;
            string strIndentEdit = string.Empty;
            string strIndentExistOrNot = string.Empty;
            int issueno;
            int str123 = 1;
            try
            {
                con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                trans = con.BeginTransaction();
                IndentDTO objProduct = new IndentDTO();

                //strIndentExistOrNot = "select count(*) from tabmmsmaterialissue where vchindentno='" + ManageQuote(ID.vchindentno) + "';";
                //int Indentcount = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentExistOrNot));
                //if (Indentcount == 0)
                //{

                strIndentEdit = "select substring(vchissueno,4)::int +1 from tabmmsmaterialissue order by recordid desc limit 1;";
                int vchKOTID = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit));
                objProduct.vchissueno = "MIS" + vchKOTID;
                ID.IssueType = "Indent";
                strmaterialissue = @"INSERT INTO tabmmsmaterialissue(vchissueno, vchindentno, datissuedate, vchissuetype,vchrequestedby, vchapprovalby, vchissuedby, statusid,  createdby, createddate,vchdepartment) 
                                               VALUES ('" + ManageQuote(objProduct.vchissueno) + "','" + ManageQuote(ID.vchindentno) + "', current_date, '" + ManageQuote(ID.DirOrIndent) + "', '" + ManageQuote(ID.vchrequestedby) + "', '" + ManageQuote(ID.ApprovalBy) + "',  '" + ManageQuote(ID.IssuedBy) + "', 1, " + ID.userid + ",current_timestamp,'" + ManageQuote(ID.DeparmentName) + "') returning recordid;";
                issueno = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strmaterialissue));
                if (issueno != null)
                {
                    for (int i = 0; i < ID2.Count; i++)
                    {
                        string storagelocationname = string.Empty;
                        //  string strQuery = "select storagelocationname from tabmmsstoragelocationmst where storagelocationid=" + ID2[i].storagelocationname + ";";
                        // storagelocationname = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strQuery));
                        storagelocationname = Convert.ToString(ID2[i].storagelocationname);
                        string strIssueuomQuery = "select vchuomdescription from TABINVUOMMST where vchuomid='" + ManageQuote(ID2[i].vchsaleuomid) + "';";
                        string issueuom = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strIssueuomQuery));

                        //string strnewavailability = "null";
                        if (ID2[i].ConversionValue == null || ID2[i].ConversionValue == string.Empty)
                        {
                            ID2[i].ConversionValue = "0";
                        }
                        if (ID2[i].issuedqy != null && ID2[i].issuedqy != "" && Convert.ToDecimal(ID2[i].issuedqy) != 0)
                        {
                            if (ID2[i].numindentqty == null || ID2[i].numindentqty == "")
                            {
                                ID2[i].numindentqty = "0";
                            }
                            //  numissueqty-numconvertionvalue
                            string issueqty = ID2[i].issuedqy;
                            string conversionval = ID2[i].ConversionValue;
                            decimal iTotal1;
                            iTotal1 = Convert.ToDecimal(issueqty) * Convert.ToDecimal(conversionval);
                            ID2[i].numissueconvertionqty = Convert.ToString(iTotal1);
                            decimal available = Convert.ToDecimal(ID2[i].numavailabilityqty);
                            decimal iavailable = Convert.ToDecimal(available) - Convert.ToDecimal(ID2[i].numissueconvertionqty);
                            ID2[i].numavailabilityqty = Convert.ToString(iavailable);

                            if (ID2[i].productsubcategoryid == null)
                            {
                                ID2[i].productsubcategoryid = "null";
                            }

                            strmaterialissuedetails = @"INSERT INTO tabmmsmaterialissuedetails(issueno, vchissueno, productid, productcode, productcategoryid,categoryname, productsubcategoryid, subcategoryname, vchuom,vchindentuom, numindentqty, vchissueuom, 
                                    numissueqty, numissueconvertionqty, numavailableqty, storagelocation,  shelfname, statusid,  createdby, createddate,productname,numconvertionvalue)
                                    VALUES (" + issueno + ",'" + ManageQuote(objProduct.vchissueno) + "'," + ID2[i].productid + ", '" + ManageQuote(ID2[i].productcode) + "'," + ID2[i].productcategoryid + ", '" + ManageQuote(ID2[i].categoryname) + "'," + ID2[i].productsubcategoryid + ", '" + ManageQuote(ID2[i].subcategoryname) + "','" + ID2[i].uom + "', '" + ID2[i].uom + "', " + ID2[i].numindentqty + ",'"
                                      + issueuom + "'," + ID2[i].issuedqy + ", " + ID2[i].numissueconvertionqty + ", " + ID2[i].numavailabilityqty + ",'" + storagelocationname + "', '" + ID2[i].shelfname + "', 1, " + ID.userid + ",current_timestamp,'" + ManageQuote(ID2[i].productname) + "', " + ID2[i].ConversionValue + ");";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strmaterialissuedetails);

                        }
                        if (Convert.ToDecimal(ID2[i].ConversionValue) > 0)
                        {
                            string strconvsnsave = string.Empty;
                            strconvsnsave = "SELECT * FROM tabmmsproductwiseuomconvertion where vchstandarduom='" + ManageQuote(ID2[i].uom) + "' and  vchconvertionuom='" + ManageQuote(issueuom) + "' and productid='" + ID2[i].productid + "' and productname='" + ManageQuote(ID2[i].productname) + "' and numconvertionqty=" + ID2[i].ConversionValue + " ";
                            if (Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strconvsnsave)) == 0)
                            {
                                string Items = "INSERT INTO tabmmsproductwiseuomconvertion(productid,productcode,productname,vchstandarduom, numstandardqty, vchconvertionuom, numconvertionqty, createdby, createddate) values (" + ID2[i].productid + ",'" + ManageQuote(ID2[i].productcode) + "','" + ManageQuote(ID2[i].productname) + "','" + ManageQuote(ID2[i].uom) + "', 1, '" + ManageQuote(issueuom) + "', " + ID2[i].ConversionValue + "," + ID.userid + ",current_timestamp);";
                                NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                            }
                        }
                    }
                    str123 = 0;
                }
                else
                {
                    str123 = 1;
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                EventLogger.WriteToErrorLog(ex, "IndentEdit");
                throw ex;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                    trans.Dispose();
                }
            }
            return str123;
        }

        public List<IndentDTO> IssueDetailsBind(IndentDTO objindentissue)
        {
            List<IndentDTO> lstindentissue = new List<IndentDTO>();
            try
            {
                //string strSelect = "select storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1;";
                // string strSelect = @"select distinct tmid.categoryname,tmid.subcategoryname,tpm.producttype,tmid.vchuom from tabmmsmaterialissuedetails tmid join tabmmsproductmst tpm on tmid.productid=tpm.productid where tmid.productID=" + objindentissue.productid + " ;";

                //string strSelect = @"select categoryname,subcategoryname,producttype,uomname as vchuom from tabmmsproductmst where productID=" + objindentissue.productid + " and  statusid=1;";

                string strSelect = @"select productcategoryid,categoryname,subcategoryname,producttype,uomname as vchuom,storagelocation,shelfname,productsubcategoryid,storagelocationid  from tabmmsproductmst where productID=" + objindentissue.productid + " and  statusid=1;";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect);
                while (npgdr.Read())
                {
                    IndentDTO objindentstorage = new IndentDTO();
                    objindentstorage.categoryname = npgdr["categoryname"].ToString();
                    objindentstorage.subcategoryname = Convert.ToString(npgdr["subcategoryname"]);
                    objindentstorage.productsubcategoryid = Convert.ToString(npgdr["productsubcategoryid"]);
                    objindentstorage.producttype = npgdr["producttype"].ToString();
                    objindentstorage.uom = Convert.ToString(npgdr["vchuom"]);
                    objindentstorage.storagelocationname = Convert.ToString(npgdr["storagelocation"]);
                    objindentstorage.shelfname = Convert.ToString(npgdr["shelfname"]);
                    objindentstorage.storagelocationid = Convert.ToString(npgdr["storagelocationid"]);

                    string strQuery = "select productname from vwgetstockdetails where productid=" + objindentissue.productid + ";";
                    string productname = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strQuery));

                    objindentstorage.productname = productname;

                    string strProductQuery = "select availableqty from vwgetstockdetails where productid=" + objindentissue.productid + " and productname='" + ManageQuote(objindentstorage.productname) + "';";
                    string availableqty = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strProductQuery));
                    objindentstorage.numavailabilityqty = availableqty;
                    //if (objindentstorage.uom==objindentstorage.issueduom)
                    //{

                    //}
                    objindentstorage.productcategoryid = Convert.ToString(npgdr["productcategoryid"]);
                    lstindentissue.Add(objindentstorage);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndentIssues");
            }
            finally
            {
                npgdr.Dispose();
            }
            return lstindentissue;
        }

        public List<IndentDTO> ShowDirectIndentGridDetails(IndentDTO objproductname)
        {

            List<IndentDTO> objlstDirectindentgrid = new List<IndentDTO>();
            try
            {
                string StrSelect = string.Empty;
                StrSelect = @"select tmid.productcode,tmid.categoryname,tmid.subcategoryname,tpm.producttype,tmid.vchindentuom,tpm.storagelocation,
                tpm.productid,tpm.productcategoryid,tpm.productsubcategoryid,tmid.numindentconvertionqty,tpm.storagelocationid,tpm.storagelocation,tpm.shelfid,tpm.shelfname,
                tmind.vchsaleuomid,tmind.numsaleuomconvertion,tmind.vchmeasureduomid,tmid.numindentqty from tabmmsmaterialindent tmi join tabmmsmaterialindentdetails tmid on tmi.vchindentno=tmid.vchindentno join tabmmsproductmst tpm on tmid.productcode=tpm.productcode
                join tabmmsproductuomconversion tmind
                on tmind.productid=tpm.productid
                where tpm.productid=" + objproductname.productid + ";";//20
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, StrSelect);
                while (npgdr.Read())
                {
                    IndentDTO objProduct = new IndentDTO();

                    objProduct.productcode = npgdr["productcode"].ToString();
                    objProduct.categoryname = Convert.ToString(npgdr["categoryname"]);
                    objProduct.subcategoryname = npgdr["subcategoryname"].ToString();
                    objProduct.producttype = npgdr["producttype"].ToString();
                    objProduct.uom = npgdr["vchindentuom"].ToString();
                    // objProduct.issueduom = string.Empty;

                    string strQuery = "select vchuomdescription from TABINVUOMMST where vchuomid='" + ManageQuote(npgdr["vchmeasureduomid"].ToString()) + "';";
                    string vchuomdescription = NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strQuery).ToString();
                    objProduct.numindentqty = Convert.ToString(npgdr["numindentqty"]);
                    objProduct.issueduom = vchuomdescription;
                    //objProduct.numindentqty = Convert.ToDecimal(npgdr["numindentqty"].ToString());
                    objProduct.productid = (npgdr["productid"].ToString());
                    objProduct.productcategoryid = Convert.ToString(npgdr["productcategoryid"].ToString());
                    objProduct.productsubcategoryid = Convert.ToString(npgdr["productsubcategoryid"].ToString());
                    objProduct.numindentconvertionqty = Convert.ToString(npgdr["numindentconvertionqty"].ToString());
                    objProduct.shelfid = Convert.ToString(npgdr["shelfid"].ToString());
                    objProduct.shelfname = npgdr["shelfname"].ToString();
                    objProduct.vchsaleuomid = npgdr["vchsaleuomid"].ToString();
                    objProduct.numsaleuomconvertion = Convert.ToString(npgdr["numsaleuomconvertion"].ToString());
                    objProduct.storagelocationid = Convert.ToString(npgdr["storagelocationid"].ToString());
                    objProduct.storagelocationname = npgdr["storagelocation"].ToString();
                    objProduct.issuedqy = null;
                    objProduct.numavailabilityqty = null;
                    //21
                    objlstDirectindentgrid.Add(objProduct);
                }

            }
            catch (Exception ex)
            {


            }
            return objlstDirectindentgrid;
        }

        public int SaveDirectIndentIssueDetails(IndentDTO IND, List<IndentDTO> IND2)
        {
            string strmaterialissue = string.Empty;
            string strmaterialissuedetails = string.Empty;
            string strIndentEdit = string.Empty;
            string strIndentExistOrNot = string.Empty;
            int issueno;
            int str123 = 1;
            try
            {

                con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                if (con.State != ConnectionState.Open)
                {
                    con.Open();
                }
                trans = con.BeginTransaction();
                IndentDTO objProduct = new IndentDTO();

                //strIndentExistOrNot = "select count(*) from tabmmsmaterialissue where vchindentno='" + ManageQuote(IND.vchindentno) + "';";
                //int Indentcount = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentExistOrNot));
                //if (Indentcount == 0)
                //{

                strIndentEdit = "select substring(vchissueno,4)::int +1 from tabmmsmaterialissue order by recordid desc limit 1;";
                int vchKOTID = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit));
                objProduct.vchissueno = "MIS" + vchKOTID;
                IND.IssueType = "Direct";
                strmaterialissue = @"INSERT INTO tabmmsmaterialissue(vchissueno, vchindentno, datissuedate, vchissuetype,vchrequestedby, vchapprovalby, vchissuedby, statusid, createdby,createddate,vchdepartment) 
                                               VALUES ('" + ManageQuote(objProduct.vchissueno) + "','" + ManageQuote(IND.vchindentno) + "', current_date, '" + ManageQuote(IND.IssueType) + "', '" + ManageQuote(IND.vchrequestedby) + "', '" + ManageQuote(IND.ApprovalBy) + "',  '" + ManageQuote(IND.IssuedBy) + "', 1, " + IND.userid + ",current_timestamp,'" + ManageQuote(IND.DeparmentName) + "') returning recordid;";
                issueno = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strmaterialissue));
                if (issueno != null)
                {
                    for (int i = 0; i < IND2.Count; i++)
                    {
                        string strQuery = "select storagelocationname from tabmmsstoragelocationmst where storagelocationid=" + IND2[i].storagelocationid + ";";
                        string storagelocationname = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strQuery));


                        if (IND2[i].numindentqty == "")
                        {
                            IND2[i].numindentqty = "null";
                        }
                        if (IND2[i].numindentconvertionqty == "")
                        {
                            IND2[i].numindentconvertionqty = "0.0000";
                        }
                        if (IND2[i].numsaleuomconvertion == "")
                        {
                            IND2[i].numsaleuomconvertion = "0.0000";
                        }
                        if (IND2[i].ConversionValue == null || IND2[i].ConversionValue == string.Empty)
                        {
                            IND2[i].ConversionValue = "0";
                        }
                        if (IND2[i].productsubcategoryid == null)
                        {
                            IND2[i].productsubcategoryid = "null";
                        }
                        string strIssueuomQuery = "select vchuomdescription from TABINVUOMMST where vchuomid='" + ManageQuote(IND2[i].vchsaleuomid) + "';";
                        string issueuom = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strIssueuomQuery));

                        string strCateidQuery = "select productcategoryid from tabmmsproductcategorymst  where productcategoryname='" + ManageQuote(IND2[i].categoryname) + "'";
                        string productcatgoryid = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strCateidQuery));

                        IND2[i].productcategoryid = productcatgoryid;

                        string strSubCateidQuery = "select productsubcategoryid from tabmmsproductsubcategorymst where upper(productsubcategoryname)='" + ManageQuote(IND2[i].subcategoryname) + "'";
                        string productSubcatgoryid = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strSubCateidQuery));

                        IND2[i].productsubcategoryid = productSubcatgoryid;

                        string strnewavailability = IND2[i].numavailabilityqty;
                        if (IND2[i].productsubcategoryid == "")
                        {
                            IND2[i].productsubcategoryid = "NULL";
                        }

                        strmaterialissuedetails = "INSERT INTO tabmmsmaterialissuedetails(issueno, vchissueno, productid, productcode, productcategoryid,categoryname, productsubcategoryid, subcategoryname, vchuom,vchindentuom, numindentqty, numindentconvertionqty, vchissueuom, numissueqty, numissueconvertionqty, numavailableqty, storagelocationid,storagelocation,  shelfname, statusid, createdby, createddate,productname,numconvertionvalue) VALUES (" + issueno + ",'" + ManageQuote(objProduct.vchissueno) + "'," + IND2[i].productid + ", '" + ManageQuote(IND2[i].productcode) + "'," + IND2[i].productcategoryid + ", '" + ManageQuote(IND2[i].categoryname) + "'," + IND2[i].productsubcategoryid + ", '" + ManageQuote(IND2[i].subcategoryname) + "','" + ManageQuote(IND2[i].uom) + "', '" + ManageQuote(IND2[i].uom) + "', " + IND2[i].numindentqty + "," + IND2[i].numindentconvertionqty + ",'" + ManageQuote(issueuom) + "'," + IND2[i].issuedqy + ", " + IND2[i].numsaleuomconvertion + ", " + strnewavailability + "," + IND2[i].storagelocationid + ",'" + ManageQuote(storagelocationname) + "', '" + ManageQuote(IND2[i].shelfname) + "', 1, " + IND.userid + ",current_timestamp,'" + ManageQuote(IND2[i].productname) + "', '" + IND2[i].ConversionValue + "');";
                        // strmaterialissuedetails = "INSERT INTO tabmmsmaterialissuedetails(issueno, vchissueno, productid, productcode, productcategoryid,categoryname, subcategoryname, vchuom,vchindentuom, numindentqty, numindentconvertionqty, vchissueuom, numissueqty, numissueconvertionqty, numavailableqty, storagelocationid,storagelocation, shelfid, shelfname, statusid, createdby, createddate,productname,numconvertionvalue) VALUES (" + issueno + ",'" + ManageQuote(objProduct.vchissueno) + "'," + IND2[i].productid + ", '" + ManageQuote(IND2[i].productcode) + "'," + IND2[i].productcategoryid + ", '" + ManageQuote(IND2[i].categoryname) + "', '" + ManageQuote(IND2[i].subcategoryname) + "','" + IND2[i].uom + "', '" + IND2[i].uom + "', " + IND2[i].numindentqty + "," + IND2[i].numindentconvertionqty + ",'" + issueuom + "'," + IND2[i].issuedqy + ", " + IND2[i].numsaleuomconvertion + ", " + strnewavailability + "," + IND2[i].storagelocationname + ",'" + storagelocationname + "'," + IND2[i].shelfid + ", '" + IND2[i].shelfname + "', 1, " + IND.userid + ",current_timestamp,'" + ManageQuote(IND2[i].productcode) + "', " + IND2[i].ConversionValue + ");";
                        NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strmaterialissuedetails);



                        if (Convert.ToDecimal(IND2[i].ConversionValue) > 0)
                        {
                            string strconvsnsave = string.Empty;
                            strconvsnsave = "SELECT * FROM tabmmsproductwiseuomconvertion where vchstandarduom='" + ManageQuote(IND2[i].uom) + "' and  vchconvertionuom='" + ManageQuote(issueuom) + "' and productid='" + IND2[i].productid + "' and productname='" + ManageQuote(IND2[i].productname) + "' and numconvertionqty=" + IND2[i].ConversionValue + " ";
                            if (Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strconvsnsave)) == 0)
                            {
                                string Items = "INSERT INTO tabmmsproductwiseuomconvertion(productid,productcode,productname,vchstandarduom, numstandardqty, vchconvertionuom, numconvertionqty, createdby, createddate) values (" + IND2[i].productid + ",'" + ManageQuote(IND2[i].productcode) + "','" + ManageQuote(IND2[i].productname) + "','" + ManageQuote(IND2[i].uom) + "', 1, '" + ManageQuote(issueuom) + "', " + IND2[i].ConversionValue + "," + IND.userid + ",current_timestamp);";
                                NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Items);
                            }
                        }


                    }
                    str123 = 0;
                    //7,11,12
                }
                else
                {
                    str123 = 1;
                }
                //}
                //else
                //{
                //    str123 = 2;
                //}
                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                EventLogger.WriteToErrorLog(ex, "IndentEdit");
                throw ex;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                    trans.Dispose();
                }
            }
            return str123;


        }

        public string DirectShelfDetails(IndentDTO objavailablequantity)
        {
            string strAvailble = string.Empty;

            string StrProductSelect = "select productname from tabmmsproductmst where productid=" + objavailablequantity.productid + ";";
            string productname = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, StrProductSelect));
            objavailablequantity.productname = productname;


            string Strstoragelocationid = "select storagelocationname from   tabmmsstoragelocationmst WHERE storagelocationid =" + objavailablequantity.storagelocationid + ";";
            string storagename = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, Strstoragelocationid));
            objavailablequantity.storagelocationname = storagename;

            string strSelect = @"select coalesce(availableqty,0) as availableqty from vwgetstockdetails where productid=" + objavailablequantity.productid + " and productname='" + objavailablequantity.productname + "' and uomname='" + objavailablequantity.vchuom + "' and storagelocation='" + ManageQuote(objavailablequantity.storagelocationname) + "';";
            strAvailble = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect));

            return strAvailble;
        }

        public string BindShelfAvailabilityDetails(IndentDTO objavailablequantity)
        {
            string strAvailble = string.Empty;

            string StrProductSelect = "select productname from tabmmsproductmst where productid=" + objavailablequantity.productid + ";";
            string productname = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, StrProductSelect));
            objavailablequantity.productname = productname;
            if (objavailablequantity.storagelocationid == null)
            {

            }

            else
            {
                string StrshelfidSelect = "select storagelocationname from tabmmsstoragelocationmst  where storagelocationid=" + objavailablequantity.storagelocationid + ";";
                string storagename = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, StrshelfidSelect));
                objavailablequantity.storagelocationname = storagename;
                string strSelect = @"select coalesce(availableqty,0) as availableqty from vwgetstockdetails where productid=" + objavailablequantity.productid + " and productname='" + objavailablequantity.productname + "' and uomname='" + objavailablequantity.vchuom + "' and storagelocation='" + ManageQuote(objavailablequantity.storagelocationname) + "'  and shelfname='" + ManageQuote(objavailablequantity.shelfname) + "' ;";
                strAvailble = Convert.ToString(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect));

            }
            return strAvailble;
        }

        public DataTable StorageShelfDetails(string StorageID)
        {
            DataTable dt = new DataTable();
            try
            {
                if (StorageID != "" && StorageID != null && StorageID != string.Empty)
                {
                    string xccxcxc = "select shelfname,shelfid from tabmmsstoragelocationmst tsa join tabmmsshelfmst tss  on tsa.storagelocationname=tss.storagearea and tsa.storagelocationcode=tss.storagecode where tsa.storagelocationid=" + StorageID + " and tss.statusid=1;";
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "select shelfname,shelfid from tabmmsstoragelocationmst tsa join tabmmsshelfmst tss  on tsa.storagelocationname=tss.storagearea and tsa.storagelocationcode=tss.storagecode where tsa.storagelocationid=" + StorageID + " and tss.statusid=1;").Tables[0];
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "StorageShelfDetails");
            }
            return dt;
        }
        public List<IndentDTO> IssueProductStores(IndentDTO objindentissue)
        {
            List<IndentDTO> lststorage = new List<IndentDTO>();
            try
            {
                //string strSelect = "select storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1;";
                string strSelect = "select storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1 and storagelocationid::text in (select storagelocationid  from tabmmsproductmst  where statusid=1 and  productID=" + objindentissue.productid + ");";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect);
                while (npgdr.Read())
                {
                    IndentDTO objindentstorage = new IndentDTO();
                    objindentstorage.storagelocationid = Convert.ToString(npgdr["storagelocationid"]);
                    objindentstorage.storagelocationname = Convert.ToString(npgdr["storagelocationname"]);
                    lststorage.Add(objindentstorage);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowProductIndentStorages");
            }
            finally
            {
                npgdr.Dispose();
            }
            return lststorage;
        }

        public List<IndentDTO> IssueProductShelfsStores(IndentDTO objindentissue)
        {
            List<IndentDTO> lststorage = new List<IndentDTO>();
            try
            {
                //string strSelect = "select storagelocationid,storagelocationname from tabmmsstoragelocationmst where statusid=1;";
                string strSelect = "select shelfid,shelfname from tabmmsshelfmst where statusid=1 and shelfid::text in (select shelfid from tabmmsproductmst where statusid=1 and productID=" + objindentissue.productid + ");";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strSelect);
                while (npgdr.Read())
                {
                    IndentDTO objindentstorage = new IndentDTO();
                    objindentstorage.shelfid = Convert.ToString(npgdr["shelfid"]);
                    objindentstorage.shelfname = Convert.ToString(npgdr["shelfname"]);
                    lststorage.Add(objindentstorage);
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowProdShelfs");
            }
            finally
            {
                npgdr.Dispose();
            }
            return lststorage;


        }
        public List<IndentDTO> GetConversionvalues()
        {
            List<IndentDTO> lstconvsns = new List<IndentDTO>();
            string strconvsn = string.Empty;
            try
            {
                strconvsn = "select productid,productname,productcode,vchstandarduom,vchconvertionuom,numconvertionqty from tabmmsproductwiseuomconvertion ";
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, strconvsn);
                while (npgdr.Read())
                {
                    IndentDTO objindentconv = new IndentDTO();
                    objindentconv.productid = Convert.ToString(npgdr["productid"]);
                    objindentconv.productcode = Convert.ToString(npgdr["productcode"]);
                    objindentconv.productname = Convert.ToString(npgdr["productname"]);
                    objindentconv.uom = Convert.ToString(npgdr["vchconvertionuom"]);
                    objindentconv.vchsaleuomid = Convert.ToString(npgdr["vchstandarduom"]);
                    objindentconv.ConversionValue = Convert.ToString(npgdr["numconvertionqty"]);
                    lstconvsns.Add(objindentconv);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lstconvsns;
        }
        #endregion


        #region  Purchase Return
        public DataTable getdropdownvalues(string Type, string vendorid)
        {
            DataTable dt = new DataTable();
            try
            {
                string strQuery = string.Empty;
                if (Type == "PURCHASE RETURN")
                {
                    //strQuery = "select distinct  vchpurchaseorderno as transactionname,recordid as transactionvalue from tabmmspurchaseorder where vchpurchaseorderno  in (select vchpurchaseorderno from vwpurchasereturn ) and vchpurchaseorderstatus='Y' and statusid='1' and vendorid=" + vendorid + ";";
                    //strQuery = "select distinct  coalesce(vchpurchaseorderno,vchgrnno) as transactionname,coalesce(vchpurchaseorderno,vchgrnno) as transactionvalue from tabmmsgoodsreceivednote where coalesce(vchpurchaseorderno,vchgrnno)  in  (select distinct vchpurchaseorderno from vwpurchasereturn ) and statusid='1' and vendorid=" + vendorid + ";";
                    //strQuery = "select distinct coalesce(vchpurchaseorderno,vchgrnno) as transactionname,coalesce(vchpurchaseorderno,vchgrnno) as transactionvalue from vwpurchasereturn;";
                    strQuery = "select transactionname,transactionvalue from (select distinct coalesce(vchpurchaseorderno,vchgrnno) as transactionname,coalesce(vchpurchaseorderno,vchgrnno) as transactionvalue,coalesce(replace(vchpurchaseorderno,'PO','')::int,replace(vchgrnno,'GRN','')::int) as d from vwpurchasereturn order by  d)v where transactionname in (select vchgrnno as tranasactionvalue from tabmmsgoodsreceivednote where vendorid=" + vendorid + " union all select vchpurchaseorderno  as tranasactionvalue from tabmmspurchaseorder where vendorid =" + vendorid + ");";
                    //strQuery = "select distinct  vchpurchaseorderno as transactionname,recordid as transactionvalue from tabmmspurchaseorder where vchpurchaseorderno  in (select distinct  vchpurchaseorderno from tabmmsgoodsreceivednote where vchpurchaseorderno is not null) and vchpurchaseorderstatus='Y' and statusid='1' and vendorid=" + vendorid + ";";
                }
                else if (Type == "PURCHASE CANCEL")
                {
                    strQuery = "select distinct  vchpurchaseorderno as transactionname,recordid as transactionvalue from tabmmspurchaseorder where vchpurchaseorderno not in (select distinct  vchpurchaseorderno from tabmmsgoodsreceivednote where vchpurchaseorderno is not null) and vchpurchaseorderstatus='Y' and statusid=1 and  vendorid=" + vendorid + ";";
                }
                if (strQuery.Length > 0)
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, strQuery).Tables[0];
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getdropdownvalues");
            }
            return dt;
        }
        public DataTable getpurchasereturnGridValues(string strtype, string ID)
        {
            DataTable dt = new DataTable();
            try
            {
                string strQuery = string.Empty;
                if (strtype == "PURCHASE RETURN")
                {
                    if (ID.Contains("PO"))
                    {
                        strQuery = "select distinct vchgrnno as transactionname,vchgrnno as transactionvalue from vwpurchasereturn  where vchpurchaseorderno='" + ID + "' order by 1;";
                    }
                    else
                    {
                        strQuery = "select * from vwpurchasereturn  where vchgrnno='" + ID + "' order by vchgrnno,productid;";
                    }
                }
                else
                {
                    strQuery = @"SELECT  distinct productid, productcode, productname, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchuom as orderuom, numorderedqty  as orderedqty FROM tabmmspurchaseorderdetails where vchpurchaseorderno='" + ID + "';";
                }
                if (strQuery.Length > 0)
                {
                    dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, strQuery).Tables[0];
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "getpurchasereturnGridValues");
            }
            return dt;
        }
        public bool SavePurchaseReturnCancel(GoodsReceivedNoteDTO PURCHASE, List<GoodsReceivedNoteDTO> lstPURCHASE)
        {
            bool IsSave = false;
            int Count = 0;
            con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            trans = con.BeginTransaction();
            try
            {
                string StrCode = string.Empty;
                string StrID = string.Empty;
                string strInsert = string.Empty;
                decimal conversionQty = 0;
                if (PURCHASE.Type == "PURCHASE RETURN")
                {
                    StrCode = GenerateNextID("tabmmspurchasereturn", "vchpurchasereturnno", "PURCHASE RETURN");
                    strInsert = "INSERT INTO tabmmspurchasereturn(vchpurchasereturnno, datpurchasereturndate, vendorid, vchvendorname, vchpurchaseorderno, vchreason, statusid,createddate, createdby) values ('" + StrCode + "','" + FormatDate(PURCHASE.Date) + "'," + PURCHASE.vendorid + ",'" + PURCHASE.vendorname + "','" + PURCHASE.transactionname + "','" + PURCHASE.Reason + "','1',current_timestamp,'" + PURCHASE.CreatedBy + "') returning recordid;";
                    StrID = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));
                    Count = lstPURCHASE.Count;
                    for (int i = 0; i < Count; i++)
                    {
                        conversionQty = 0;
                        if (Convert.ToDecimal(lstPURCHASE[i].returnqty) > 0)
                        {
                            if (Convert.ToString(lstPURCHASE[i].shelfid) == "" || Convert.ToString(lstPURCHASE[i].shelfid) == string.Empty || lstPURCHASE[i].shelfid == null)
                            {
                                lstPURCHASE[i].shelfname = "";
                                lstPURCHASE[i].shelfid = "null";
                            }
                            if (Convert.ToString(lstPURCHASE[i].shelfname) == "" || Convert.ToString(lstPURCHASE[i].shelfname) == string.Empty || lstPURCHASE[i].shelfname == null || lstPURCHASE[i].shelfname == "SELECT")
                            {
                                lstPURCHASE[i].shelfname = "";
                                lstPURCHASE[i].shelfid = "null";
                            }
                            ///////////////////////////////
                            if (Convert.ToString(lstPURCHASE[i].productsubcategoryid) == "" || Convert.ToString(lstPURCHASE[i].productsubcategoryid) == string.Empty || lstPURCHASE[i].productsubcategoryid == null)
                            {
                                lstPURCHASE[i].productsubcategoryid = "null";
                                lstPURCHASE[i].subcategoryname = "";
                            }
                            if (Convert.ToString(lstPURCHASE[i].subcategoryname) == "" || Convert.ToString(lstPURCHASE[i].subcategoryname) == string.Empty || lstPURCHASE[i].subcategoryname == null || lstPURCHASE[i].subcategoryname == "SELECT")
                            {
                                lstPURCHASE[i].subcategoryname = "";
                                lstPURCHASE[i].productsubcategoryid = "null";
                            }
                            conversionQty = Math.Round(Convert.ToDecimal(lstPURCHASE[i].returnqty) * (Convert.ToDecimal(lstPURCHASE[i].uomconversionvalue)), 4);
                            strInsert = "INSERT INTO tabmmspurchasereturndetails(returnid, vchpurchasereturnno, productid, productcode, productname, productcategoryid, categoryname, productsubcategoryid, subcategoryname, vchuom, storagelocationid, storagelocation, shelfid, shelfname, numorderedqty, numreturnquantity, numavailableqty, statusid, createdby, createddate,receiveduom, uomconvesrionvalue, rate, numreturnconversionquantity) values (" + StrID + ", '" + StrCode + "', " + lstPURCHASE[i].productid + ", '" + lstPURCHASE[i].productcode + "', '" + lstPURCHASE[i].productname + "', " + lstPURCHASE[i].productcategoryid + ", '" + lstPURCHASE[i].categoryname + "', " + lstPURCHASE[i].productsubcategoryid + ", '" + lstPURCHASE[i].subcategoryname + "', '" + lstPURCHASE[i].orderuom + "', " + lstPURCHASE[i].storagelocationid + ", '" + lstPURCHASE[i].storagelocation + "', " + lstPURCHASE[i].shelfid + ", '" + lstPURCHASE[i].shelfname + "', " + lstPURCHASE[i].receivedqty + ", " + lstPURCHASE[i].returnqty + ", " + lstPURCHASE[i].availableqty + ", '1', '" + PURCHASE.CreatedBy + "', current_timestamp,'" + lstPURCHASE[i].receiveduom + "', " + lstPURCHASE[i].uomconversionvalue + ", " + lstPURCHASE[i].grnrate + ", " + conversionQty + ");";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strInsert);
                        }
                    }
                    IsSave = true;
                }
                else if (PURCHASE.Type == "PURCHASE CANCEL")
                {
                    //Count = lstPURCHASE.Count;
                    int rowcnt = lstPURCHASE.Count;
                    for (int i = 0; i < rowcnt; i++)
                    {
                        //select * from tabmmspurchaseorderdetails;
                        //select * from tabmmspurchaseorder;
                        Count = Convert.ToInt16(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, "select count(*) from tabmmspurchaseorder where recordid=" + PURCHASE.transactionvalue + " and vendorid=" + PURCHASE.vendorid + ";"));
                        if (Count > 0)
                        {
                            strInsert = "update tabmmspurchaseorder set vchpurchaseorderstatus='C',statusid='2',modifiedby='" + PURCHASE.CreatedBy + "',modifieddate=current_timestamp where recordid=" + PURCHASE.transactionvalue + " and vendorid=" + PURCHASE.vendorid + ";";
                            if (NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strInsert) == 1)
                            {
                                strInsert = "update tabmmspurchaseorderdetails set statusid='2',modifiedby='" + PURCHASE.CreatedBy + "',modifieddate=current_timestamp  where orderid=" + PURCHASE.transactionvalue + ";";
                                NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strInsert);
                            }
                        }
                    }
                    IsSave = true;
                }
                if (IsSave)
                {
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                EventLogger.WriteToErrorLog(ex, "SavePurchaseReturnCancel");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                }
            }
            return IsSave;
        }
        #endregion

        #region vendorpayment

        public DataTable GetVendorNames()
        {
            DataTable dt = new DataTable();
            string strData = string.Empty;
            try
            {

                strData = "select distinct vendorid,vchvendorname from vwmmsvendorpaymentgriddetails where vchvendorname is not null;";
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, strData).Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "VendorPayment");
            }

            return dt;
        }


        public DataTable GetBankNames()
        {
            DataTable dt = new DataTable();
            string strData = string.Empty;
            try
            {

                strData = "select recordid,vchbankname from tabbanksetup ;";
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, strData).Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "VendorPayment");
            }

            return dt;
        }



        public DataTable GetCheques(int id)
        {
            DataTable dt = new DataTable();
            string strData = string.Empty;
            try
            {

                strData = "select recordid,chequenumber from tabcheques where bankid=" + id + "  and statusid=12;";
                dt = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, strData).Tables[0];
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "VendorPayment");
            }

            return dt;
        }


        public List<VendorPaymentDTO> ShowVendorPaymentDetails(string VendorName)
        {

            List<VendorPaymentDTO> lstVendorPayment = new List<VendorPaymentDTO>();
            DataTable dt = new DataTable();
            string Query = string.Empty;

            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    // Query = "select vchpurchaseorderno,datpurchaseorderdate,totalamount,dueamount from vwmmsvendorpaymentgriddetails where vchvendorname='Kapil Agro';";



                    Query = "select vchgrnno,vchpurchaseorderno, to_char(datgrndate, 'YYYY-MM-DD') AS datgrndate,coalesce(totalamount, 0) as totalamount,coalesce(dueamount,0) as dueamount,vchinvoiceno from vwmmsvendorpaymentgriddetails where vchvendorname='" + ManageQuote(VendorName) + "' and dueamount>0 order by vchgrnno;";

                    using (NpgsqlDataReader npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.Text, Query))
                    {

                        if (npgdr != null)
                        {

                            while (npgdr.Read())
                            {
                                VendorPaymentDTO VendorPaymentDTO = new VendorPaymentDTO();
                                VendorPaymentDTO.PurchaseOrderNo = npgdr["vchpurchaseorderno"].ToString();
                                VendorPaymentDTO.GrnNo = npgdr["vchgrnno"].ToString();

                                VendorPaymentDTO.PurchaseOrderDate = npgdr["datgrndate"].ToString();





                                VendorPaymentDTO.Amount = Convert.ToDouble(npgdr["totalamount"]);









                                VendorPaymentDTO.DueAmount = Convert.ToDouble(npgdr["dueamount"]);

                                VendorPaymentDTO.InvoiceNo = npgdr["vchinvoiceno"].ToString();
                                // VendorPaymentDTO.PaidAmount = 100;

                                lstVendorPayment.Add(VendorPaymentDTO);

                            }


                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }

            return lstVendorPayment;
        }


        public string GenerateNextID(string strtablename, string strcolname, int prefix, string strdate, string strColumnDate, string strPrefix)
        {

            try
            {
                npgdr = NPGSqlHelper.ExecuteReader(NPGSqlHelper.SQLConnString, CommandType.StoredProcedure, "GENERATENEXTID('" + strtablename + "','" + strcolname + "','" + prefix + "','" + strdate + "','" + strColumnDate + "','" + strPrefix + "')", null);
                while (npgdr.Read())
                {
                    GNextID = npgdr[0].ToString();
                }
                npgdr.Close();


            }
            catch (Exception ex)
            {
                string excp = ex.Message.ToString();
                return null;
            }
            return GNextID;
        }


        public bool SaveVPDetails(VendorPaymentDTO lstTotalDetails, List<VendorPaymentDTO> lstInvDetails)
        {

            bool isSaved = false;
            try
            {
                string strInsert = string.Empty;
                string strInsertDetails = string.Empty;
                string strACCUpdates = string.Empty;
                string strNextID = string.Empty;

                string strMVInsert = string.Empty;
                int chqbookid = 0;

                string strBankId = string.Empty;
                string strBankAccId = string.Empty;
                string strBankRecordId = string.Empty;
                double numtotalamount = 0;

                string strVenRecordId = string.Empty;
                string strVenAccId = string.Empty;
                string strVenParAccName = string.Empty;
                string VENDORNAME = string.Empty;

                string BANKNAME = string.Empty;

                string vendorcode = string.Empty;


                long RecordID;
                if (lstInvDetails.Count > 0)
                {
                    con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    strNextID = GenerateNextID("tabmmsvendorpayment", "vchtransactionno", 2, FormatDate(lstTotalDetails.VPDate), "dattransdate", "VV");
                    strNextID = "VV" + strNextID;
                    trans = con.BeginTransaction();





                    string strgettableid = "select vchvendorcode,vchvendorname from tabmmsvendormst   where vendorid='" + lstTotalDetails.VendorName + "'";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(strgettableid, con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {


                            VENDORNAME = npgdr["vchvendorname"].ToString();
                            vendorcode = npgdr["vchvendorcode"].ToString();

                        }
                    }

                    if (lstTotalDetails.PaymentMode == "Cheque")
                    {

                        string getcheckbookid = "select numchqbookid from tabcheques where chequenumber=" + lstTotalDetails.ChequeNO + ";";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(getcheckbookid, con))
                        {
                            npgdr = cmd.ExecuteReader();
                            while (npgdr.Read())
                            {


                                chqbookid = Convert.ToInt32(npgdr["numchqbookid"].ToString());


                            }
                        }


                        string strbanknameid = "select vchbankname from tabbanksetup where recordid=" + lstTotalDetails.BankName + ";";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(strbanknameid, con))
                        {
                            npgdr = cmd.ExecuteReader();
                            while (npgdr.Read())
                            {


                                BANKNAME = npgdr["vchbankname"].ToString();


                            }
                        }

                    }
                    for (int i = 0; i < lstInvDetails.Count; i++)
                    {
                        if (lstInvDetails[i].PaidAmount > 0)
                        {
                            numtotalamount += lstInvDetails[i].PaidAmount;
                        }
                    }

                    if (lstTotalDetails.PaymentMode == "Cheque")
                    {

                        strInsert = "INSERT INTO tabmmsvendorpayment(vchtransactionno,dattransdate,VCHVENDORNAME,VCHVENDORID,vchmodeofpayment,vchchequeno,vchbank,datchequedate,numpaidamount,statusid,createdby,createddate)VALUES('" + strNextID + "','" + FormatDate(lstTotalDetails.VPDate) + "','" + VENDORNAME + "','" + vendorcode + "','" + ManageQuote(lstTotalDetails.PaymentMode) + "','" + ManageQuote(lstTotalDetails.ChequeNO) + "','" + BANKNAME + "','" + FormatDate(lstTotalDetails.ChqDate) + "'," + numtotalamount + ",1," + lstTotalDetails.createdby + ",CURRENT_TIMESTAMP) RETURNING RECORDID;";

                        RecordID = Convert.ToInt64(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));


                    }
                    else
                    {
                        strInsert = "INSERT INTO tabmmsvendorpayment(vchtransactionno,dattransdate,VCHVENDORNAME,VCHVENDORID,vchmodeofpayment,numpaidamount,statusid,createdby,createddate)VALUES('" + strNextID + "','" + FormatDate(lstTotalDetails.VPDate) + "','" + VENDORNAME + "','" + vendorcode + "','" + ManageQuote(lstTotalDetails.PaymentMode) + "'," + numtotalamount + ",1," + lstTotalDetails.createdby + ",CURRENT_TIMESTAMP) RETURNING RECORDID;";
                        RecordID = Convert.ToInt64(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));
                    }

                    for (int i = 0; i < lstInvDetails.Count; i++)
                    {
                        if (lstInvDetails[i].PaidAmount > 0)
                        {

                            strInsertDetails = "insert into tabmmsvendorpaymentdetails(detailsid,vchtransactionno,vchpono,numpoamount,numpaidamount,statusid,createdby,createddate,vchinvoiceno) values('" + RecordID + "','" + strNextID + "','" + ManageQuote(lstInvDetails[i].PurchaseOrderNo) + "'," + lstInvDetails[i].Amount + "," + lstInvDetails[i].PaidAmount + ",1," + lstTotalDetails.createdby + ",CURRENT_TIMESTAMP,'" + ManageQuote(lstInvDetails[i].InvoiceNo) + "');";


                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strInsertDetails);



                            //string mystring = lstInvDetails[i].PurchaseOrderNo;


                            //char firstletter = mystring.FirstOrDefault();


                            //if (firstletter == 'G')
                            //{

                            string Updategoodsreceivednote = "update tabmmsgoodsreceivednote set numdueamount=numdueamount-" + lstInvDetails[i].PaidAmount + " where vchgrnno='" + lstInvDetails[i].GrnNo + "';";
                            NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Updategoodsreceivednote);
                            //}

                            //if (firstletter == 'P')
                            //{

                            //    string Updatepurchaseorder = "update tabmmspurchaseorder set numdueamount=numdueamount-" + lstInvDetails[i].PaidAmount + " where vchpurchaseorderno='" + lstInvDetails[i].PurchaseOrderNo + "';";
                            //    NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, Updatepurchaseorder);
                            //}




                            // numRAamount += lstInvDetails[i].DueAmount;

                            lstTotalDetails.vpPaidAmount += lstInvDetails[i].PaidAmount;


                        }
                    }

                    DataSet ds = new DataSet();
                    ds = bankBankDetails(BANKNAME);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        strBankRecordId = ds.Tables[0].Rows[0]["RECORDID"].ToString();
                        strBankId = ds.Tables[0].Rows[0]["BANKID"].ToString();
                        strBankAccId = ds.Tables[0].Rows[0]["VCHACCOUNTID"].ToString();
                    }
                    else
                    {
                        strBankId = "1";
                        strBankAccId = "acc9";
                    }
                    lstTotalDetails.Narration = "Being The Amount Paid" + strNextID + "";
                    strMVInsert = "INSERT INTO TABMAINCASHVOUCHER(VCHVOUCHERNO,DATDATE,VCHNARRATION,CREDITACCOUNTID,VCHCREDITACCID,NUMAMOUNT,STATUSID,CREATEDDATE,CREATEDBY)VALUES('" + strNextID + "','" + FormatDate(lstTotalDetails.VPDate) + "','" + ManageQuote(lstTotalDetails.Narration) + "','" + strBankId + "','" + strBankAccId + "'," + lstTotalDetails.vpPaidAmount + ",1,CURRENT_TIMESTAMP,1) RETURNING RECORDID";

                    long MVRecordID = Convert.ToInt64(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strMVInsert));

                    DataSet dsAccDetails = new DataSet();
                    dsAccDetails = bindVendorAccDetails(VENDORNAME);
                    if (dsAccDetails.Tables[0].Rows.Count > 0)
                    {
                        strVenRecordId = dsAccDetails.Tables[0].Rows[0]["RECORDID"].ToString();
                        strVenAccId = dsAccDetails.Tables[0].Rows[0]["VCHACCOUNTID"].ToString();
                        strVenParAccName = dsAccDetails.Tables[0].Rows[0]["VENPARENTNAME"].ToString();

                    }
                    if (lstTotalDetails.vpPaidAmount != 0)
                    {
                        strMVInsert = "INSERT INTO TABMAINCASHDETAILS(DETAILSID,VCHVOUCHERNO,ACCOUNTID,VCHACCOUNTID,NUMAMOUNT)VALUES(" + MVRecordID + ",'" + strNextID + "','" + strVenRecordId + "','" + strVenAccId + "'," + lstTotalDetails.vpPaidAmount + ")";
                        NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strMVInsert);

                    }


                    if (lstTotalDetails.PaymentMode == "Chash" || lstTotalDetails.PaymentMode == "Draft")
                    {

                    }
                    if (lstTotalDetails.PaymentMode == "Cheque")
                    {

                        strMVInsert = "INSERT INTO TABCHEQUEDRAFTPAID(VOUCHERNO,VCHVOUCHERNO,BANKID,VCHBANKID,VCHBANKNAME,CHRTYPE,VCHCHEQUENUMBER,VCHDETAILS,CHRCLEARSTATUS,CHRREPRESENTSTATUS,DATISSUEDATE,NUMCHQBOOKID,STATUSID,CREATEDBY,CREATEDDATE)VALUES(" + MVRecordID + ",'" + strNextID + "'," + strBankRecordId + ",'" + strBankAccId + "','" + BANKNAME + "','C','" + lstTotalDetails.ChequeNO + "','" + strVenParAccName + "','N','N','" + FormatDate(lstTotalDetails.ChqDate.ToString()) + "'," + chqbookid + ",1," + lstTotalDetails.createdby + ",CURRENT_TIMESTAMP)";
                        NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strMVInsert);



                        strMVInsert = "UPDATE TABCHEQUES SET STATUSID=13,MODIFIEDBY='" + lstTotalDetails.createdby + "',MODIFIEDDATE=CURRENT_TIMESTAMP WHERE BANKID='" + strBankRecordId + "' AND VCHBANKID='" + strBankAccId + "' AND CHEQUENUMBER='" + lstTotalDetails.ChequeNO + "' AND NUMCHQBOOKID=" + chqbookid + "";
                        NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strMVInsert);

                    }



                    strMVInsert = "SELECT FNTOTALTRANSACTIONS('" + strNextID + "','PAYMENT');";
                    NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strMVInsert);


                    strACCUpdates = "UPDATE TABACCOUNTTREE SET NUMACCOUNTBALANCE=(NUMACCOUNTBALANCE)-" + lstTotalDetails.vpPaidAmount + " WHERE VCHACCOUNTID='" + strBankAccId + "'; ";
                    NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strACCUpdates);


                    strACCUpdates = "UPDATE TABACCOUNTTREE SET NUMACCOUNTBALANCE=(NUMACCOUNTBALANCE)+" + lstTotalDetails.vpPaidAmount + " WHERE VCHACCOUNTID='" + strVenAccId + "';";
                    NPGSqlHelper.ExecuteNonQuery(trans, CommandType.Text, strACCUpdates);









                }
                trans.Commit();
                isSaved = true;




            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

            }
            return isSaved;
        }


        public DataSet bankBankDetails(string strBankName)
        {
            try
            {
                return NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "SELECT RECORDID,VCHBANKNAME,BANKID,VCHACCOUNTID FROM TABBANKSETUP WHERE VCHBANKNAME='" + strBankName + "'");
            }
            catch
            {
                return null;
            }
        }





        public DataSet bindVendorAccDetails(string strVendorName)
        {
            try
            {
                string strCreateHead = string.Empty;

                DataSet dsDetails = new DataSet();
                //dsDetails = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "SELECT RECORDID,VCHACCOUNTID,VCHPARENTACCOUNTNAME||'('||VCHACCOUNTNAME||')' AS VENPARENTNAME FROM TABACCOUNTTREE WHERE VCHPARENTID IN(SELECT VCHACCOUNTID FROM TABACCOUNTTREE WHERE VCHACCOUNTNAME LIKE '%CREDITORS -%' AND CHRACCTYPE='2') AND VCHACCOUNTNAME LIKE '" + ManageQuote(strVendorName) + "' LIMIT 1;");
                dsDetails = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "SELECT RECORDID,VCHACCOUNTID,VCHPARENTACCOUNTNAME||'('||VCHACCOUNTNAME||')' AS VENPARENTNAME FROM TABACCOUNTTREE WHERE VCHPARENTID IN(SELECT VCHACCOUNTID FROM TABACCOUNTTREE WHERE VCHACCOUNTNAME LIKE 'SUNDRY PARTY ACCOUNTS' AND CHRACCTYPE='2') AND VCHACCOUNTNAME LIKE '" + ManageQuote(strVendorName) + "' LIMIT 1;");
                if (dsDetails.Tables[0].Rows.Count > 0)
                {
                    return dsDetails;
                }
                else
                {
                    strCreateHead = "SELECT INSERTACCOUNTHEADS('" + strVendorName + "' ,'acc43','3');";
                    string AccountHead = NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strCreateHead).ToString();
                    dsDetails = NPGSqlHelper.ExecuteDataset(NPGSqlHelper.SQLConnString, CommandType.Text, "SELECT RECORDID,VCHACCOUNTID,VCHPARENTACCOUNTNAME||'('||VCHACCOUNTNAME||')' AS VENPARENTNAME FROM TABACCOUNTTREE WHERE VCHACCOUNTID='" + AccountHead + "'");

                }

                return dsDetails;
            }
            catch
            {
                return null;
            }
        }




        #endregion

        #region material Return
        public List<MaterialReturnDTO> ShowIssuenumbers()
        {

            List<MaterialReturnDTO> lstnumbers = new List<MaterialReturnDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("select recordid,vchissueno from tabmmsmaterialissue order by recordid", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            MaterialReturnDTO objUser = new MaterialReturnDTO();
                            objUser.recordid = Convert.ToInt32(npgdr["recordid"]);
                            objUser.Number = npgdr["vchissueno"].ToString();

                            lstnumbers.Add(objUser);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "showNumber");
                throw ex;

            }
            finally
            {
                npgdr.Dispose();
            }
            return lstnumbers;

        }
        public List<MaterialReturnDTO> ShowIndentNos()
        {

            List<MaterialReturnDTO> lstnumbers = new List<MaterialReturnDTO>();
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("select distinct mn.recordid,mn.vchindentno from tabmmsmaterialindent mn join tabmmsmaterialissue mi on mn.vchindentno=mi.vchindentno order by mn.recordid;", con))
                    {
                        npgdr = cmd.ExecuteReader();
                        while (npgdr.Read())
                        {
                            MaterialReturnDTO objUser = new MaterialReturnDTO();
                            objUser.recordid = Convert.ToInt32(npgdr["recordid"]);
                            objUser.Number = npgdr["vchindentno"].ToString();

                            lstnumbers.Add(objUser);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowIndentnumbers");
                throw ex;

            }
            finally
            {
                npgdr.Dispose();
            }
            return lstnumbers;

        }


        public List<MaterialReturnDTO> ShowMaterialReturn(string Number, string Returntype)
        {
            List<MaterialReturnDTO> lstMaterialReturn = new List<MaterialReturnDTO>();
            try
            {

                using (NpgsqlConnection con = new NpgsqlConnection(NPGSqlHelper.SQLConnString))
                {

                    if (Returntype == "MaterialIndent")
                    {
                        con.Open();
                        //using (NpgsqlCommand cmd = new NpgsqlCommand(" select productid,productname,categoryname,subcategoryname,uomname,availableqty,storagelocation,shelfname from vwgetstockdetails where storagelocationid='" + locationid + "' order by productname;", con))
                        using (NpgsqlCommand cmd = new NpgsqlCommand("select vchindentno,productid,productname,vchuom,storagelocationname,storagelocationcode,numindentqty,shelfname,numindentconvertionqty from tabmmsmaterialindentdetails where vchindentno='" + Number + "'; ", con))
                        {
                            npgdr = cmd.ExecuteReader();
                            while (npgdr.Read())
                            {
                                MaterialReturnDTO objMaterialReturn = new MaterialReturnDTO();

                                objMaterialReturn.productname = npgdr["productname"].ToString();
                                objMaterialReturn.productid = Convert.ToInt32(npgdr["productid"].ToString());
                                objMaterialReturn.uom = npgdr["vchuom"].ToString();
                                objMaterialReturn.Storagelocation = npgdr["storagelocationname"].ToString();
                                objMaterialReturn.Shelfname = npgdr["shelfname"].ToString();
                                objMaterialReturn.Storagelocationcode = npgdr["storagelocationcode"].ToString();
                                objMaterialReturn.qty = Convert.ToDecimal(npgdr["numindentqty"].ToString());
                                objMaterialReturn.convertionvalue = npgdr["numindentconvertionqty"].ToString();

                                lstMaterialReturn.Add(objMaterialReturn);
                            }
                        }
                    }
                    else
                    {
                        con.Open();
                        //using (NpgsqlCommand cmd = new NpgsqlCommand(" select productid,productname,categoryname,subcategoryname,uomname,availableqty,storagelocation,shelfname from vwgetstockdetails where storagelocationid='" + locationid + "' order by productname;", con))
                        using (NpgsqlCommand cmd = new NpgsqlCommand("select vchissueno,productid,productname,vchuom,storagelocation,storagelocationid,shelfname,shelfid,numissueqty,numconvertionvalue from tabmmsmaterialissuedetails where vchissueno='" + Number + "'; ", con))
                        {
                            npgdr = cmd.ExecuteReader();
                            while (npgdr.Read())
                            {
                                MaterialReturnDTO objMaterialReturn = new MaterialReturnDTO();

                                objMaterialReturn.productname = npgdr["productname"].ToString();
                                objMaterialReturn.productid = Convert.ToInt32(npgdr["productid"].ToString());
                                objMaterialReturn.uom = npgdr["vchuom"].ToString();
                                objMaterialReturn.Storagelocation = npgdr["storagelocation"].ToString();
                                objMaterialReturn.Shelfname = npgdr["shelfname"].ToString();
                                objMaterialReturn.Storagelocationid = Convert.ToInt32(npgdr["storagelocationid"].ToString());
                                objMaterialReturn.qty = Convert.ToDecimal(npgdr["numissueqty"].ToString());
                                objMaterialReturn.convertionvalue = npgdr["numconvertionvalue"].ToString();

                                lstMaterialReturn.Add(objMaterialReturn);
                            }
                        }
                    }

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                EventLogger.WriteToErrorLog(ex, "ShowMaterialReturn");
                throw ex;

            }
            finally
            {
                npgdr.Dispose();
            }
            return lstMaterialReturn;

        }

        public bool SaveMaterialReturn(MaterialReturnDTO MaterialReturnDTO, List<MaterialReturnDTO> lstMaterialReturn)
        {
            bool IsSave = false;
            con = new NpgsqlConnection(NPGSqlHelper.SQLConnString);
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            trans = con.BeginTransaction();
            try
            {
                string StrCode = string.Empty;
                string StrID = string.Empty;
                string strInsert = string.Empty;

                string strIndentEdit = "select substring(vchreturnno,3)::int +1 from tabmmsmaterialreturn order by recordid desc limit 1;";
                int vchKOTID3 = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(NPGSqlHelper.SQLConnString, CommandType.Text, strIndentEdit));
                MaterialReturnDTO.issuereturnno = "MR" + vchKOTID3;
                if (MaterialReturnDTO.returntype == "Direct")
                {
                    MaterialReturnDTO.issueno = "Direct";
                    for (int i = 0; i < lstMaterialReturn.Count; i++)
                    {
                        string insertissuertn = " INSERT INTO tabmmsmaterialreturn( vchreturnno, datreturndate, vchreturnby, vchreceivedby,statusid, createdby, createddate) VALUES ('" + MaterialReturnDTO.issuereturnno + "','" + MaterialReturnDTO.Date + "','" + MaterialReturnDTO.Returndby + "','" + MaterialReturnDTO.Recivedby + "',1,1,current_date) returning recordid;";
                        int isureturn = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, insertissuertn));
                        strInsert = "INSERT INTO tabmmsmaterialreturndetails( returnno, vchreturnno,vchissueno, productid, productname,vchuom, vchreturnuom, vchreturnqty, vchreturnconvertionqty,storagelocation,shelfname)VALUES (" + isureturn + ",'" + MaterialReturnDTO.issuereturnno + "','" + MaterialReturnDTO.issueno + "','" + lstMaterialReturn[i].productid + "','" + lstMaterialReturn[i].productname + "','" + lstMaterialReturn[i].uom + "','" + lstMaterialReturn[i].uom + "','" + lstMaterialReturn[i].Returnqty + "','" + lstMaterialReturn[i].convertionvalue + "','" + lstMaterialReturn[i].Storagelocation + "','" + lstMaterialReturn[i].Shelfname + "') returning recordid;";
                        StrID = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));

                    }
                }
                else
                {
                    for (int i = 0; i < lstMaterialReturn.Count; i++)
                    {
                        string insertissuertn = " INSERT INTO tabmmsmaterialreturn( vchreturnno, datreturndate, vchreturnby, vchreceivedby,statusid, createdby, createddate) VALUES ('" + MaterialReturnDTO.issuereturnno + "','" + MaterialReturnDTO.Date + "','" + MaterialReturnDTO.Returndby + "','" + MaterialReturnDTO.Recivedby + "',1,1,current_date) returning recordid;";
                        int isureturn = Convert.ToInt32(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, insertissuertn));
                        strInsert = "INSERT INTO tabmmsmaterialreturndetails( returnno, vchreturnno,vchissueno, productid, productname,vchuom, vchreturnuom, vchreturnqty, vchreturnconvertionqty,storagelocation,shelfname)VALUES (" + isureturn + ",'" + MaterialReturnDTO.issuereturnno + "','" + MaterialReturnDTO.issueno + "','" + lstMaterialReturn[i].productid + "','" + lstMaterialReturn[i].productname + "','" + lstMaterialReturn[i].uom + "','" + lstMaterialReturn[i].uom + "','" + lstMaterialReturn[i].Returnqty + "','" + lstMaterialReturn[i].convertionvalue + "','" + lstMaterialReturn[i].Storagelocation + "','" + lstMaterialReturn[i].Shelfname + "') returning recordid;";
                        StrID = Convert.ToString(NPGSqlHelper.ExecuteScalar(trans, CommandType.Text, strInsert));

                    }
                }


                if (StrID != "")
                {
                    IsSave = true;
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                EventLogger.WriteToErrorLog(ex, "SaveMaterial Return");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                    con.ClearPool();
                }
            }
            return IsSave;
        }

        #endregion

        //public bool SaveMaterialReturn(MaterialReturnDTO MaterialReturnDTO, List<MaterialReturnDTO> lstMaterialReturn)
        //{
        //    throw new NotImplementedException();
        //} 

        //List<MaterialIndentDTO> IMMSTransaction.GetProductAvailabilityQty(string productid, string productname, string uomname, string storagearea, string shelfname)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
















