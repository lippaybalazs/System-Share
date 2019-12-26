using System.Threading;
using System.Windows.Forms;

namespace SystemShare
{
    class ClipBoard
    {
        public static string Clip;

        /// <summary>
        /// Fetches the clipboard
        /// </summary>
        public static void Get()
        {
            Thread ClipFetcher = new Thread(new ThreadStart(GetClip));
            ClipFetcher.SetApartmentState(ApartmentState.STA);
            ClipFetcher.Start();
        }

        /// <summary>
        /// Sets the cliboard
        /// </summary>
        public static void Set(string Cl)
        {
            Clip = Cl;
            Thread ClipSetter = new Thread(new ThreadStart(SetClip));
            ClipSetter.SetApartmentState(ApartmentState.STA);
            ClipSetter.Start();
        }

        /// <summary>
        /// Gets the clipboard
        /// </summary>
        private static void GetClip()
        {
            IDataObject iData = Clipboard.GetDataObject();

            if (iData.GetDataPresent(DataFormats.Text))
            {
                Clip = (string)iData.GetData(DataFormats.Text);
            }
            Connection.Clip = Clip;
        }

        /// <summary>
        /// Sets the clipboard
        /// </summary>
        private static void SetClip()
        {
            Clipboard.SetText(Clip);
        }
    }
}
