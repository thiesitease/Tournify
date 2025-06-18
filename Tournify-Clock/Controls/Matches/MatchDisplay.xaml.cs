using Gemelo.Applications.Tournify.Clock.Code.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gemelo.Applications.Tournify.Clock.Controls.Matches;
/// <summary>
/// Interaktionslogik für MatchDisplay.xaml
/// </summary>
public partial class MatchDisplay : UserControl
{
    #region Properties



    private MatchModel m_MatchModel = null;

    public MatchModel MatchModel
    {
        get { return m_MatchModel; }
        set
        {
            if (value != m_MatchModel)
            {
                m_MatchModel = value;
                UpdateUi();
            }
        }
    }

 

    #endregion Properties

    public MatchDisplay()
    {
        InitializeComponent();
    }

    #region private Methoden

    private void UpdateUi()
    {
        m_TxtField.Text = $"{MatchModel?.FieldName}";
        m_TxtPool.Text = $"{MatchModel?.PouleName.Replace("Gruppe","")}";
        m_TxtReferee.Text = $"{MatchModel?.MatchReferee?.Replace("&amp;","&")}";
        m_TxtTeam1.Text = $"{MatchModel?.Team1}";
        m_TxtTeam2.Text = $"{MatchModel?.Team2}";
        m_TxtTime.Text = $"{MatchModel?.MatchColumnScoreTime.Hours:00}:{MatchModel?.MatchColumnScoreTime.Minutes:00}";
        m_TxtDay.Text = $"{MatchModel?.MatchColumnScoreDate.DayOfWeek.ToString().Substring(0,2).ToUpper()}";
    }


    #endregion private Methoden
}
