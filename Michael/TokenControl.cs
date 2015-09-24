using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Michael
{
    public partial class TokenControl : UserControl
    {
        public Token token;

        public TokenControl()
        {
            InitializeComponent();
        }

        public TokenControl(Token t)
        {
            InitializeComponent();

            this.token = t;

            this.label1.Text = token.Value;


        }
    }
}
