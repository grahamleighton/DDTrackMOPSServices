using DDTrackPlusCommon.Controllers;
using DDTrackPlusCommon.Models;
using System;
using System.Data.SqlClient;

using Serilog;
using System.Data;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class SupplierDataController : CommonDataController
    {
        private string _addAdminProcedure = "admin.AddSupplier";
        private string _addOrderLineProcedure = "ordercreate.AddSupplier";
  

        public SupplierDataController()
        {
            _controllerName = "SupplierDataController";
        }

        /// <summary>
        /// Adds a supplier to both Admin and Orderline
        /// </summary>
        /// <param name="supplier"></param>
        /// <param name="Feedback"></param>
        /// <param name="SupplierID"></param>
        /// <returns></returns>
        public returnValue AddSupplier(NewSupplier supplier, out string Feedback, out long SupplierID)
        {
            _actionName = "AddSupplier";
            returnValue rc = returnValue.RETURN_SUCCESS;

            clearError();
            Feedback = "";
            SupplierID = 0;
            try
            {

                rc = _addSupplier(supplier, _addAdminProcedure, out Feedback, out SupplierID);
                if ( rc == returnValue.RETURN_SUCCESS)
                {
                    rc = _addSupplier(supplier, _addOrderLineProcedure, out Feedback, out SupplierID);
                }
            }
            catch (Exception )
            {
                throw new Exception("Supplier not added");
            }


            return rc;
        }



        // GET: SupplierData
        private returnValue _addSupplier(NewSupplier supplier, string procedure, out string Feedback, out long SupplierID)
        {
            _actionName = "_addSupplier";
            if (!ModelState.IsValid)
            {

                Feedback = "Invalid supplier details";
                SupplierID = 0;
                setError(Feedback);
                Log.Error(String.Format("{0} {1} Model Error : {2}", getCaller("ERROR"), supplier.ToJSON(), Feedback));
                return returnValue.RETURN_FAILURE;
            }

            clearError();

            try
            {

                SqlCommand com = sql.sqlGetCommand(procedure);
                com.Parameters.AddWithValue("@SupplierCode", supplier.SupplierCode);
                com.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);
                SqlParameter sp = com.Parameters.AddWithValue("@Id", (long)0);
                sp.Direction = ParameterDirection.InputOutput;


                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    string m = supplier.ToJSON();
                    string msg = String.Format("{0}", m);
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");

                    Log.Error(String.Format ( "{0} {1}" , getCaller("ERROR",procedure) , msg));
                    if (sql.sqlError())
                        setError(sql.sqlErrorMessage());
                    Log.Error(String.Format("{0} {1}", getCaller("ERROR",procedure), sql.sqlErrorMessage()));

                    Feedback = "Failed to add record";
                    SupplierID = 0;
                    return returnValue.RETURN_FAILURE;
                }
                try
                {
                    string m = supplier.ToJSON();
                    string msg = String.Format("{0}", m);
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");

                    Log.Information(String.Format("{0} {1}" , getCaller("SUCCESS",procedure) , msg)) ;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex);
                    setError(ex.Message);
                    Log.Error(String.Format("{0} {1}", getCaller("ERROR","Logging Error"), ex.Message));
                    throw new Exception(ex.Message);

                }
                Feedback = "Added Successfully";
                SupplierID = (long)sp.Value;
                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                Log.Error(String.Format("{0} {1}", getCaller(), ex.Message));

                throw new Exception(ex.Message);
            }

        }

    }
}