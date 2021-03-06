﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS17010Test {
  public partial class Form1 : Form {
    public Form1() {
      InitializeComponent();
    }
    private IPAddress LocalIPAddress() {
      if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
        return null;
      }

      IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

      return host
          .AddressList
          .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    private void button1_Click(object sender, EventArgs e) {
      button1.Enabled = false;
      button2.Enabled = false;
      progressBar1.Visible = true;
      resultLabel.Text = strings.pleaseWait;
      resultLabel.ForeColor = Color.AliceBlue;
      testIpBox.Text = LocalIPAddress().ToString();
      ThreadPool.QueueUserWorkItem((t) => {
        try {
          var res = Tester.TestIP(LocalIPAddress().ToString());
          UpdateWithResults(res);
        } catch (Exception ex) {
          ResultError(ex);
        }
      });
    }

    private void UpdateWithResults(TestResult res) {
      if (testIpBox.InvokeRequired) {
        var self = new Action<TestResult>(UpdateWithResults);
        BeginInvoke(self, res);
      } else {
        var msg = string.Format(strings.resultString, res.OSName, res.OSBuild, res.Workgroup, res.IsVulnerable ? strings.yes : strings.no, res.error);
        resultLabel.Text = msg;
        if (res.hadError) {
          resultLabel.ForeColor = Color.PaleVioletRed;
        } else if (res.IsVulnerable) {
          resultLabel.ForeColor = Color.Red;
        } else {
          resultLabel.ForeColor = Color.Green;
        }
        button1.Enabled = true;
        button2.Enabled = true;
        progressBar1.Visible = false;
        if (res.hadError) {
          MessageBox.Show(strings.executionError);
        }
      }
    }

    private void ResultError(Exception e) {
      if (testIpBox.InvokeRequired) {
        var self = new Action<Exception>(ResultError);
        BeginInvoke(self, e);
      } else {
        var msg = string.Format(strings.resultErrorMessage, e.Message);
        MessageBox.Show(msg);
        button1.Enabled = true;
        button2.Enabled = true;
        progressBar1.Visible = false;
      }
    }

    private void button2_Click(object sender, EventArgs e) {
      button1.Enabled = false;
      button2.Enabled = false;
      progressBar1.Visible = true;
      resultLabel.Text = strings.pleaseWait;
      resultLabel.ForeColor = Color.AliceBlue;
      ThreadPool.QueueUserWorkItem((t) => {
        try {
          var res = Tester.TestIP(testIpBox.Text);
          UpdateWithResults(res);
        } catch (Exception ex) {
          ResultError(ex);
        }
      });
    }
  }
}
