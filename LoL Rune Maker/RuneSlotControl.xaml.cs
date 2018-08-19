﻿using LoL_Rune_Maker.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LoL_Rune_Maker
{
    /// <summary>
    /// Interaction logic for RuneSlotControl.xaml
    /// </summary>
    public partial class RuneSlotControl : UserControl
    {
        public double SecondarySlotsHeight
        {
            get => (double)GetValue(SecondarySlotsHeightProperty);
            set => SetValue(SecondarySlotsHeightProperty, value);
        }
        public static readonly DependencyProperty SecondarySlotsHeightProperty =
            DependencyProperty.Register("SecondarySlotsHeight", typeof(double), typeof(RuneTreeControl), new PropertyMetadata(40.0));

        public event EventHandler<int> SelectedRuneChanged;

        private int _selectedRuneID;
        public int SelectedRuneID
        {
            get => _selectedRuneID;
            set
            {
                _selectedRuneID = value;
                SelectedRuneChanged?.Invoke(this, value);

                for (int i = 0; i < Runes.Count; i++)
                {
                    Runes[i].Selected = Runes[i].Rune.ID == value;
                }
            }
        }

        private RuneSlot Slot;
        private IList<GrayscaleImageControl> Runes = new List<GrayscaleImageControl>();

        public RuneSlotControl(RuneSlot slot, bool first)
        {
            this.Slot = slot;

            InitializeComponent();

            if (first)
                this.Height = 65;
            else
                this.Height = SecondarySlotsHeight;

            int col = 0;
            foreach (var rune in slot.Runes)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var image = new GrayscaleImageControl(rune);
                image.MouseDown += Image_MouseDown;

                if (first)
                    image.Height = 65;

                MainGrid.Children.Add(image);
                Runes.Add(image);
                Grid.SetColumn(image, col++);
            }
        }
        
        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectedRuneID = ((GrayscaleImageControl)sender).Rune.ID;
        }
    }
}
