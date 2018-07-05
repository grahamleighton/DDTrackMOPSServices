using DDTrackPlusCommon.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// Manages Orders from MOPS into DDTrack+
    /// </summary>
    public class OrderController : CommonApiController
    {
     //   private CommonController cc = new CommonController();
        private OrderDataController dc = new OrderDataController();
        private CompanyDataController company_dc = new CompanyDataController();
        private SupplierDataController supplier_dc = new SupplierDataController();
        private FGHItemDataController item_dc = new FGHItemDataController();
        private CustomerDeliveryDataController delivery_dc = new CustomerDeliveryDataController();
        private CarrierDataController carrier_dc = new CarrierDataController();

        List<Order> validList = new List<Order>();

        /// <summary>
        /// 
        /// 
        /// </summary>
        public OrderController()
        {
            _controllerName = "OrderController";
        }


        private void CheckModel(Order o)
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            this.Validate(o);

            string JSON = o.ToJSON();

            if (ModelState.IsValid)
            {
                Log.Information(String.Format("{0} {1}", getCaller("MODEL VALID"), JSON));
                validList.Add(o);
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
        /// Adds a new Order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult PostNewOrder(List<Order> order)
        {
            _actionName = "PostNewOrder";

            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }

            foreach (Order o in order)
            {
                CheckModel(o);
            }


            if (!ModelState.IsValid)
            {
                List<string> errs = cc.GetErrorListFromModelState(ModelState);

                foreach (string s in errs)
                {
                    Log.Error(s);
                }
                return BadRequest(ModelState);

            }

            try
            {
                string Feedback = "";
                foreach (Order o in order)
                {
                    returnValue rc = returnValue.RETURN_SUCCESS;
                    OrderID ids = new OrderID();
                              
                    try
                    {
                        // add the supplier details 
                        supplier_dc.clearError();
                        rc = supplier_dc.AddSupplier(o.SupplierDetails, out Feedback, out ids.SupplierID);                      
                        if ( rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(supplier_dc.getError());
                        }
                        // add the company details
                        rc = company_dc.AddCompany(o.Company, out Feedback, out ids.CompanyID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(company_dc.getError());
                        }

                        // add the item details
                        rc = item_dc.AddFGHItemOption(o.ItemData, out Feedback, out ids.FGHItemID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(item_dc.getError());
                        }

                        // add the customer delivery details
                        rc = delivery_dc.AddCustomerDeliveryDetails(o.CustomerDeliveryDetails, out Feedback, out ids.CustomerDeliveryID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(delivery_dc.getError());
                        }

                        // add the customer delivery details
                        rc = carrier_dc.AddCarrier (o.Carrier, out Feedback, out ids.CarrierID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(carrier_dc.getError());
                        }

                        if (ids.isValid())
                        {
                            long OrderID = 0;
                            rc = dc.AddOrder(o, ids, out Feedback, out OrderID);
                            ids.ID = OrderID;
                            if (rc != returnValue.RETURN_SUCCESS)
                            {
                                throw new Exception(dc.getError());
                            }
                            if ( ids.ID > 0 )
                            {
                                dc.AddSpecialRequest(o, ids.ID);
                            }
                        }
                        else
                        {

                            Log.Error(String.Format("{0} Returning 400 Bad Request as Order IDs not valid", getCaller()));
                            return BadRequest(String.Format( "Order Not Added Due To Bad IDs {0}", ids.getInvalidIds()));
                        }


                    }
                    catch(Exception ex)
                    {
                        Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                        return BadRequest(ex.Message);
                    }

                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        return BadRequest(String.Format("An Error Occurred while adding {0}", o.OrderLineId));
                    }
                }
            }
            catch (Exception )
            {
                return BadRequest("Order Not Added");
            }


            return Ok();

        }
    }

}

