using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMCEmulator
{
	public partial class SMCEmulatorForm : Form
	{
		SerialPort port;
		System.Threading.Thread comThread;
		bool disconnectClicked = false;
		int counts = 0;

		public SMCEmulatorForm()
		{
			InitializeComponent();
		}

		private void populateComPortMenu()
		{
			COMPortDropdown.Items.AddRange(
					System.IO.Ports.SerialPort.GetPortNames()
						.OrderBy(s => s)
						.Distinct()
						.ToArray());
		}

		private void SMCEmulatorForm_Load(object sender, EventArgs e)
		{
			populateComPortMenu();
			timer1.Enabled = true;
		}

		private string GetDropdownValue()
		{
			if(COMPortDropdown.SelectedItem != null)
				return COMPortDropdown.SelectedItem.ToString();
			else
				return "";
		}

		private void SetButtonStates(bool connected)
		{
			if(connected)
			{
				connectButton.Enabled = false;
				disconnectButton.Enabled = true;
				connectButton.BackColor = Color.LightGreen;
				disconnectButton.BackColor = Color.White;
			}
			else
			{
				connectButton.Enabled = true;
				disconnectButton.Enabled = false;
				disconnectButton.BackColor = Color.Red;
				connectButton.BackColor = Color.White;
			}

		}

		private void communicate()
		{
			string portName = (string)COMPortDropdown.Invoke(new Func<string>(() => GetDropdownValue()));
			if(portName == "")
			{
				connectButton.Invoke(new Action(() => SetButtonStates(false)));
				return;
			}
			port = new SerialPort(portName, 9600);
			port.Open();
			byte[] buffer;
			string recd, msg;
			Regex setRegex = new Regex("(SET )(\\d+)");
			Match match;
			while(!disconnectClicked)
			{
				int count = port.BytesToRead;
				if(count >= 2)
				{
					buffer = new byte[count];
					var str = port.Read(buffer, 0, count);
					recd = Encoding.Default.GetString(buffer);
					Console.WriteLine(recd);
					match = setRegex.Match(recd);
					if(match.Success)
					{
						Int32.TryParse(match.Groups[2].Value, out counts);
						msg = "asdf";
					}
					else
					{
						msg = "error";
					}
					port.WriteLine(msg);
					Console.WriteLine(msg);
				}
				else
				{
					Thread.Sleep(100);
				}
			}
			disconnectClicked = false;
			port.Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			countLabel.Text = counts.ToString() + " counts";
		}

		private void connectButton_Click(object sender, EventArgs e)
		{
			SetButtonStates(true);

			comThread = new Thread(() => communicate());
			comThread.Start();
		}

		private void disconnectButton_Click(object sender, EventArgs e)
		{
			SetButtonStates(false);
			disconnectClicked = true;
		}
	}
}
