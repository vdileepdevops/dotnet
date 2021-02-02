﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMS.Core
{
    public class MMSTransaction
    {
    }

    #region GoodsReceivedNoteDTO
    public class GoodsReceivedNoteDTO
    {
        public int CreatedBy { get; set; }
        //GRID DETAILS
        public int productid { get; set; }
        public string productcode { get; set; }
        public string productname { get; set; }
        public string productcategoryid { get; set; }
        public string categoryname { get; set; }
        public string productsubcategoryid { get; set; }
        public string subcategoryname { get; set; }
        public string orderuom { get; set; }
        public string receiveduom { get; set; }
        public decimal orderedqty { get; set; }
        public string receivedqty { get; set; }
        public string returnqty { get; set; }
        public string porate { get; set; }
        public string grnrate { get; set; }
        public string storagelocationid { get; set; }
        public string storagelocation { get; set; }
        public string shelfid { get; set; }
        public string shelfname { get; set; }
        public string grnuom { get; set; }
        //Main DETAILS
        public string grntype { get; set; }
        public string grndate { get; set; }
        public string vendorname { get; set; }
        public string vendorid { get; set; }
        public string poid { get; set; }
        public string pono { get; set; }
        public string invoiceno { get; set; }
        public string invoicedate { get; set; }
        public string requestedid { get; set; }
        public string requestedperson { get; set; }
        public string TermsandConditions { get; set; }
        public string uomconversionvalue { get; set; }
        //PURCHASE RETURN  AND PURCHASE CANCEL
        public string Reason { get; set; }
        public string availableqty { get; set; }
        public string transactionvalue { get; set; }
        public string transactionname { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string uom { get; set; }
    }
    public class GoodsReceivedNoteTAXDTO
    {
        public string taxtype { get; set; }
        //--EXCISE
        public string TaxExcisePercentage { get; set; }
        public string TaxExciseAmount { get; set; }
        public string TaxCESSPercentage { get; set; }
        public string TaxCESSAmount { get; set; }
        public string TaxSHCESSPercentage { get; set; }
        public string TaxSHCESSAmount { get; set; }

        //--VAT/CST
        public string vatorcst { get; set; }
        public string taxvatcst { get; set; }
        public string TaxvatorcstAmount { get; set; }

        //--TRANSPORT
        public string TransportCharges { get; set; }

        //--DISCOUNT
        public string DiscountType { get; set; }
        public string DiscountValue { get; set; }
        public string DiscountFlatPercentage { get; set; }

        public string TotalAmount { get; set; }
        public string BasicAmount { get; set; }

    }
    #endregion

    #region PhysicalStockUpdateDTO
    public class PhysicalStockUpdateDTO
    {

        //dropdown DETAILS
        public int Userid { get; set; }
        public string Username { get; set; }
        public int productid { get; set; }
        public string productname { get; set; }
        public int productcategoryid { get; set; }
        public string categoryname { get; set; }
        public int productsubcategoryid { get; set; }
        public string subcategoryname { get; set; }
        public string uom { get; set; }
        public decimal minstock { get; set; }
        public decimal maxstock { get; set; }
        public decimal PhysicalStock { get; set; }
        public decimal Stockinstore { get; set; }
        public decimal closingstock { get; set; }
        public string Date { get; set; }
        public string Storagelocation { get; set; }
        public int Storagelocationid { get; set; }
        public int Shelfid { get; set; }
        public string Shelfname { get; set; }
        public string issueno { get; set; }
        public string issuereturnno { get; set; }
        public string vchindentno { get; set; }
        public string vchissueno { get; set; }
        public string vchindentuom { get; set; }
        public string numindentconvertionqty { get; set; }
        public string vchissuetype { get; set; }
        public int CreatedBy { get; set; }

    }
    #endregion

    #region  Indent Details

    public class MaterialIndentDTO
    {
        //BE 
        public string IndentNo { get; set; }
        public string Indenttypenew { get; set; }
        public string Indenttypeexisting { get; set; }
        public DateTime vchdate { get; set; }
        public string RequestedBy { get; set; }
        public string vchindentno { get; set; }
        public string ApprovedBy { get; set; }
        public decimal? indentqty { get; set; }

        // Grid Fields
        public string productname { get; set; }
        public string categoryname { get; set; }
        public string subcategoryname { get; set; }
        public string producttype { get; set; }
        public string uomname { get; set; }
        public decimal? minqty { get; set; }
        public decimal? maxqty { get; set; }
        public string storagelocationname { set; get; }
        public string storagelocationcode { get; set; }
        public string shelfname { get; set; }
        public decimal? AvailableQty { get; set; }
        public int? createdby { set; get; }
        public int? productid { set; get; }
        public int? productcategoryid { get; set; }
        public int? productsubcategoryid { get; set; }
        public string productcode { get; set; }
        public string vchuomid { get; set; }
        public string uomid { get; set; }
        public string uom { get; set; }
        public int? recordid { get; set; }
        public int? recordid2 { get; set; }

        public string storagetext { get; set; }

        public string storageareavalue { get; set; }

        public int? deptid { get; set; }
        public string DeparmentName { get; set; }
        public string DeparmentCode { get; set; }
        //  public string categoryname { get; set; }
    }


    #endregion

    /// <vijay>
    /// vijay kandimalla
    /// </summary>
    #region  Indent

    public class IndentGenerationDTO
    {
        //BE 
        public string IndentNo { get; set; }
        public string Indenttypenew { get; set; }
        public string Indenttypeexisting { get; set; }
        public DateTime vchdate { get; set; }
        public string RequestedBy { get; set; }
        public string vchindentno { get; set; }
        public string ApprovedBy { get; set; }

        // Grid Fields
        public string productname { get; set; }
        public string categoryname { get; set; }
        public string subcategoryname { get; set; }
        public string producttype { get; set; }
        public string uomname { get; set; }
        public decimal? minqty { get; set; }
        public decimal? maxqty { get; set; }
        public string storagelocationname { set; get; }
        public string storagelocationcode { get; set; }
        public string shelfname { get; set; }
        public decimal? AvailableQty { get; set; }

        public int createdby { set; get; }

        public int productid { set; get; }
        public int productcategoryid { get; set; }
        public int productsubcategoryid { get; set; }
        public string productcode { get; set; }
        public string vchuomid { get; set; }
        public string uomid { get; set; }
        public string uom { get; set; }

        //  protected string AvailableQty { get; set; } 
        //  protected string IndentUOM { get; set; } 
        // public string Products { get; set; }
        // public int ProductsId { get; set; } 
        // productname,categoryname,subcategoryname,producttype,
        // uomname,minqty,maxqty,maxqty-minqty as availaibleqnty  
        //storagelocationname,storagelocationcode
        //shelfname,shelfcode,storagearea,storagecode 
        //   public string shelfcode { get; set; }
        //  public string storagearea { get; set; }
        //  public string storagecode { get; set; }
        //vchindentno,vchrequestedby 
        //  public string vchrequestedby { get; set; }














    }
    #endregion

    #region VendorPayment



    public class VendorPaymentDTO
    {


        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string VPDate { get; set; }
        public string TransNo { get; set; }
        public string TransType { get; set; }
        public string TransDate { get; set; }
        public double TransAmount { get; set; }
        public double TransDueAmount { get; set; }
        public decimal TransInvPaidAmount { get; set; }
        public double vpPaidAmount { get; set; }
        public string Narration { get; set; }
        public string PaymentMode { get; set; }
        public string vchmobilization { get; set; }
        public string BankName { get; set; }
        public string BankRecordId { get; set; }
        public string BankId { get; set; }
        public string BankAccId { get; set; }
        public string ChequeNO { get; set; }
        public string ChequeBookId { get; set; }
        public double dueamt { get; set; }
        public string ReferenceNo { get; set; }
        public string ChqDate { get; set; }
        public string AccountTransNO { get; set; }
        public string TransOrderno { get; set; }
        public string status { get; set; }
        public int createdby { get; set; }
        public string createddate { get; set; }
        public int modifiedby { get; set; }
        public string modifieddate { get; set; }
        public bool Select { get; set; }
        public string vchpurchasevoucherno { get; set; }

        public string type { get; set; }
        public string vchinvoiceno { get; set; }

        public string PurchaseOrderNo { set; get; }

        public string PurchaseOrderDate { set; get; }

        public double Amount { set; get; }

        public double DueAmount { set; get; }

        public double PaidAmount { set; get; }

        public string InvoiceNo { set; get; }

        public string GrnNo { set; get; }

























        //public string PurchaseOrderNo { set; get; }

        //public string PurchaseOrderDate { set; get; }

        //public double Amount { set; get; }

        //public double DueAmount { set; get; }

        //public double PaidAmount { set; get; }




    }

    //public class VendorPaymentDTO
    //{


    //    public string PurchaseOrderNo { set; get; }

    //    public string PurchaseOrderDate { set; get; }

    //    public double Amount { set; get; }

    //    public double DueAmount { set; get; }

    //    public double PaidAmount { set; get; }




    //}



    #endregion


    #region Material return


    public class MaterialReturnDTO
    {
        public int recordid { get; set; }
        public string Number { get; set; }
        public int productid { get; set; }
        public string productname { get; set; }
        public string Returnuom { get; set; }
        public string uom { get; set; }
        public string Date { get; set; }
        public string Storagelocation { get; set; }
        public string Storagelocationcode { get; set; }
        public int Storagelocationid { get; set; }
        public int Shelfid { get; set; }
        public string Shelfname { get; set; }
        public string issueno { get; set; }
        public string issuereturnno { get; set; }
        public string vchindentno { get; set; }
        public string vchissueno { get; set; }
        public string vchindentuom { get; set; }
        public string convertionvalue { get; set; }
        public decimal qty { get; set; }
        public decimal Returnqty { get; set; }
        public string Returndby { get; set; }
        public string Recivedby { get; set; }
        public string returntype { get; set; }

    }

    #endregion

}
