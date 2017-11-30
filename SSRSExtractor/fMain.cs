using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web.Services.Protocols;
using System.Xml;
using System.Net;
using System.IO;

namespace SSRSExtractor
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            GetLocations();

            cmbLocations.SelectedIndex = 0;
        }

        private void GetLocations()
        {
            RSWebReference_SSRS.ReportingService2010 rs = new RSWebReference_SSRS.ReportingService2010();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            try
            {
                cmbLocations.Items.Add("/");

                RSWebReference_SSRS.CatalogItem[] items = rs.ListChildren(@"/", false);
                foreach (RSWebReference_SSRS.CatalogItem item in items)
                {
                    if (item.TypeName == "Folder")
                        cmbLocations.Items.Add(item.Name);
                }
            }
            catch (SoapException)
            {
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            lbStatus.Text = "Processing...";
            lbStatus.Visible = true;

            Application.DoEvents();

            string sFrom = cmbLocations.Text;
            //string sOutputDir = string.Format(@"{0}{1}\", txtOutput.Text, sFrom);
            string sOutputDir = string.Format(@"{0}", txtOutput.Text);
            string sOutFile = "";

            if (!Directory.Exists(sOutputDir))
                Directory.CreateDirectory(sOutputDir);

            RSWebReference_SSRS.ReportingService2010 rs = new RSWebReference_SSRS.ReportingService2010();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            try
            {
                //RSWebReference_SSRS.CatalogItem[] items = rs.ListChildren(string.Format(@"/{0}", sFrom), true);
                RSWebReference_SSRS.CatalogItem[] items = rs.ListChildren(@"/", true);

                progBar.Maximum = items.Count();
                progBar.Value = 0;

                foreach (RSWebReference_SSRS.CatalogItem item in items)
                {
                    if (item.TypeName == "Report" ) {
                        byte[] rpt_def = null;
                        XmlDocument doc = new XmlDocument();

                        rpt_def = rs.GetItemDefinition(item.Path);
                        MemoryStream stream = new MemoryStream(rpt_def);

                        sOutFile = string.Format(@"{0}{1}.rdl", sOutputDir, item.Path.Replace("/","\\"));

                        if (File.Exists(sOutFile))
                            File.Delete(sOutFile);

                        doc.Load(stream);
                        doc.Save(sOutFile);
                    }
                    else if(item.TypeName == "Folder")
                    {
                        var path = string.Format(@"{0}{1}", sOutputDir, item.Path.Replace("/", "\\"));
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                    }

                    progBar.Value += 1;
                }
            }
            catch (SoapException)
            {
            }

            lbStatus.Text = "Complete";
            progBar.Value = progBar.Maximum;
        }
    }
}


