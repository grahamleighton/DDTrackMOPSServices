using DDTrackPlusCommon.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

using Serilog;


namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// Sets the Supplier Information for use in each Order and also the main system for use in the Portal
    /// </summary>
    public class SupplierController : CommonApiController
    {
        private SupplierDataController dc = new SupplierDataController();

        List<NewSupplier> validList = new List<NewSupplier>();

        /// <summary>
        /// 
        /// 
        /// </summary>
        public SupplierController()
        {
            _controllerName = "SupplierController";
        }

     
        private void CheckModel(NewSupplier supp )
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            this.Validate(supp);

            string suppJSON = supp.ToJSON("{{", "}}");

            if (ModelState.IsValid)
            {
                Log.Information(String.Format("{0} {1}", getCaller("MODEL VALID"), suppJSON));
                validList.Add(supp);
            }
            else
            {
                Log.Error(String.Format("{0} {1}", getCaller("MODEL INVALID"), suppJSON));
                if (!ModelState.IsValid)
                {
                    List<string> errs = cc.GetErrorListFromModelState(ModelState);
                    
                    foreach (string s in errs)
                    {
                        Log.Error(String.Format("{0} {1}", getCaller(), s));
                    }
                }
            }
        }

        /// <summary>
        /// Add a new supplier
        /// </summary>
        ///   
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult PostNewSupplier( List<NewSupplier> supplier)
        {
            _actionName = "PostNewSupplier";

          
            
            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }

            foreach ( NewSupplier supp in supplier)
            {
                CheckModel(supp);
            }
               
       
            // add supplier 
            try
            {
                string Feedback = "";
                foreach (NewSupplier s in validList)
                {
                    long SupplierID = 0;
                    returnValue rc = dc.AddSupplier(s, out Feedback,out SupplierID);
                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                        return BadRequest(String.Format("An Error Occurred while adding {0}", s.ToDisplay()));
                    }
                    else
                    {
                        Log.Information(String.Format("{0} {1}", getCaller("SUCCESS"), s.ToJSON()));
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(String.Format ( "{0} {1}" , getCaller() , ex.Message)) ;
                return BadRequest("Supplier Not Added");
            }

            if (validList.Count == 0)
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Supplier Not Added");
            }


            if ( validList.Count != supplier.Count )
            {
                return Json(validList);
            }

            return Ok();

        }
    }
}