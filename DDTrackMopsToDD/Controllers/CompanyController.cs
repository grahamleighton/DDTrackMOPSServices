using DDTrackPlusCommon.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

using Serilog;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// Sets the Company Details information for use in each Order
    /// </summary>
    public class CompanyController : CommonApiController
    {
        private CompanyDataController dc = new CompanyDataController();

        List<CompanyDetails> validList = new List<CompanyDetails>();

        /// <summary>
        /// 
        /// 
        /// </summary>
        public CompanyController()
        {
            _controllerName = "CompanyController";
        }



        private void CheckModel(CompanyDetails cd)
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            this.Validate(cd);

            string JSON = cd.ToJSON("{{", "}}");

            if (ModelState.IsValid)
            {
                Log.Information(String.Format("{0} {1}", getCaller("MODEL VALID"), JSON));
                validList.Add(cd);
            }
            else
            {
                Log.Error(String.Format("{0} {1}", getCaller("MODEL INVALID"), JSON));
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
        /// Add a new Company
        /// </summary>
        /// <param name="companies"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult PostNewCompany( List<CompanyDetails> companies)
        {
            _actionName = "PostNewCompany";

            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }

            foreach (CompanyDetails cd  in companies)
            {
                CheckModel(cd);
            }

            // add company
            try
            {
                string Feedback = "";
                foreach (CompanyDetails cd in validList)
                {
                    long CompanyID = 0;
                    returnValue rc = dc.AddCompany(cd, out Feedback,out CompanyID);

                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                        return BadRequest(String.Format("{0} {1}", Feedback, cd.ToDisplay()));
                    }
                }
            }
            catch(Exception ex)
            {
               
                Log.Error(String.Format("{0} {1}", getCaller(), ex.Message));
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Company Not Added");
            }

            if ( validList.Count == 0 )
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Company Not Added");
            }

            if (validList.Count != companies.Count)
            {
                Log.Information(String.Format("{0} Returning 200 OK with payload as errors occurred", getCaller()));
                return Json(validList);
            }

            Log.Information(String.Format("{0} Returning 200 OK", getCaller()));
            return Ok();

        }
    }
}