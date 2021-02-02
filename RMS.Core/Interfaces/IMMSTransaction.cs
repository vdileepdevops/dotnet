using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace RMS.Core.Interfaces
{
    public interface IMMSTransaction
    {
        #region Goods Received Note
        DataTable getponumbers(string ID);
        DataTable getgrngriddetails(string VID, string POID);
        DataTable Getstoragelocations();
        DataTable getshelfs(string ID);
        bool SaveGRN(GoodsReceivedNoteDTO GoodsReceivedNoteDTO, List<GoodsReceivedNoteDTO> lstGoodsReceivedNoteDTO, GoodsReceivedNoteTAXDTO TAX);
        DataTable getuomlist();
        DataTable GetVendorProducts(string ID);
        DataTable BinduomConversionValues();
        DataTable getRequestPersons();
        #endregion

        #region Physical Stock
        List<PhysicalStockUpdateDTO> ShowStockusers();

        List<PhysicalStockUpdateDTO> ShowStockupdate(string locationid);
        List<PhysicalStockUpdateDTO> ShowStoragelocation();
        bool SaveStock(PhysicalStockUpdateDTO PhysicalStockUpdateDTO, List<PhysicalStockUpdateDTO> lstStockupdate);
        #endregion

        #region Old Commented Code(Indent)


        //List<MaterialIndentDTO> GetProductAvailability(string productid, string productname,string showroomselected,string storagelocationname,string uomname);

        //List<MaterialIndentDTO> GetExistingIndents(string Indents);

        //List<MaterialIndentDTO> GetExistingIndentNo(string IndentType);

        //bool SaveIndent(List<MaterialIndentDTO> griddata, MaterialIndentDTO BE);

        //List<MaterialIndentDTO> GetSelfdetails(string showroomselected);

        //List<MaterialIndentDTO> ShowDepartment();

        //List<MaterialIndentDTO> showrequestedby();

        //List<MaterialIndentDTO> ShowStorage();

        //List<MaterialIndentDTO> ShowIndentProducts();
        #endregion

        #region Metireal Indent By Vikram

       // List<MaterialIndentDTO> GetProductAvailability(string productid, string productname, string showroomselected, string storagelocationname, string uomname);
        List<MaterialIndentDTO> GetAvailablestock();
        List<MaterialIndentDTO> GetExistingIndents(string Indents);

        List<MaterialIndentDTO> GetExistingIndentNo(string IndentType);
        bool UpdateIndentDetails(List<MaterialIndentDTO> griddata, MaterialIndentDTO BE);
        bool SaveIndent(List<MaterialIndentDTO> griddata, MaterialIndentDTO BE, out string indentno);

        List<MaterialIndentDTO> GetSelfdetails(string showroomselected);

        List<MaterialIndentDTO> ShowDepartment();

        List<MaterialIndentDTO> showrequestedby();

       
        List<MaterialIndentDTO> ShowStorage(string storageareavalue);

        List<MaterialIndentDTO> ShowIndentProducts();

        bool Deleteproductdetails(MaterialIndentDTO gridrowdata);

        // List<MaterialIndentDTO> GetProductAvailabilityQty(string productid, string productname,string uomname, string storagearea, string shelfname);
        List<MaterialIndentDTO> GetReorderIndents(string IndentType);
      //  decimal GetProductAvailabilityQty(string productid, string productname, string uomname, string storagearea, string shelfname);
        #endregion

        /// <summary>
        /// Vikram Chathilla
        /// </summary>
        /// <returns></returns>
        #region IndentRelease

        List<IndentDTO> ShowProducts();
        List<IndentDTO> ShowIndentNumbers();
        List<IndentDTO> ShowIndentGridDetails(string objindentdto);
        List<IndentDTO> StorageLocationBind();
        int SaveIndentIssueDetails(IndentDTO ID, List<IndentDTO> ID2);
        List<IndentDTO> IssueDetailsBind(IndentDTO objindentissue);
        List<IndentDTO> ShowDirectIndentGridDetails(IndentDTO objproductname);
        int SaveDirectIndentIssueDetails(IndentDTO IND, List<IndentDTO> IND2);
        string DirectShelfDetails(IndentDTO objavailablequantity);
        string BindShelfAvailabilityDetails(IndentDTO objavailablequantity);
        DataTable StorageShelfDetails(string StorageID);
        List<IndentDTO> IssueProductStores(IndentDTO objindentissue);
        List<IndentDTO> IssueProductShelfsStores(IndentDTO objindentissue);

        List<IndentDTO> GetConversionvalues();
        #endregion


        #region Purchase Order
        DataTable getexistponumbers(string Type, string Vendorid);
        DataTable getpodetails(string poid, string Vendorid);
        PurchaseOrderTAXDTO getpotaxdetails(string poid, string Vendorid);

        #endregion

        #region Purchase Return
        DataTable getdropdownvalues(string strType, string vendorid);
        DataTable getpurchasereturnGridValues(string strtype, string ID);
        bool SavePurchaseReturnCancel(GoodsReceivedNoteDTO PURCHASE, List<GoodsReceivedNoteDTO> lstPURCHASE);
        #endregion


        #region VendorPayment





        List<VendorPaymentDTO> ShowVendorPaymentDetails(string VendorName);

        bool SaveVPDetails(VendorPaymentDTO lstTotalDetails, List<VendorPaymentDTO> lstInvDetails);
        DataTable GetVendorNames();
        DataTable GetBankNames();
        DataTable GetCheques(int id);







        #endregion


        #region Material Return

        List<MaterialReturnDTO> ShowIssuenumbers();
        List<MaterialReturnDTO> ShowIndentNos();

        List<MaterialReturnDTO> ShowMaterialReturn(string Number, string Returntype);

        bool SaveMaterialReturn(MaterialReturnDTO MaterialReturnDTO, List<MaterialReturnDTO> lstMaterialReturn);
   
        #endregion


        
    }
}
