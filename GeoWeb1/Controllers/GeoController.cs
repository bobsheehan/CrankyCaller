using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using GeoWeb1.Scrapers;


namespace WebApi.Controllers
{
   
    
    public class GeoController : ApiController
    {


        [HttpGet]
        [HttpPost]
        [Route("api/ping")]
        public IHttpActionResult Ping()
        {

            return Json(new GenericResult { result = "1", payload = "yeah, yeah, yeah.... I'm listening... now what do you want?" });
        }


        [HttpGet]
        [HttpPost]
        [Route("api/GeoWho")]
        public IHttpActionResult GeoWhoOpenStreet(double Lat, Double Lon, int zoom = 18) {

            string URL = "http://nominatim.openstreetmap.org/reverse?format=json&addressdetails=1";
            URL = URL + "&zoom=" + zoom + "&lat=" + Lat + "&lon=" + Lon;
            string responseFromServer = "";
            try
            {
                responseFromServer =  ScraperHelper.GetURL(URL);

            }
            catch (Exception ex)
            {
                return Ok(new GenericResult { result = "0", payload = ex.Message });

            }
            return Ok(new GenericResult { result = "1", payload = responseFromServer });
        }


        [HttpGet]
        [HttpPost]
        [Route("api/GeoYellowPage")]
        public IHttpActionResult GeoYellowpage(int houseNumber = 1, string road = "", string town = "",
            string state = "", string zip = "")
        {

            string URL = "http://people.yellowpages.com/whitepages/address?street=" +
                 houseNumber + ' ' + road + "&qloc=" + town + "+" + state + "+" + zip;

            try
            {

                string responseFromServer = ScraperHelper.GetURL(URL);

                List<WhitePageHit> mynums = ParseWpList(responseFromServer);

                return Ok(new PhoneLookupResult { result = mynums.Count.ToString(), phoneList = mynums });

            }
            catch (Exception ex)
            {
                return Ok(new GenericResult { result = "0", payload = ex.Message });

            }

        }

        [HttpGet]
        [HttpPost]
        [Route("api/GeoYellowPageName")]
        public IHttpActionResult GeoYellowpageName(string fName="", string lName="", int houseNumber=-1, string street ="", string town = "",
       string state = "", string zip = "")
        {
            /* For a given name location find all phone numbers and names */

            string URL = "http://people.yellowpages.com/whitepages?first=" + fName 
                 + "&last=" + lName + "&zip=" + town + "&state=" + state;

            try
            {

                string responseFromServer = ScraperHelper.GetURL(URL);

                List<WhitePageHit> myPhoneNums = ParseWpList(responseFromServer);
                List<WhitePageHit> toRemove = new List<WhitePageHit>();
                foreach (WhitePageHit oneHit in myPhoneNums)
                {
                    if (!oneHit.address.Contains(street))
                    {
                        toRemove.Add(oneHit);
                    }
                }
                myPhoneNums.RemoveAll(x => toRemove.Contains(x));

                return Ok(new PhoneLookupResult { result = myPhoneNums.Count.ToString(), phoneList = myPhoneNums });

            }
            catch (Exception ex)
            {
                return Ok(new GenericResult { result = "0", payload = ex.Message });

            }

        }

    
        [HttpGet]
        [HttpPost]
        [Route("api/GeoSpokeTo")]
        public IHttpActionResult GeoSpokeTo(int houseNumber = 1, string road = "", string town = "",
            string state = "", string zip = "")
        {

      
            try
            {

                Spoketo myScraper = ScraperFactory.Create<Spoketo>(new CrankyAddress { state = state, city = town, road = road, housenum = houseNumber });
                return Ok(myScraper.ScraperResults());
            
            }
            catch (Exception ex)
            {
                return Ok(new GenericResult { result = "0", payload = ex.Message });

            }

        }

#region privateHelpers

       
 
        private List<WhitePageHit> ParseWpList(string pageHtml)
        {
            var html = new HtmlDocument();
            html.LoadHtml(pageHtml);
            List<WhitePageHit> mynums = new List<WhitePageHit>();
            int hits = 0;

            foreach (HtmlNode node in html.DocumentNode.SelectNodes("//div[@class='result-single']") ?? Enumerable.Empty<HtmlNode>())
            {

                string selector = "div[@class='result-left']";

                foreach (HtmlNode node2 in node.SelectNodes(selector) ?? Enumerable.Empty<HtmlNode>())
                {
                    WhitePageHit myHit = new WhitePageHit();
                    foreach (HtmlNode datanode in node2.SelectNodes("a") ?? Enumerable.Empty<HtmlNode>())
                    {
                        myHit.name = datanode.InnerText;

                    }
                    foreach (HtmlNode datanode in node2.SelectNodes("div") ?? Enumerable.Empty<HtmlNode>())
                    { /*run through all the divs in theis node and take the last result found for each div class type - has been only 1 each at this point*/
                        string attributeValue = datanode.GetAttributeValue("class", "");
                        if (attributeValue == "phone")
                        {
                            myHit.phoneNumber = datanode.InnerText;
                        }
                        if (attributeValue == "address")
                        {
                            myHit.address = datanode.InnerText;
                        }
                    }
                    if ((myHit.name != null) && (myHit.phoneNumber != null)){ 
                       mynums.Add(myHit);
                    hits++;
                }



                }

            }
            return mynums;
        }


#endregion
    }



}
 
  
