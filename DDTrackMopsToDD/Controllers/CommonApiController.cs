using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public  class CommonApiController : ApiController
    {
        public CommonController cc = new CommonController();
        /// <summary>
        /// 
        /// </summary>
        public  String _controllerName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public  String _actionName { get; set; }
        /// <summary>
        /// Used for Logging
        /// </summary>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public String getCaller(String Procedure = "")
        {
            if (Procedure == "")
            {
                return String.Format("{0}/{1}", _controllerName, _actionName);
            }
            return String.Format("{0}/{1} Procedure : {2} ", _controllerName, _actionName, Procedure);

        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="Procedure"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public String getCaller(String Result,String Procedure = "")
        {
            if (Procedure == "")
            {
                return String.Format("{0}/{1} {2} ", _controllerName, _actionName, Result);
            }
            return String.Format("{0}/{1} {2} Procedure : {3} ", _controllerName, _actionName, Result, Procedure);

        }

    }
}
