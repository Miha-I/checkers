using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Threading;

namespace CheckersGame
{
    public partial class Checkers : Form
    {
        CheckersBoard board;
        Game game;
        public Checkers()
        {
            InitializeComponent();
            board = new CheckersBoard(panel1);
            game = new Game(panel1, board);
        }

        private void panel1_Paint(object sender, PaintEventArgs e) => game.panel_Paint(sender, e);
        private void panel1_MouseClick(object sender, MouseEventArgs e) =>
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                game.panel_MouseClick(e);
            }).Start();

    }
}

