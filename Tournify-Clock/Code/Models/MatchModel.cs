using Gemelo.Applications.Tournify.Clock.Apps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Tournify.Clock.Code.Models;
public class MatchModel
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Team1) && !string.IsNullOrWhiteSpace(Team2) && !string.IsNullOrWhiteSpace(FieldName);
    public string Team1 { get; internal set; }
    public string Team2 { get; internal set; }
    public string PouleName { get; internal set; }
    public string MatchReferee { get; internal set; }
    public string FieldName { get; internal set; }

    public DateTime StartTime => MatchColumnScoreDate + MatchColumnScoreTime;
    public DateTime EndTime => StartTime + App.Current.MatchDuration;

    public DateTime MatchColumnScoreDate { get; private set; } = App.Current.Now.Date;

    private string m_MatchColumnScoreDateString;
    public string MatchColumnScoreDateString
    {
        get { return m_MatchColumnScoreDateString; }
        set
        {
            if (value != m_MatchColumnScoreDateString)
            {
                m_MatchColumnScoreDateString = value;
                if (DateTime.TryParse(value, out DateTime t))
                {
                    MatchColumnScoreDate = t;
                }
            }
        }
    }

    public TimeSpan MatchColumnScoreTime { get; private set; }

    private string m_MatchColumnScoreTimeString;
    public string MatchColumnScoreTimeString
    {
        get { return m_MatchColumnScoreTimeString; }
        set
        {
            if (value != m_MatchColumnScoreTimeString)
            {
                m_MatchColumnScoreTimeString = value;
                if (TimeSpan.TryParse(value, out TimeSpan t))
                {
                    MatchColumnScoreTime = t;
                }
            }
        }
    }




}
