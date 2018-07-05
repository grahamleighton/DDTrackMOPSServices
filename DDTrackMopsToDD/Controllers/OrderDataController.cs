using DDTrackPlusCommon.Controllers;
using DDTrackPlusCommon.Models;
using Serilog;
using System;
using System.Data.SqlClient;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class OrderDataController : CommonDataController
    {
        private string _addprocedure = "ordercreate.AddOrder";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="OrderId"></param>
        /// <returns></returns>
        public returnValue AddSpecialRequest(Order o, long OrderId  )
        {
            string _specialrequestprocedure = "ordercreate.AddSpecialRequest";

            try
            {

                SqlCommand com = sql.sqlGetCommand(_specialrequestprocedure);

                int SequenceNo = 0;

                foreach (string det in o.SpecialRequests)
                {
                    SequenceNo++;
                    com.Parameters.Clear();
                    com.Parameters.AddWithValue("@OrderID", OrderId);
                    com.Parameters.AddWithValue("@Details", det);
                    com.Parameters.AddWithValue("@SequenceNo", SequenceNo);
                    SqlParameter sp =  com.Parameters.AddWithValue("@Success", 0);
                    sp.Direction = System.Data.ParameterDirection.InputOutput;
                    bool resp = sql.sqlRunCommand(com);
                    if (!resp)
                    {
                        try
                        {
                            string msg = String.Format("{0} Special Request {1} for Order Id {2}  added OK",
                              getCaller("ERROR", _specialrequestprocedure),
                              det, OrderId);
                            Log.Error(msg);

                        }
                        catch(Exception)
                        {

                        }
                        if (sql.sqlError())
                        {
                            setError(sql.sqlErrorMessage());
                            Log.Error(sql.sqlErrorMessage());
                        }
                        else
                        {
                            Log.Error(String.Format("failed call to {0}", _specialrequestprocedure));
                        }
                        return returnValue.RETURN_FAILURE;

                    }
                    else
                    {
                        string msg = String.Format("{0} Special Request \"{1}\" for Order Id {2}  added OK", 
                                getCaller("SUCCESS", _specialrequestprocedure),
                                det , OrderId  );
                        Log.Information(msg);
                    }
                }
                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);
            }

        }
       /// <summary>
       /// 
       /// 
       /// </summary>
       /// <param name="o"></param>
       /// <param name="ids"></param>
       /// <param name="Feedback"></param>
       /// <param name="OrderID"></param>
       /// <returns></returns>
        public returnValue AddOrder(Order o, OrderID ids ,  out string Feedback, out long OrderID)
        {
            Feedback = "";


            OrderID = 0;

            if (!ModelState.IsValid)
            {
                Feedback = "Invalid order details";
                setError(Feedback);
                return returnValue.RETURN_FAILURE;
            }

            clearError();

            try
            {

                SqlCommand com = sql.sqlGetCommand(_addprocedure);

                com.Parameters.AddWithValue("@SupplierID", ids.SupplierID);
                com.Parameters.AddWithValue("@CompanyID", ids.CompanyID);
                com.Parameters.AddWithValue("@CustomerDeliveryAddressID", ids.CustomerDeliveryID);
                com.Parameters.AddWithValue("@CarrierID", ids.CarrierID);
                com.Parameters.AddWithValue("@ItemOptionID", ids.FGHItemID);

                com.Parameters.AddWithValue("@OrderNumber", o.OrderNumber);
                com.Parameters.AddWithValue("@InvoiceNumber", o.InvoiceNumber);
                com.Parameters.AddWithValue("@DateOfOrder", o.DateOfOrder);
                com.Parameters.AddWithValue("@DespatchPromiseDate", o.DespatchPromiseDate);
                com.Parameters.AddWithValue("@DeliveryPromiseDate", o.DeliveryPromiseDate);
                com.Parameters.AddWithValue("@FGHBarcode", o.FGHBarcode);
                com.Parameters.AddWithValue("@EAN", o.EAN);
                com.Parameters.AddWithValue("@CostPrice", o.CostPrice);
                com.Parameters.AddWithValue("@NoOfItems", o.NoOfItems);
                com.Parameters.AddWithValue("@VanRound", o.VanRound);

                SqlParameter sp = com.Parameters.AddWithValue("@OrderId", (long)0);
                sp.Direction = System.Data.ParameterDirection.InputOutput;


                bool resp = sql.sqlRunCommand(com);
                if (!resp)
                {
                    //        string m = supplier.ToJSON();
                    //        string msg = String.Format("addSupplier Model {0} error : {1}", m, sql.sqlErrorMessage());
                    //        msg = msg.Replace("{", "{{");
                    //        msg = msg.Replace("}", "}}");
                    string msg = "msg";

                    Log.Information(msg);
                    if (sql.sqlError())
                    {
                        setError(sql.sqlErrorMessage());
                        Log.Error(sql.sqlErrorMessage());
                    }
                    else
                    {
                        Log.Error(String.Format("failed call to {0}", _addprocedure));
                    }
                    Feedback = "Failed to add record";
                    return returnValue.RETURN_FAILURE;
                }
                try
                {

                    string msg = String.Format("Order added OK");
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");

                    OrderID = (long)sp.Value;

                    Log.Information(msg);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex);
                    setError(ex.Message);
                    throw new Exception(ex.Message);

                }
                Feedback = "Added Successfully";
                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);

                throw new Exception(ex.Message);
            }
        }

    }
}
