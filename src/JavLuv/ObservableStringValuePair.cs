namespace JavLuv
{
    public class ObservableStringValuePair<T> : ObservableObject
    {
        #region Constructor

        public ObservableStringValuePair(string key, T val)
        {
            m_key = key;
            Value = val;
        }

        #endregion

        #region Properties

        public string Name { get { return TextManager.GetString(m_key); } }

        public T Value { get; private set; }

        #endregion

        #region Public Functions

        public void Notify()
        {
            NotifyAllPropertiesChanged();
        }

        #endregion

        #region Private Members

        private string m_key;

        #endregion
    }
}
