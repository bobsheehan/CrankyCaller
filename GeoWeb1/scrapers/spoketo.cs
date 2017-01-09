using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace GeoWeb1.Scrapers
{

   
    public class Spoketo : IAddressPageScraper
    {
       
        public Spoketo(CrankyAddress myAddress)
        {

            URL = "http://www.spokeo.com/" + myAddress.state + "/" + myAddress.city + "/" + myAddress.housenum + "-" + myAddress.road.Replace(" ", "-");

        }
        public string URL { get; set; }

        public GenerticInfoHit ScraperResults()
        {
            string responseFromServer = ScraperHelper.GetURL(URL);

            var html = new HtmlDocument();
            html.LoadHtml(responseFromServer);
            var root = html.DocumentNode;
            // unused but a better way via linq... leaving here for now to refactor 
            var residents = root.Descendants().Where(n => n.GetAttributeValue("id", "").Equals("property_resident_listview_section"));

            return ParseSpokeTo(html);
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
                            CrankyPerson myPersonHit = new CrankyPerson();  // assume one person in each .. pick up last one

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


    }
}