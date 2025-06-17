using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;


namespace Gemelo.Applications.Tournify.Clock.Code.Connectors;
public class TournifyConnector
{
    #region öffentliche Properties

    public Uri TournifyUri { get; }

    #endregion öffentliche Properties

    #region ctor

    public TournifyConnector(Uri tournifyUri)
    {
        TournifyUri = tournifyUri;
    }


    #endregion ctor

    #region öffentliche Methoden

    //public async Task ReadContent(string html)
    //{
    //    try
    //    {
          

    //    }
    //    catch (Exception ex)
    //    {
    //    }
    //}


    public async Task ReadContent(string html)
    {
        try
        {
            //var wc = new HttpClient();
            //string webData = await wc.GetStringAsync(TournifyUri);

            //var url = "https://example.com";  // Ersetze mit gewünschter URL

            //using var httpClient = new HttpClient();
            //var html = await httpClient.GetStringAsync(TournifyUri);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            Parse(htmlDoc.DocumentNode);

            var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");

            if (linkNodes != null)
            {
                foreach (var link in linkNodes)
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    var text = link.InnerText.Trim();

                    Console.WriteLine($"Text: {text} | Link: {href}");
                }
            }
            else
            {
                Console.WriteLine("Keine Links gefunden.");
            }

        }
        catch (Exception ex)
        {
        }
    }

    private void Parse(HtmlNode parentNode)
    {
        if (parentNode.Name == "div")
        {
            //Debugger.Break();
            foreach (var attrib in parentNode.Attributes)
            {
                Debug.WriteLine(attrib.Name);
                Debug.WriteLine(attrib.Value);

                //if (attrib.Name == "class" )
                //{
                //    Debugger.Break();
                //}


                if (attrib.Name == "class" && 
                    attrib.Value == "MatchRow Columns6")
                {
                    Debugger.Break();
                }
            }

        }

        foreach (var node in parentNode.ChildNodes)
        {
            Parse(node);
        }
    }




    #endregion öffentliche Methoden
}
