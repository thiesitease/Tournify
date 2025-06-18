using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Applications.Tournify.Clock.Code.Models;
public class TournifyModel
{
    public List<MatchModel> MatchModels { get; }

    private string m_CurrentTimeString = string.Empty;

    public string CurrentTimeString
    {
        get { return m_CurrentTimeString; }
        set
        {
            if (value != m_CurrentTimeString)
            {
                m_CurrentTimeString = value;

                if (TimeSpan.TryParse(value, out TimeSpan t))
                {
                    CurrentTime = t;
                }
            }
        }
    }


    public TimeSpan CurrentTime { get; private set; }

    public TournifyModel()
    {
        MatchModels = new List<MatchModel>();
    }

}
