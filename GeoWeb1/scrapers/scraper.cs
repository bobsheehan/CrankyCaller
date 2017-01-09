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

namespace GeoWeb1.Scrapers
{

    public interface IAddressPageScraper
    {

        string URL { get; set; }
        GenerticInfoHit ScraperResults();
    }

   public class ScraperFactory {
        public static T Create<T>(CrankyAddress myAddress)

        {

            if (typeof(T) == typeof(Spoketo))

            {
                return (T)(IAddressPageScraper)new Spoketo(myAddress);
            }
            else
            {
                throw new NotImplementedException(String.Format("Creation of {0} interface is not supported yet.", typeof(T)));


            }
        }

    }

    public class CrankyAddress {
        public int housenum { get; set; }
        public string road { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
    };

    public class CrankyPerson
    {
        private string _fname = null;
        private string _lname = null;
        private string _age = null;
        public int seq = 0;
        public string fName { get { return _fname; } set { _fname = value; } }
        public string lName { get { return _lname; } set { _lname = value; } }
        public string age { get { return _age; } set { _age = value; } }
        public string phoneNumber { get; set; }
    }

    public class WhitePageHit
    {
        private string _name = null;
        private string _address = null;
        private string _phonenumber = null;


        public string phoneNumber { get { return _phonenumber; } set { _phonenumber = value; } }
        public string address { get { return _address; } set { _address = value; } }
        public string name { get { return _name; } set { _name = value; } }
    }

   


    public class GenerticInfoHit
    {
        private List<CrankyPerson> _MyPeopleInfo = new List<CrankyPerson>();

        public string result { get; set; }

        public string sourceName { get; set; }
        public string houseValue { get; set; }
        public List<CrankyPerson> peopleInfo { get { return _MyPeopleInfo; } set { _MyPeopleInfo = value; } }
    }


    public class PhoneLookupResult
    {
        public string result { get; set; }
        public List<WhitePageHit> phoneList { get; set; }

    }

    public class GenericResult
    {
        public string result { get; set; }
        public string payload { get; set; }
    }



    public class ScraperHelper
    {
        public static string GetURL(string URL)
        {


            try
            {


                HttpWebRequest myReq =
                (HttpWebRequest)WebRequest.Create(URL);


                //begin fake useragent so external service won't deny us
                myReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                myReq.UserAgent = "Mozilla / 5.0(Macintosh; Intel Mac OS X 10.10; rv: 50.0) Gecko / 20100101 Firefox / 50.0";
                myReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                //end fake useragent so external service won't deny us

                using (WebResponse MyResponse = myReq.GetResponse())
                {
                    using (Stream dataStream = MyResponse.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        return reader.ReadToEnd();
                    }
                }

            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        } // end get URL



    }
}
