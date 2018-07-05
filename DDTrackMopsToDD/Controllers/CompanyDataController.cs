using DDTrackPlusCommon.Controllers;
using DDTrackPlusCommon.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class CompanyDataController : CommonDataController
    {
        private string _addprocedure = "ordercreate.AddCompany";


        public CompanyDataController()
        {
            _controllerName = "CompanyDataController";
        }
        public returnValue AddCompany(CompanyDetails company, out string Feedback, out long CompanyID)
        {
            _actionName = "AddCompany";
            Feedback = "";
            CompanyID = 0;
            if (!ModelState.IsValid)
            {
                Feedback = "Invalid company details";
                setError(Feedback);
                return returnValue.RETURN_FAILURE;
            }

            clearError();

            try
            {

                SqlCommand com = sql.sqlGetCommand(_addprocedure);
                //      com.Parameters.AddWithValue("@SupplierCode", supplier.SupplierCode);
                //      com.Parameters.AddWithValue("@SupplierName", supplier.SupplierName);


              
                List<KeyValuePair<string, string>> kvp = new List<KeyValuePair<string, string>>();

                kvp.Add(new KeyValuePair<string, string>("@CompanyCode", company.CompanyCode));
                kvp.Add(new KeyValuePair<string, string>("@CompanyName", company.CompanyName));
                kvp.Add(new KeyValuePair<string, string>("@BusAddrLine1", company.CompanyBusinessAddress.Line1));
                kvp.Add(new KeyValuePair<string, string>("@BusAddrLine2", company.CompanyBusinessAddress.Line2));
                kvp.Add(new KeyValuePair<string, string>("@BusAddrLine3", company.CompanyBusinessAddress.Line3));
                kvp.Add(new KeyValuePair<string, string>("@BusAddrTown", company.CompanyBusinessAddress.Town));
                kvp.Add(new KeyValuePair<string, string>("@BusAddrCounty", company.CompanyBusinessAddress.County));
                kvp.Add(new KeyValuePair<string, string>("@BusAddrPostCode", company.CompanyBusinessAddress.Postcode));
                kvp.Add(new KeyValuePair<string, string>("@RetAddrLine1", company.CompanyReturnAddress.Line1));
                kvp.Add(new KeyValuePair<string, string>("@RetAddrLine2", company.CompanyReturnAddress.Line2));
                kvp.Add(new KeyValuePair<string, string>("@RetAddrLine3", company.CompanyReturnAddress.Line3));
                kvp.Add(new KeyValuePair<string, string>("@RetAddrTown", company.CompanyReturnAddress.Town));
                kvp.Add(new KeyValuePair<string, string>("@RetAddrCounty", company.CompanyReturnAddress.County));
                kvp.Add(new KeyValuePair<string, string>("@RetAddrPostCode", company.CompanyReturnAddress.Postcode));



                foreach ( KeyValuePair<string,string> kp in kvp)
                {
                    com.Parameters.AddWithValue(kp.Key, kp.Value);
                }
                SqlParameter sp = com.Parameters.AddWithValue("@Id", (long)0);
                sp.Direction = ParameterDirection.InputOutput;


                bool resp = sql.sqlRunCommand(com);


                if (!resp)
                {
                    //        string m = supplier.ToJSON();
                    //        string msg = String.Format("addSupplier Model {0} error : {1}", m, sql.sqlErrorMessage());
                    //        msg = msg.Replace("{", "{{");
                    //        msg = msg.Replace("}", "}}");
                    string msg = company.ToJSON("{{", "}}");


                  
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

                    string msg = String.Format("{0} Company added OK {1}", getCaller("SUCCESS",_addprocedure) ,company.ToJSON());
                    msg = msg.Replace("{", "{{");
                    msg = msg.Replace("}", "}}");
                    CompanyID = (long)sp.Value;
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
