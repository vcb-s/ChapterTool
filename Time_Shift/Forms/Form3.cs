﻿using System;
using System.Drawing;
using ChapterTool.Util;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ChapterTool.Forms
{

    public partial class Form3 : Form
    {
        private readonly Form1 _mainWindow;
        private readonly List<Color> _currentSetting;
        public Form3(Form1 mainWindow)
        {
            InitializeComponent();
            MaximizeBox     = false;
            _mainWindow     = mainWindow;
            _currentSetting = mainWindow.CurrentColor;
            SetDefault();
        }

        private void SetDefault()
        {
            back.BackColor      = _currentSetting[0];
            textBack.BackColor  = _currentSetting[1];
            overBack.BackColor  = _currentSetting[2];
            downBack.BackColor  = _currentSetting[3];
            bordBack.BackColor  = _currentSetting[4];
            textFront.BackColor = _currentSetting[5];
        }

        private void back_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.BackChange = back.BackColor = colorDialog1.Color;
            }
        }
        private void textBack_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.TextBack = textBack.BackColor = colorDialog1.Color;
            }
        }
        private void overBack_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.MouseOverColor = overBack.BackColor = colorDialog1.Color;
            }
        }
        private void downBack_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.MouseDownColor = downBack.BackColor = colorDialog1.Color;
            }
        }
        private void bordBack_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.BordBackColor = bordBack.BackColor = colorDialog1.Color;
            }
        }
        private void textFront_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.TextFrontColor = textFront.BackColor = colorDialog1.Color;
            }
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConvertMethod.SaveColor(_mainWindow.CurrentColor);
            e.Cancel = true;
            Hide();
        }
    }
}
