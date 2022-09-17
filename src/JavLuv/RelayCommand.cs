using System;
using System.Diagnostics;
using System.Windows.Input;

namespace JavLuv
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates. 
    /// The default return value for the CanExecute method is 'true'.
    /// </summary>
    public class RelayCommand : ICommand
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand&lt;T&gt;"/> class and the command can always be executed.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<Boolean> canExecute)
        {

            if (execute == null)
                throw new ArgumentNullException("execute");
            m_execute = execute;
            m_canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (m_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (m_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        [DebuggerStepThrough]
        public Boolean CanExecute(Object parameter)
        {
            return m_canExecute == null ? true : m_canExecute();
        }

        public void Execute(Object parameter)
        {
            m_execute();
        }

        #endregion

        #region Private Data

        readonly Func<Boolean> m_canExecute;
        readonly Action m_execute;

        #endregion

    }
}
