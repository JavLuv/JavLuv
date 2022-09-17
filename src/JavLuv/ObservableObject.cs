using System;
using System.ComponentModel;
using System.Diagnostics;

namespace JavLuv
{

    public class ObservableObject : INotifyPropertyChanged
    {

        public PropertyChangedEventHandler PropertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this.PropertyChanged += value; 
                m_propertyCount++;
                if (m_propertyCount > 0)
                    OnShow();

            }
            remove
            {
                this.PropertyChanged -= value;
                m_propertyCount--;
                if (m_propertyCount <= 0)
                    OnHide();
            }
        }

        protected void NotifyAllPropertiesChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                VerifyPropertyName(propertyName);
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void ForwardPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, args);
            }
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real 
            // public instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;
                Debug.Fail(msg);
            }
        }

        private int m_propertyCount = 0;

    }

}
