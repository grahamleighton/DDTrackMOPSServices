using DDTrackMopsToDD.Models;
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


        private List<string> CheckModel(Order o)
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            List<string> allerrs = new List<string>();



            //this.Validate(o.SupplierDetails);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("Supplier {0} {1}", getCaller(), s));

            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}
            //this.Validate(o.CustomerDeliveryDetails);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("Customer {0} {1}", getCaller(), s));
            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}

            //this.Validate(o.ItemData);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("ItemData {0} {1}", getCaller(), s));
            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}
            //this.Validate(o.Company);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("Company {0} {1}", getCaller(), s));
            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}

            //this.Validate(o.Company.CompanyBusinessAddress);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("CompanyBusinessAddress {0} {1}", getCaller(), s));
            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}

            //this.Validate(o.Company.CompanyReturnAddress);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("CompanyReturnAddress {0} {1}", getCaller(), s));
            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}

            //this.Validate(o.Carrier);
            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(String.Format("Carrier {0} {1}", getCaller(), s));
            //        if (!allerrs.Contains(s)) allerrs.Add(s);
            //    }

            //}

            this.Validate(o);
            if (!ModelState.IsValid)
            {
                List<string> errs = cc.GetErrorListFromModelState(ModelState);

                foreach (string s in errs)
                {
                    Log.Error(String.Format("Order {0} {1}", getCaller(), s));
                    if (!allerrs.Contains(s)) allerrs.Add(s);
                }

            }

            string JSON = o.ToJSON();

            if ( allerrs.Count == 0 )
            {
                Log.Information(String.Format("{0} {1}", getCaller("MODEL VALID"), JSON));
                validList.Add(o);
                return new List<string>();
            }
            else
            {
                Log.Error(String.Format("{0} {1}", getCaller("MODEL INVALID"), JSON));
                    return allerrs;
            }

        }


        /// <summary>
        /// Adds a new Order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(List<OrderResult>))]
        public IHttpActionResult PostNewOrder(List<Order> order)
        {
            _actionName = "PostNewOrder";

            List<OrderResult> orderResults = new List<OrderResult>();

            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }

            foreach (Order o in order)
            {
                OrderResult ordRes = new OrderResult();
                List<string> errors = CheckModel(o);
                if ( errors != null && errors.Count > 0 )
                {
                    try
                    {
                        ordRes.Reset(o);
                        foreach( string s in errors )
                        {
                            ordRes.Result.Add(s);
                        }
                        orderResults.Add(ordRes);
                    }
                    catch(Exception )
                    {
                        ordRes.setResult("Invalid model");
                        orderResults.Add(ordRes);
                    }
                }
            }


            //if (!ModelState.IsValid)
            //{
            //    List<string> errs = cc.GetErrorListFromModelState(ModelState);

            //    foreach (string s in errs)
            //    {
            //        Log.Error(s);
            //    }
            //    return BadRequest(ModelState);

            //}

            try
            {
                string Feedback = "";
                OrderID ids = new OrderID();
               
                foreach (Order o in validList)
                {
                    returnValue rc = returnValue.RETURN_SUCCESS;

                    ids.Reset();
                    OrderResult ordResult = new OrderResult();

                    ordResult.Reset(o);

                    

                   
                    try
                    {
                        // add the supplier details 
                        supplier_dc.clearError();
                        rc = supplier_dc.AddSupplier(o.SupplierDetails, out Feedback, out ids.SupplierID);                      
                        if ( rc != returnValue.RETURN_SUCCESS)
                        {
                           
                            throw new Exception(String.Format("AddSupplier : {0}",  supplier_dc.getError()));
                        }
                        // add the company details
                        rc = company_dc.AddCompany(o.Company, out Feedback, out ids.CompanyID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                           
                            throw new Exception(String.Format("AddCompany : {0}" , company_dc.getError()));
                        }

                        // add the item details
                        rc = item_dc.AddFGHItemOption(o.ItemData, out Feedback, out ids.FGHItemID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(String.Format("Item : {0}", item_dc.getError()));
                        }

                        // add the customer delivery details
                        rc = delivery_dc.AddCustomerDeliveryDetails(o.CustomerDeliveryDetails, out Feedback, out ids.CustomerDeliveryID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(String.Format("CustomerAddress : {0}" , delivery_dc.getError()));
                        }

                        // add the customer delivery details
                        rc = carrier_dc.AddCarrier (o.Carrier, out Feedback, out ids.CarrierID);
                        if (rc != returnValue.RETURN_SUCCESS)
                        {
                            throw new Exception(String.Format("Carrier : {0}" , carrier_dc.getError()) );
                        }

                        if (ids.isValid())
                        {
                            long OrderID = 0;
                            rc = dc.AddOrder(o, ids, out Feedback, out OrderID);
                            ids.ID = OrderID;
                            if (rc != returnValue.RETURN_SUCCESS)
                            {
                                throw new Exception(String.Format("Order : {0}" , dc.getError()));
                            }
                            if ( ids.ID > 0 )
                            {
                                dc.AddSpecialRequest(o, ids.ID);
                            }
                        }
                        else
                        {
                            throw new Exception("Bad Order IDS");
  //                          Log.Error(String.Format("{0} Returning 400 Bad Request as Order IDs not valid", getCaller()));
  //                          return BadRequest(String.Format( "Order Not Added Due To Bad IDs {0}", ids.getInvalidIds()));
                        }

                        ordResult.setResult("OK");

                    }
                    catch(Exception ex)
                    {
                        ordResult.setResult(ex.Message);
                        Log.Error(String.Format("{0} {1} ", getCaller() , ex.Message));
                   //     return BadRequest(ex.Message);
                    }

                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        ordResult.setResult("Unknown error");
//                        return BadRequest(String.Format("An Error Occurred while adding {0}", o.OrderLineId));
                    }

                    orderResults.Add(ordResult);
                }
            }
            catch (Exception )
            {
//                ordResult.setResult("Unknown error");
                return BadRequest("Order Not Added");
            }


            return Ok(orderResults);

        }
    }

}

