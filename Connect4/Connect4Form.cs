using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Connect4
{
    public partial class Connect4Form : Form
    {
        private Board board;

        public Connect4Form()
        {
            InitializeComponent();

            board = new Board(7, 6, plBoard);
        }
    }
}
