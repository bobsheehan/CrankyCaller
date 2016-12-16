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

namespace WebApi.Controllers
{
    public class WhitePageHit
    {
        private string _name = null;
        private string _address = null;
        private string _phonenumber = null;
  

        public string phoneNumber { get { return _phonenumber; } set { _phonenumber = value; } }
        public string address { get { return _address; } set { _address = value; } }
        public string name { get { return _name; } set { _name = value; } }
    }

    public class Person {
        private string _fname = null;
        private string _lname = null;
        private string _age = null;
        public int seq = 0;
        public string fName { get { return _fname; } set { _fname = value; } }
        public string lName { get { return _lname; } set { _lname = value; } }
        public string age { get { return _age; } set { _age = value; } }
        public string phoneNumber { get; set; }
    }


    public class GenericResult {
        public string result { get; set; }
        public string payload { get; set; }
    }


    public class GenerticInfoHit
    {
        private List<Person> _MyPeopleInfo = new List<Person>();

        public string result { get; set; }

        public string sourceName { get; set; }
        public string houseValue { get; set; }
        public List<Person> peopleInfo { get { return _MyPeopleInfo; } set { _MyPeopleInfo = value; } }
    }


    public class PhoneLookupResult
    {
        public string result { get; set; }
        public List<WhitePageHit> phoneList { get; set; }

    }

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
                responseFromServer = GetURL(URL);

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

                string responseFromServer = GetURL(URL);

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

                string responseFromServer = GetURL(URL);

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

            string URL = "http://www.spokeo.com/" + state + "/" + town + "/" + houseNumber + "-" + road.Replace(" ", "-");

            try
            {

                string responseFromServer = GetURL(URL);
                
                var html = new HtmlDocument();
                html.LoadHtml(responseFromServer);
                var root = html.DocumentNode;
                // unused but a better way via linq... leaving here for now to refactor 
                var residents = root.Descendants().Where(n => n.GetAttributeValue("id", "").Equals("property_resident_listview_section"));
              
                return Ok(ParseSpokeTo(html));

            }
            catch (Exception ex)
            {
                return Ok(new GenericResult { result = "0", payload = ex.Message });

            }

        }

#region privateHelpers

        private string GetURL(string URL)
        {


            try
            {

           
                HttpWebRequest myReq =
                (HttpWebRequest)WebRequest.Create(URL);
                //begin fake useragent so external service won't deny us
                myReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                myReq.UserAgent = "Mozilla / 5.0(Macintosh; Intel Mac OS X 10.10; rv: 50.0) Gecko / 20100101 Firefox / 50.0";
                //end fake useragent so external service won't deny us

                WebResponse MyResponse = myReq.GetResponse();

                Stream dataStream = MyResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                return reader.ReadToEnd();
                

            }
            catch (Exception ex)
            {
                return  ex.Message ;

            }




        }

        private GenerticInfoHit ParseSpokeTo(HtmlDocument html)
        {
            GenerticInfoHit myResults = new GenerticInfoHit();
            int hits = 0;
            myResults.sourceName = "spoketo";


            foreach (HtmlNode node in html.DocumentNode.SelectNodes("//div[@id='property_resident_listview_section']") ?? Enumerable.Empty<HtmlNode>())
            {

                string selector = "div[@class='listview_page']";


                foreach (HtmlNode node2 in node.SelectNodes(selector) ?? Enumerable.Empty<HtmlNode>())
                {

                    foreach (HtmlNode listViewSection in node2.SelectNodes("div[@class='listview_section']") ?? Enumerable.Empty<HtmlNode>())
                    {
                        // GenerticInfoHit myGenericInfoHit = new GenerticInfoHit();
                        foreach (HtmlNode listColumnSection in listViewSection.SelectNodes("div[@class='listview_section_column']") ?? Enumerable.Empty<HtmlNode>())
                        {
                            Person myPersonHit = new Person();  // assume one person in each .. pick up last one

                            foreach (HtmlNode resSection in listColumnSection.SelectNodes("a[@class='listview_primary_title']") ?? Enumerable.Empty<HtmlNode>())
                            {
                                string Name = resSection.InnerText.Replace("*", "");
                                var nameParts = Name.Split(' ');
                                if (nameParts.Length > 0)
                                {
                                    myPersonHit.fName = nameParts[0];
                                    myPersonHit.lName = nameParts[1];
                                }

                            }
                            //look for age
                            foreach (HtmlNode ageSection in listColumnSection.SelectNodes("div[@class='listview_primary_subtitle']") ?? Enumerable.Empty<HtmlNode>())
                            {
                                myPersonHit.age = ageSection.InnerText;
                            }
                            if (myPersonHit.lName != null)
                            {
                                myPersonHit.seq = hits;
                                myResults.peopleInfo.Add(myPersonHit);
                                hits++;
                            }
                        }

                       
                    }


                }

            }
            myResults.result = hits.ToString();
            myResults.houseValue = ParseSpokeToHomeValue(html);
            return myResults;
        }

        public void Test()
        {
            string test = "<html><body><!-- testcomment --><!--react - text: 7-- > Home Value <!-- / react - text-- ><!--react - text: 8-- >< !-- / react - text-- >$1.04M < !--react - text: 12-- >< !-- / react - text-- > </body ></html >";
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(test);
            foreach ( HtmlNode bodynode in html.DocumentNode.SelectNodes("//body")){
                Regex Reggie = new Regex("\\<.*?>");
                string foobar = bodynode.InnerText;
                string bar = Reggie.Replace(foobar, " ");
                foreach (HtmlNode comment in bodynode.SelectNodes("comment()") ?? Enumerable.Empty<HtmlNode>())
                {
                    string foo = comment.InnerText;
                }
            }

        }
         private string ParseSpokeToHomeValue(HtmlDocument html)
        {
            string HomeVal = "";
            foreach (HtmlNode JewlNode in html.DocumentNode.SelectNodes("//div[@class='icon_jewel_blocks']") ?? Enumerable.Empty<HtmlNode>())
            {
                foreach (HtmlNode JewlBlock in JewlNode.SelectNodes("div[@class='icon_jewel_block']") ?? Enumerable.Empty<HtmlNode>())
                {
                    Regex Reggie = new Regex("\\<.*?>");
                    string foobar = JewlBlock.InnerText;
                    string bar = Reggie.Replace(foobar, " ");  // yup, hacky as all. Change to list of name value pairs
                    if (bar.IndexOf("Home Value") > -1)
                    {
                        HomeVal = bar;
                    }
                }
            }
                return HomeVal;
        }
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
 
  
