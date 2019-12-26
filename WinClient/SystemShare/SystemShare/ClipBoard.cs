using System.Threading;
using System.Windows.Forms;

namespace SystemShare
{
    class ClipBoard
    {
        public static string Clip;
        public static bool ClipWait = false;

        /// <summary>
        /// Updates the public stinrg 'Clip' to the current clipboard.
        /// </summary>
        public static void Get()
        {
            ClipWait = true;
            Thread newThread1 = new Thread(new ThreadStart(GetClip));
            newThread1.SetApartmentState(ApartmentState.STA);
            newThread1.Start();
        }

        /// <summary>
        /// Updates the clipboard to the current string 'Clip'.
        /// </summary>
        public static void Set(string Cl)
        {
            Clip = Cl;
            Thread newThread1 = new Thread(new ThreadStart(SetClip));
            newThread1.SetApartmentState(ApartmentState.STA);
            newThread1.Start();
        }

        /// <summary>
        /// The thread that grabs the data from the clipboard.
        /// Sets Connection.ClipWait to fals when done
        /// </summary>
        private static void GetClip()
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.Text))
            {
                Clip = (string)iData.GetData(DataFormats.Text);
            }
            else
            {
                Clip = "";
            }
            ClipWait = false;
        }

        /// <summary>
        /// The thread that sets the clipboard.
        /// Sends a 'v' keypress after wards
        /// </summary>
        private static void SetClip()
        {
            Clipboard.SetText(Clip);
            Thread.Sleep(1);
            VKeyBoard.KeyDown(86);
        }
    }
}
