using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Globalization;

namespace OppolzerTerms1986
{
    public partial class OppolzerTermsProperties : Form
    {
        public OppoltzerTerms.OppolzerParameter OppolzerParameter {get; private set;}

        public OppolzerTermsProperties(OppoltzerTerms.OppolzerParameter OppolzerParameter)
        {
            this.OppolzerParameter = OppolzerParameter;
            InitializeComponent();

            userControl_OppolzerTermsProperties1.OppolzerParameter = OppolzerParameter;
            userControl_OppolzerTermsProperties1.ModelChanged += new UserControl_OppolzerTermsProperties.ModelChangedHandler(userControl_OppolzerTermsProperties1_ModelChanged);
        }
                                
        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void userControl_OppolzerTermsProperties1_ModelChanged(object sender, UserControl_OppolzerTermsProperties.ModelChangedEventArgs e)
        {
            if (userControl_OppolzerTermsProperties1.IsValid)
            {
                OppolzerParameter = userControl_OppolzerTermsProperties1.OppolzerParameter;
                buttonOK.Enabled = true;
            }
            else
            {
                buttonOK.Enabled = true;
            }
        }
    }
}
