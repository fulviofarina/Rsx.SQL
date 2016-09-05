using System;
using System.Windows.Forms;

namespace Rsx
{
    public class Tim : Timer
    {
        public Tim(int seg, Action toDo)
        {
            this.Tick += new EventHandler(Tim_Tick);
            this.Interval = seg * 1000;
        }

        private void Tim_Tick(object sender, EventArgs e)
        {
            if (afterTick == null) return;
            afterTick.Invoke();
            afterTick = null;
            this.Dispose();
        }

        private Action afterTick;

        public Action AfterTick
        {
            get { return afterTick; }
            set { afterTick = value; }
        }
    }

    /// <summary>
    /// A Worker made for multiple methods
    /// </summary>
    ///
}