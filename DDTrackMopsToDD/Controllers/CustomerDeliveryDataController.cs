using DDTrackPlusCommon.Controllers;
using DDTrackPlusCommon.Models;
using Serilog;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// Loads / amends FGH Item Data for orders
    /// </summary>
    public class CustomerDeliveryDataController : CommonDataController
    {
        private string _addprocedure = "ordercreate.AddCustomerDeliveryDetails";

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomerDeliveryDataController()
        {
            _controllerName = "CustomerDeliveryDataController";
        }



        /// <summary>
        ///  Adds Item Data to the required tables
        /// </summary>
        /// <param name="custdata"></param>
        /// <param name="Feedback">Any Error Message rsulting from a failure</param>
        /// <param name="Id">The returned Id from the database record</param>
        /// <returns></returns>
        public returnValue AddCustomerDeliveryDetails(CustomerDeliveryDetails custdata, out string Feedback, out long Id)
        {
            _actionName = "AddCustomerDeliveryDetails";
            Feedback = "";
            Id = 0;
            if (!ModelState.IsValid)
            {

                Feedback = "Invalid Customer Delivery details";
                setError(Feedback);
                return returnValue.RETURN_FAILURE;
            }

            clearError();

            try
            {

                SqlCommand com = sql.sqlGetCommand(_addprocedure);

                com.Parameters.AddWithValue("@CustomerName", custdata.CustomerName);
                com.Parameters.AddWithValue("@AccountNumber", custdata.AccountNumber);
                com.Parameters.AddWithValue("@AddressLine1", custdata.Line1 );
                com.Parameters.AddWithValue("@AddressLine2", custdata.Line2);
                com.Parameters.AddWithValue("@AddressLine3", custdata.Line3);
                com.Parameters.AddWithValue("@AddressLine4", custdata.Line4);
                com.Parameters.AddWithValue("@AddressLine5", custdata.Line5);
                com.Parameters.AddWithValue("@AddressLine6", custdata.Line6);
                com.Parameters.AddWithValue("@Country", custdata.Country);
                com.Parameters.AddWithValue("@Postcode", custdata.Postcode);
                com.Parameters.AddWithValue("@EmailAddress", custdata.EmailAddress);
                com.Parameters.AddWithValue("@PhoneNumber1", custdata.PhoneNumber1);
                com.Parameters.AddWithValue("@PhoneNumber2", custdata.PhoneNumber2);

                SqlParameter sp = com.Parameters.AddWithValue("@Id", (long)0);
                sp.Direction = ParameterDirection.InputOutput;
                bool resp = sql.sqlRunCommand(com);

                if (!resp)
                {
                    string msg = custdata.ToJSON();
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");

                    Log.Error(String.Format("{0} {1}" , getCaller() ,  msg));
                    if (sql.sqlError())
                    {
                        setError(sql.sqlErrorMessage());
                        Log.Error("{0} {1}" , getCaller() , sql.sqlErrorMessage());
                    }
                    else
                    {
                        Log.Error(String.Format("{0} failed call to {1}", getCaller() ,  _addprocedure));
                    }
                    Feedback = "Failed to add record";
                    return returnValue.RETURN_FAILURE;
                }
                try
                {

                    string msg = custdata.ToJSON();
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");
                        Id = (long)com.Parameters["@Id"].Value;
                    Log.Information(String.Format("{0} {1}" , getCaller("SUCCESS") ,  msg));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex);
                    setError(ex.Message);
                        Log.Error("{0} {1}", getCaller(), ex.Message);
                    throw new Exception(ex.Message);

                }

              
                Feedback = "Added Successfully";
                return returnValue.RETURN_SUCCESS;
            }
            catch (Exception ex)
            {
                setError(ex.Message);
                Log.Error("{0} {1}", getCaller(), ex.Message);

                throw new Exception(ex.Message);
            }
        }
    }
}
