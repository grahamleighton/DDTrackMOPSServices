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
    public class FGHItemController : CommonApiController
    {
        private FGHItemDataController dc = new FGHItemDataController();


        List<FGHItem> validList = new List<FGHItem>();

        /// <summary>
        /// Constructor
        /// </summary>
        public FGHItemController()
        {
            _controllerName = "FGHItemController";
        }

        private void CheckModel(FGHItem item)
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            this.Validate(item);

            string JSON = item.ToJSON(0,"{{", "}}");

            if (ModelState.IsValid)
            {
                Log.Information(String.Format("{0} {1}", getCaller("MODEL VALID"), JSON));
                validList.Add(item);
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
       // /// <summary>
       // /// 
       // /// 
       // /// </summary>
       // /// <returns></returns>
       //[HttpGet]
       // public async Task<IHttpActionResult> Post()
       // {
       //     byte[] json = await Request.Content.ReadAsByteArrayAsync();
       //     var str = System.Text.Encoding.Default.GetString(json);



       //     return Ok();

       // }

        /// <summary>
        /// Add a new item 
        /// can accept a single line ( cat ) item with a single option OR a single line item ( cat ) 
        /// with multiple options
        /// </summary>
        ///   
      
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult PostNewItem( List<FGHItem> items)
        {
            _actionName = "PostNewItem";
            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }
            if ( items == null  )
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request as parameter items = null ", getCaller()));
               

                return BadRequest();
            }

            foreach (FGHItem item in items)
            {
                CheckModel(item);
            }


            // add items
            try
            {
                string Feedback = "";
                foreach (FGHItem i in items)
                {
                    long Id = 0;
                    returnValue rc = dc.AddFGHItemOption (i, out Feedback,out Id);
                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));

                        return BadRequest(String.Format("{0} : Item {1}", Feedback , i.FGHItemNumber ));
                    }
                    else
                    {
                        Log.Information(String.Format("{0} {1}", getCaller("SUCCESS"), _actionName, ""));
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(String.Format("{0} {1}", getCaller() ,  ex.Message));
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));

                return BadRequest("Item Not Added");
            }


            if (validList.Count == 0)
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Supplier Not Added");
            }


            if (validList.Count != items.Count)
            {
                return Json(validList);
            }

            Log.Information(String.Format("{0} Returning 200 OK", getCaller()));
            return Ok();

        }

      
    }
}