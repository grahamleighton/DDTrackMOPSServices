using DDTrackPlusCommon.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

using Serilog;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// Sets the Carrier Details information for use in each Order
    /// </summary>
    public class CarrierController : CommonApiController
    {
        private CarrierDataController dc = new CarrierDataController();

        List<CarrierDetails> validList = new List<CarrierDetails>();

        /// <summary>
        /// 
        /// 
        /// </summary>
        public CarrierController()
        {
            _controllerName = "CarrierController";
        }



        private void CheckModel(CarrierDetails cd)
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            this.Validate(cd);

            string JSON = cd.ToJSON();

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
        /// Add a new Carrier
        /// </summary>
        /// <param name="companies"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult PostNewCarrier( List<CarrierDetails> companies)
        {
            _actionName = "PostNewCarrier";

            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }

            foreach (CarrierDetails cd  in companies)
            {
                CheckModel(cd);
            }

            // add Carrier
            try
            {
                string Feedback = "";
                foreach (CarrierDetails cd in validList)
                {
                    long CarrierID = 0;
                    returnValue rc = dc.AddCarrier(cd, out Feedback,out CarrierID);

                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                        return BadRequest(String.Format("{0} {1}", Feedback, cd.ToJSON()));
                    }
                }
            }
            catch(Exception ex)
            {
               
                Log.Error(String.Format("{0} {1}", getCaller(), ex.Message));
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Carrier Not Added");
            }

            if ( validList.Count == 0 )
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Carrier Not Added");
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