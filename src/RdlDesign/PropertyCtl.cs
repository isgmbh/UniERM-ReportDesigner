using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using Reporting.Rdl;

namespace Reporting.RdlDesign
{
    public partial class PropertyCtl : UserControl
    {
        private DesignXmlDraw _Draw;
        private DesignCtl _DesignCtl;
        private ICollection _NameCollection = null;
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private readonly string REPORT = "*Report*";
        private readonly string GROUP = "*Group Selection*";
        private readonly string NONAME = "*Unnamed*"; 

        public PropertyCtl()
        {
            InitializeComponent();
        }

        internal void Reset()
        {
            _Draw = null;
            _DesignCtl = null;
            this.pgSelected.SelectedObject = null;
            cbReportItems.Items.Clear();
            _NameCollection = null;
        }

        internal void ResetSelection(DesignXmlDraw d, DesignCtl dc)
        {
            _Draw = d;
            _DesignCtl = dc;

            if (_Draw == null)
            {
                this.pgSelected.SelectedObject = null;
                cbReportItems.Items.Clear();

                return;
            }
            SetPropertyNames();

            if (_Draw.SelectedCount == 0)
            {
                this.pgSelected.SelectedObject = new PropertyReport(_Draw, dc);
                cbReportItems.SelectedItem = REPORT;
            }
            else if (SingleReportItemType())
            {
                XmlNode n = _Draw.SelectedList[0];
                if (_Draw.SelectedCount > 1)
                {
                    int si = cbReportItems.Items.Add(GROUP);
                    cbReportItems.SelectedIndex = si;
                }
                else
                {
                    XmlAttribute xAttr = n.Attributes["Name"];
                    if (xAttr == null)
                    {
                        int si = cbReportItems.Items.Add(NONAME);
                        cbReportItems.SelectedIndex = si;
                    }
                    else
                        cbReportItems.SelectedItem = xAttr.Value;
                }
                switch (n.Name)
                {
                    case "Textbox":
                        this.pgSelected.SelectedObject = new PropertyTextbox(_Draw, dc, _Draw.SelectedList);
                        break;
                    case "Rectangle":
                        this.pgSelected.SelectedObject = new PropertyRectangle(_Draw, dc, _Draw.SelectedList);
                        break;
                    case "Chart":
                        this.pgSelected.SelectedObject = new PropertyChart(_Draw, dc, _Draw.SelectedList);
                        break;
                    case "Image":
                        this.pgSelected.SelectedObject = new PropertyImage(_Draw, dc, _Draw.SelectedList);
                        break;
                    case "List":
                        this.pgSelected.SelectedObject = new PropertyList(_Draw, dc, _Draw.SelectedList);
                        break;
                    case "Subreport":
                        this.pgSelected.SelectedObject = new PropertySubreport(_Draw, dc, _Draw.SelectedList);
                        break;
                    case "CustomReportItem":
                    default:
                        this.pgSelected.SelectedObject = new PropertyReportItem(_Draw, dc, _Draw.SelectedList);
                        break;
                }
            }
            else
            {
                int si = cbReportItems.Items.Add(GROUP);
                cbReportItems.SelectedIndex = si;
                this.pgSelected.SelectedObject = new PropertyReportItem(_Draw, dc, _Draw.SelectedList);
            }
        }
        /// <summary>
        /// Fills out the names of the report items available in the report and all other objects with names
        /// </summary>
        private void SetPropertyNames()
        {
            if (_NameCollection != _Draw.ReportNames.ReportItemNames)
            {
                cbReportItems.Items.Clear();
                _NameCollection = _Draw.ReportNames.ReportItemNames;
            }
            else
            {   // ensure our list count is the same as the number of report items
                int count = cbReportItems.Items.Count;
                if (cbReportItems.Items.Contains(this.REPORT))
                    count--;
                if (cbReportItems.Items.Contains(this.GROUP))
                    count--;
                if (cbReportItems.Items.Contains(this.NONAME))
                    count--;
                if (count != _NameCollection.Count)
                    cbReportItems.Items.Clear();        // we need to rebuild
            }

            if (cbReportItems.Items.Count == 0)
            {
                cbReportItems.Items.Add(this.REPORT);
                foreach (object o in _NameCollection)
                {
                    cbReportItems.Items.Add(o);
                }
            }
            else
            {
                try
                {
                    cbReportItems.Items.Remove(this.GROUP);
                }
                catch { }
                try
                {
                    cbReportItems.Items.Remove(this.NONAME);
                }
                catch { }
            }
        }
        /// <summary>
        /// Returns true if all selected reportitems are of the same type
        /// </summary>
        /// <returns></returns>
        private bool SingleReportItemType()
        {
            if (_Draw.SelectedCount == 1)
                return true;
            string t = _Draw.SelectedList[0].Name;
            if (t == "CustomReportItem")        // when multiple CustomReportItem don't do group change.
                return false;
            for (int i = 1; i < _Draw.SelectedList.Count; i++)
            {
                if (t != _Draw.SelectedList[i].Name)
                    return false;
            }
            return true;
        }

        private void bClose_Click(object sender, EventArgs e)
        {
            RdlDesignerForm rd = this.Parent as RdlDesignerForm;
            if (rd == null)
                return;
            rd.ShowProperties(false);
        }

        private void cbReportItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ri_name = cbReportItems.SelectedItem as string;
            if (ri_name == GROUP || ri_name == NONAME)
                return;
            if (ri_name == REPORT)
            {
                // handle request for change to report property
                if (_Draw.SelectedCount == 0)   // we're already on report
                    return;
                _DesignCtl.SetSelection(null);
                return;
            }

            // handle request to change selected report item
            XmlNode ri_node = _Draw.ReportNames.GetRINodeFromName(ri_name);
            if (ri_node == null)
                return;
            if (_Draw.SelectedCount == 1 &&
                _Draw.SelectedList[0] == ri_node)
                return;  // we're already selected!
            _DesignCtl.SetSelection(ri_node);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Settings.Default.ShowProperties = false;
        }
    }
}
