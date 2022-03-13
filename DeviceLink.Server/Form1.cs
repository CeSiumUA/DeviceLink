using System.ComponentModel;

namespace DeviceLink.Server
{
    public partial class Form1 : Form
    {
        private readonly CommunicationClient _communicationClient;
        public Form1()
        {
            InitializeComponent();
            _communicationClient = new CommunicationClient();
        }

        private void startListenerButton_Click(object sender, EventArgs e)
        {
            _communicationClient.StartListener();
        }

        private void stopListenerButton_Click(object sender, EventArgs e)
        {
            _communicationClient.StopListener();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _communicationClient.Dispose();
            base.OnClosing(e);
        }
    }
}