using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Gemelo.Applications.Tournify.Clock.Properties;
using Gemelo.Applications.Tournify.Clock.Code.Models;
using Gemelo.Applications.Tournify.Clock.Code.Enumerations;


namespace Gemelo.Applications.Tournify.Clock.Code.Connectors;
public class TournifyConnector
{
    public const int AnalyseMatchesMaxCount = 20;

    #region private Memmber

    private bool m_ProceedSearch;
    private int m_CountFoundResults;
    private MatchModel? m_CurrentMatchModel;

    //private TournifyModel m_InternalWorkingTournifyModel;

    #endregion private Memmber

    #region öffentliche Properties

    public TournifyModel TournifyModel { get; private set; } = new TournifyModel();

    public Uri TournifyUri { get; }

    #endregion öffentliche Properties

    #region Events


    public event EventHandler<EventArgs> ModelChanged;

    protected void OnModelChanged()
    {
        ModelChanged?.Invoke(this, new EventArgs());
    }


    #endregion Events

    #region ctor

    public TournifyConnector(Uri tournifyUri)
    {
        TournifyUri = tournifyUri;
    }


    #endregion ctor

    #region öffentliche Methoden

    public async Task ReadContent(string html)
    {
        try
        {
            TournifyModel tournifyModel = null;
            await Task.Run(() =>
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                m_ProceedSearch = true;
                m_CountFoundResults = 0;
                tournifyModel = new TournifyModel();
                FindMatchResultRow(tournifyModel, htmlDoc.DocumentNode);
            });

            TournifyModel = tournifyModel!;
            OnModelChanged();
        }
        catch (Exception ex)
        {
            Debugger.Break();
            Trace.WriteLine(ex.Message);
        }
    }

    private string? GetAttributeValueByName(HtmlNode node, string attribName)
    {
        string? result = null;
        foreach (var attrib in node.Attributes)
        {
            if (attrib.Name == attribName)
            {
                result = attrib.Value;
                break;
            }
        }
        return result;
    }

    private void FindMatchResultRow(TournifyModel model, HtmlNode parentNode)
    {
        if (m_ProceedSearch)
        {
            bool digDeeper = true;
            if (parentNode.Name == "div")
            {
                string? classValue = GetAttributeValueByName(parentNode, "class");
                if (classValue != null && classValue.Contains(Settings.Default.ClassMatchResultRow))
                {
                    AnalyseMatchResultRow(model, parentNode);
                    digDeeper = false;
                }
                else if (classValue == Settings.Default.ClassMatchCurrentTime)
                {
                    model.CurrentTimeString = parentNode.InnerText;
                }
            }

            if (digDeeper && m_ProceedSearch)
            {
                foreach (var node in parentNode.ChildNodes)
                {
                    FindMatchResultRow(model, node);
                }
            }
        }
    }


    private void AnalyseMatchResultRow(TournifyModel model, HtmlNode parentNode)
    {
        m_CurrentMatchModel = new MatchModel();

        AnalyseMatchResultRow_AllSubs(parentNode, level: 0, mode: AnalyseMatchResultRow_SearchMode.Default);

        if (m_CurrentMatchModel.IsValid)
        {
            model.MatchModels.Add(m_CurrentMatchModel);
            m_CurrentMatchModel = null;
            m_CountFoundResults++;
        }

        if (m_CountFoundResults >= AnalyseMatchesMaxCount)
        {
            m_ProceedSearch = false;
        }
    }

    private void AnalyseMatchResultRow_AllSubs(HtmlNode parentNode, int level, AnalyseMatchResultRow_SearchMode mode)
    {
        bool digDeeper = AnalyseMatchResultRow_Content(parentNode, level, mode);

        if (digDeeper)
        {
            foreach (var childNode in parentNode.ChildNodes)
            {
                AnalyseMatchResultRow_AllSubs(childNode, level + 1, mode);
            }
        }
    }

    private bool AnalyseMatchResultRow_Content(HtmlNode parentNode, int level, AnalyseMatchResultRow_SearchMode mode)
    {
        if (m_CurrentMatchModel != null)
        {
            string? classValue = GetAttributeValueByName(parentNode, "class");
            if (mode == AnalyseMatchResultRow_SearchMode.Default && !string.IsNullOrEmpty(classValue))
            {
                //if (classValue.Contains("CurrentTime"))
                //{
                //    m_CurrentMatchModel.CurrentTime = parentNode.InnerText;
                //    return false;
                //}
                if (classValue.Contains("FieldName"))
                {
                    m_CurrentMatchModel.FieldName = parentNode.InnerText;
                    return false;
                }
                else if (classValue.Contains("Team1"))
                {
                    m_CurrentMatchModel.Team1 = parentNode.InnerText;
                    return false;
                }
                else if (classValue.Contains("Team2"))
                {
                    m_CurrentMatchModel.Team2 = parentNode.InnerText;
                    return false;
                }
                else if (classValue.Contains("PouleName"))
                {
                    m_CurrentMatchModel.PouleName = parentNode.InnerText;
                    return false;
                }
                else if (classValue.Contains("MatchReferee"))
                {
                    m_CurrentMatchModel.MatchReferee = parentNode.InnerText;
                    return false;
                }
                else if (classValue.Contains("MatchColumnScore")) //  MatchColumnScore WithDate undefined MatchColumn
                {
                    AnalyseMatchResultRow_AllSubs(parentNode, 0, AnalyseMatchResultRow_SearchMode.MatchColumnScore);
                    return false;
                }
            }
            else if (mode == AnalyseMatchResultRow_SearchMode.MatchColumnScore)
            {
                if (classValue == "MatchColumnScoreDate")
                {
                    m_CurrentMatchModel.MatchColumnScoreDateString = parentNode.InnerText;
                    return false;
                }
                else if (classValue == null && level == 2)
                {
                    m_CurrentMatchModel.MatchColumnScoreTimeString = parentNode.InnerText;
                    return false;
                }
            }
        }

        return true;
    }

    //private void AnalyseMatchResultRow_MatchColumnScore(HtmlNode parentNode)
    //{
    //    if (parentNode.ChildNodes.Count > 0)
    //    {
    //        var node = parentNode.ChildNodes[0];
    //        if (node.ChildNodes.Count > 2)
    //        {
    //            MatchColumnScoreDate
    //        }
    //    }
    //}






    #endregion öffentliche Methoden
}
