using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieInfo
{
    public interface IAsyncCommand
    {
        void Execute();
    };

    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(IAsyncCommand command)
        {
            CommandName = TypeDescriptor.GetClassName(command);
         }

        public string CommandName { get; set; }
    }

    public enum CommandOrder
    {
        Last,
        First,
    }

    public sealed class CommandQueue
    {
        #region Constructor

        private CommandQueue()
        {
            m_queue = new LinkedList<IAsyncCommand>();
        }

        static CommandQueue()
        {
            s_command = new CommandQueue();
            s_shortTask = new CommandQueue();
            s_longTask = new CommandQueue();
        }

        #endregion

        #region Events

        // Delegate to signal when commands are finished executing
        public delegate void CommandEventHandler(object sender, CommandEventArgs e);
        public event CommandEventHandler CommandFinished;

        #endregion

        #region Properties

        public bool IsFinished
        {
            get
            {
                if (m_thread == null)
                    return true;
                int count = 0;
                lock (m_queue)
                {
                    count = m_queue.Count;
                }
                return (count == 0) ? true : false;
            }
        }

        public string CommandStatusText
        {
            get
            {
                if (IsFinished)
                    return "";
                int count = 0;
                lock (m_queue)
                {
                    count = m_queue.Count;
                }
                return count.ToString() + " Commands Queued";
            }
        }

        #endregion

        #region Public Functions

        public void Close()
        {
            lock (m_queue)
            {
                m_queue.Clear();
                m_thread = null;
            }
        }

        public void Execute(IAsyncCommand command, CommandOrder order = CommandOrder.Last)
        {
            lock (m_queue)
            {
                if (order == CommandOrder.Last)
                    m_queue.AddLast(command);
                else
                    m_queue.AddFirst(command);
                if (m_thread == null)
                {
                    m_thread = new Thread(new ThreadStart(ThreadRun));
                    m_thread.Start();
                }
            }
        }

        public static CommandQueue Command()
        {
            return s_command;   
        }

        public static CommandQueue ShortTask()
        {
            return s_shortTask;
        }

        public static CommandQueue LongTask()
        {
            return s_longTask;
        }

        #endregion

        #region Private Functions

        private void ThreadRun()
        {
            int idleCount = 0;
            while (m_thread != null)
            {
                IAsyncCommand command = null;
                lock (m_queue)
                {
                    if (m_queue.Count != 0)
                    {
                        command = m_queue.First();
                        m_queue.RemoveFirst();
                    }
                }
                if (command != null)
                {
                    try
                    {
                        command.Execute();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteWarning("Command exception:", ex);
                    }
                    idleCount = 0;
                    CommandFinished?.Invoke(this, new CommandEventArgs(command));
                }
                else
                {
                    // Idle for 1 second before self terminating the command thread when
                    // no commands exist.
                    Thread.Sleep(10);
                    idleCount++;
                    if (idleCount > 100)
                    {
                        lock (m_queue)
                        {
                            m_thread = null;
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Members

        private static CommandQueue s_command;
        private static CommandQueue s_shortTask;
        private static CommandQueue s_longTask;

        // Queued commands
        private Thread m_thread;
        private LinkedList<IAsyncCommand> m_queue;

        #endregion
    }

}
