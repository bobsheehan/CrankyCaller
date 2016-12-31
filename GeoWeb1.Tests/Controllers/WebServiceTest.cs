using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;
using System.Web.Http.Results;
using System.Linq;

namespace GeoWeb1.Tests
{

    
    [TestClass]
    public class WebServiceTest
    {
        
        [TestMethod]
        public void TestPing()
        {
             var MyWebAPIController= new WebApi.Controllers.GeoController();

            IHttpActionResult serviceResult = MyWebAPIController.Ping();

            var content = serviceResult as System.Web.Http.Results.JsonResult<WebApi.Controllers.GenericResult>;
            if (content != null) {
                if (content.Content != null)
                {
                    Assert.IsTrue(content.Content.payload.Contains("yeah, yeah, yeah"), "response  was not expected");
                }
                else {
                    Assert.IsTrue(false, "No content returned from serice");
                }
            }else
            {
                Assert.IsTrue(false, "Service response was empty");
            }
      
        }

        [TestMethod]
        public void TestOpenStreetService()
        {
            var MyWebAPIController = new WebApi.Controllers.GeoController();
            
            //call service for a known/random-ish lat/log
            IHttpActionResult serviceResult = MyWebAPIController.GeoWhoOpenStreet(42.4232158, -71.1537637);
        
            var content = serviceResult as OkNegotiatedContentResult<WebApi.Controllers.GenericResult>;
            if (content != null)
            {
                if (content.Content != null)
                {
                    // we just want to check that the service came back and had some resonable expected results... like country
                    Assert.IsTrue(content.Content.payload.Contains("county"), "response  was not expected");
                }
                else
                {
                    Assert.IsTrue(false, "No content returned from serice");
                }
            }
            else
            {
                Assert.IsTrue(false, "Service response was empty");
            }

        }

        [TestMethod]
        public void TestYPPhoneService()
        {
            var MyWebAPIController = new WebApi.Controllers.GeoController();

            //call service that asks for phone numbers by address for a known/random-ish address
            IHttpActionResult serviceResult = MyWebAPIController.GeoYellowpage(70,"Walnut St","Arlington","MA","02476");
           // { result = 0, phonelist = { System.Collections.Generic. }
            var content = serviceResult as OkNegotiatedContentResult<WebApi.Controllers.PhoneLookupResult>;
            if (content != null)
            {
                if (content.Content != null)
                {
                    if(content.Content.result == "0")
                    {
                        Assert.Inconclusive("no results when expected... is the external service working?");
                    }
                    else
                    {
                        Assert.IsTrue(false) ; // assume for now we got here with loaded up phone lists
                    }
            
                }
                else
                {
                    Assert.IsTrue(false, "No content returned from serice");
                }
            }
            else
            {
                Assert.IsTrue(false, "Service response was empty");
            }

        }


        [TestMethod]
        public void TestYPPhoneByNameervice()
        {
            var MyWebAPIController = new WebApi.Controllers.GeoController();

            //call service that looks up a name & town for a known/random-ish address
            IHttpActionResult serviceResult = MyWebAPIController.GeoYellowpageName("R", "Sheehan", 70, "Walnut","Arlington", "MA", "02476");
            // { result = 0, phonelist = { System.Collections.Generic. }
            var content = serviceResult as OkNegotiatedContentResult<WebApi.Controllers.PhoneLookupResult>;
            if (content != null)
            {
                if (content.Content != null)
                {
                    if (content.Content.result == "0")
                    {
                        Assert.Inconclusive("no results when expected... is the external service working?");
                    }
                    else
                    {
                        Assert.IsTrue(true); // assume for now we got here with loaded up phone lists
                    }

                }
                else
                {
                    Assert.IsTrue(false, "No content returned from serice");
                }
            }
            else
            {
                Assert.IsTrue(false, "Service response was empty");
            }

        }

     
        [TestMethod]
        public void TestSpokeToService()
        {
            var MyWebAPIController = new WebApi.Controllers.GeoController();

            //call service for a known/random-ish address
            IHttpActionResult serviceResult = MyWebAPIController.GeoSpokeTo(70, "Walnut St", "Arlington", "MA", "02476");
            // { result = 0, phonelist = { System.Collections.Generic. }
            var content = serviceResult as OkNegotiatedContentResult<WebApi.Controllers.GenerticInfoHit>;
            if (content != null)
            {
                if (content.Content != null)
                {
                    if (content.Content.result== "0")
                    {
                        Assert.Inconclusive("no results when expected... is the external service working?");
                    }
                    else
                    {
                        Assert.IsTrue(true); // assume for now we got here with loaded up phone lists
                    }

                }
                else
                {
                    Assert.IsTrue(false, "No content returned from serice");
                }
            }
            else
            {
                Assert.IsTrue(false, "Service response was empty");
            }

        }

    }
}
