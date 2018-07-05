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
    public class FGHItemDataController : CommonDataController
    {
        private string _addprocedure = "ordercreate.AddFGHItemOption";

        /// <summary>
        /// Constructor
        /// </summary>
        public FGHItemDataController()
        {
            _controllerName = "FGHItemDataController";
        }



        /// <summary>
        ///  Adds Item Data to the required tables
        /// </summary>
        /// <param name="itemdata"></param>
        /// <param name="Feedback">Any Error Message rsulting from a failure</param>
        /// <param name="Id">The returned Id from the database record</param>
        /// <returns></returns>
        public returnValue AddFGHItemOption(FGHItem itemdata, out string Feedback, out long Id)
        {
            _actionName = "AddFGHItemOption";
            Feedback = "";
            Id = 0;
            if (!ModelState.IsValid)
            {

                Feedback = "Invalid item details";
                setError(Feedback);
                return returnValue.RETURN_FAILURE;
            }

            clearError();

            try
            {

                SqlCommand com = sql.sqlGetCommand(_addprocedure);

                com.Parameters.AddWithValue("@ItemNumber", itemdata.FGHItemNumber);
                com.Parameters.AddWithValue("@ItemDescription", itemdata.FGHItemDescription);


                foreach ( FGHOptionNumber optData in itemdata.OptionData)
                {
                    if (com.Parameters.Count == 2)
                    {
                        com.Parameters.AddWithValue("@OptionNumber", optData.OptionNumber);
                        com.Parameters.AddWithValue("@OptionDescription", optData.OptionDescription);
                        com.Parameters.AddWithValue("@SupplierItemNumber", optData.SupplierItemNumber);
                        com.Parameters.AddWithValue("@OptionWeight", optData.OptionWeight);
                        SqlParameter sp = com.Parameters.AddWithValue("@Id", (long)0);
                        sp.Direction = ParameterDirection.InputOutput;
                    }
                    else
                    {
                        com.Parameters["@OptionNumber"].Value = optData.OptionNumber;
                        com.Parameters["@OptionDescription"].Value = optData.OptionDescription;
                        com.Parameters["@SupplierItemNumber"].Value = optData.SupplierItemNumber;
                        com.Parameters["@OptionWeight"].Value = optData.OptionWeight;
                        com.Parameters["@Id"].Value = (long)0;
                    }

                    

                
                bool resp = sql.sqlRunCommand(com);


                if (!resp)
                {
                    string msg = optData.ToJSON();
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

                    string msg = itemdata.ToJSON(optData);
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
