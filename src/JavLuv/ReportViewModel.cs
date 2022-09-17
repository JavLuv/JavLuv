using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JavLuv
{
    public class ReportViewModel : ObservableObject
    {
        #region Constructors

        public ReportViewModel(MainWindowViewModel parent)
        {
            m_parent = parent;
        }

        #endregion

        #region Properties

        public MainWindowViewModel Parent { get { return m_parent; } }

        public string ErrorLog
        {
            get { return m_errorLog; }
            set
            {
                if (value != m_errorLog)
                {
                    m_errorLog = value;
                    NotifyPropertyChanged("ErrorLog");
                }
            }
        }

        #endregion

        #region Private Members

        private MainWindowViewModel m_parent;
        private string m_errorLog = String.Empty;

        #endregion
    }
}
