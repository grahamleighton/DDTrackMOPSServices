using DDTrackPlusCommon.Controllers;
using DDTrackPlusCommon.Models;
using Serilog;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class CarrierDataController : CommonDataController
    {
        private string _addprocedure = "ordercreate.AddCarrier";

        /// <summary>
        /// 
        /// 
        /// </summary>
        public CarrierDataController()
        {
            _controllerName = "CarrierDataController";
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="carrier"></param>
        /// <param name="Feedback"></param>
        /// <param name="CarrierID"></param>
        /// <returns></returns>
        public returnValue AddCarrier(CarrierDetails carrier, out string Feedback, out long CarrierID)
        {
            _actionName = "AddCarrier";
            Feedback = "";
            CarrierID = 0;
            if (!ModelState.IsValid)
            {
                Feedback = "Invalid carrier details";
                setError(Feedback);
                return returnValue.RETURN_FAILURE;
            }

            clearError();

            try
            {

                SqlCommand com = sql.sqlGetCommand(_addprocedure);
                com.Parameters.AddWithValue("@CarrierCode", carrier.Code);
                com.Parameters.AddWithValue("@CarrierName", carrier.Name);

                SqlParameter sp = com.Parameters.AddWithValue("@Id", (long)0);
                sp.Direction = ParameterDirection.InputOutput;


                bool resp = sql.sqlRunCommand(com);


                if (!resp)
                {
                    string msg = carrier.ToJSON();

                    if (sql.sqlError())
                    {
                        setError(sql.sqlErrorMessage());
                        Log.Error(String.Format("{0} {1}", getCaller("ERROR", _addprocedure), sql.sqlErrorMessage()));
                    }
                    else
                    {
                        Log.Error(String.Format("{0} {1}", getCaller("ERROR", _addprocedure), "call failed" ));
                    }
                    Feedback = "Failed to add record";
                    return returnValue.RETURN_FAILURE;
                }
                try
                {

                    string msg = String.Format("{0} Carrier added OK", getCaller("SUCCESS",_addprocedure) );
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");
                    CarrierID = (long)sp.Value;
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
