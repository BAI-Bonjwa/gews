﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LibCommonControl
{
    public partial class BaseForm : Form
    {
        private MainFrm mainForm;

        public BaseForm()
        {
            InitializeComponent();
        }

        public MainFrm MainForm
        {
            get
            {
                return this.mainForm;
            }

            set { this.mainForm = value; }
        }
    }
}
