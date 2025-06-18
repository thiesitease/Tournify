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
/// Interaktionslogik für MatchesDisplay.xaml
/// </summary>
public partial class MatchesDisplay : UserControl
{

    private List<MatchModel> m_Matches = null;

    public List<MatchModel> Matches
    {
        get { return m_Matches; }
        set
        {
            if (value != m_Matches)
            {
                m_Matches = value;
                UpdateUi();
            }
        }
    }

    public MatchesDisplay()
    {
        InitializeComponent();
    }


    private void UpdateUi()
    {
        m_Panel.Children.Clear();
        foreach (MatchModel matchModel in m_Matches)
        {
            MatchDisplay matchDisplay = new MatchDisplay();
            matchDisplay.MatchModel = matchModel;
            m_Panel.Children.Add(matchDisplay);
        }
    }

}
