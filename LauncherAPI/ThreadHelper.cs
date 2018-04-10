using System.Windows.Forms;

namespace LauncherAPI
{
    public static class ThreadHelper
    {
        private delegate void SetTextCallback<T>(Form f, T ctrl, string property, object obj) where T : Control;

        /// <summary>
        /// Set text property of various controls
        /// </summary>
        /// <param name="form">The calling form</param>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        public static void SetValue<T>(this Form form, T ctrl, string property, object obj) where T : Control
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (ctrl.InvokeRequired)
            {
                SetTextCallback<T> d = new SetTextCallback<T>(SetValue);
                form.Invoke(d, new object[] { form, ctrl, property, obj });
            }
            else
            {
                ctrl.GetType().GetProperty(property).SetValue(ctrl, obj);
            }
        }
    }
}