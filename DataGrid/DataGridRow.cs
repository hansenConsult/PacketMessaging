﻿//-----------------------------------------------------------------------
// <copyright file="DataGridRow.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
//using MyToolkit.UI;

namespace DataGrid
{
    /// <summary>A <see cref="DataGrid"/> row. </summary>
    public class DataGridRow : Grid
    {
        private ContentControl _detailsControl = null;
        private bool _isSelected = false;

        internal DataGridRow(DataGrid dataGrid, object item)
        {
            Item = item;
            DataGrid = dataGrid;

            var hasStar = CreateCells(item);
            CreateColumnAndRowDefinitions(hasStar);

            Loaded += OnLoaded;
        }

        /// <summary>Gets the parent <see cref="DataGrid"/>. </summary>
        public DataGrid DataGrid { get; private set; }

        /// <summary>Gets or sets the associated item. </summary>
        public object Item { get; private set; }

        /// <summary>Gets a value indicating whether the row is selected. </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            internal set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    UpdateRowBackgroundBrush();
                    UpdateItemDetails();
                }
            }
        }

        /// <summary>Gets the list of <see cref="DataGridCellBase"/>. </summary>
        public IList<DataGridCellBase> Cells
        {
            get
            {
                return Children
                    .OfType<FrameworkElement>()
                    .Where(c => c.Tag is DataGridCellBase)
                    .Select(c => (DataGridCellBase)c.Tag)
                    .ToList();
            }
        }

        private bool CreateCells(object item)
        {
            var columnIndex = 0;
            var hasStar = false;
            foreach (var column in DataGrid.Columns)
            {
                var cell = column.CreateCell(DataGrid, item);
                cell.Control.Tag = cell;
                cell.Control.DataContext = item; 

                var cellControl = new ContentPresenter();
                cellControl.Content = cell;
                cellControl.ContentTemplate = DataGrid.CellTemplate;

                SetColumn(cellControl, columnIndex++);
                Children.Add(cellControl);

                var columnDefinition = column.CreateGridColumnDefinition();
                hasStar = hasStar || columnDefinition.Width.IsStar;

                ColumnDefinitions.Add(columnDefinition);
            }
            return hasStar;
        }

        private void CreateColumnAndRowDefinitions(bool hasStar)
        {
            if (!hasStar)
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // second row used for details view
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (DataGrid.SelectionMode == SelectionMode.Single)
            {
                if (DataGrid.SelectedItem != null)
                    IsSelected = DataGrid.SelectedItem.Equals(Item);
            }
            else
            {
                if (DataGrid.SelectedItems != null)
                    IsSelected = DataGrid.SelectedItems.Contains(Item);
            }

            UpdateRowBackgroundBrush();
        }

        private void UpdateRowBackgroundBrush()
        {
            // TODO: Update row backgrounds when DataGrid.Items changed
            var listBoxItem = this.GetVisualParentOfType<ListBoxItem>();
            if (listBoxItem != null)
            {
                if (IsSelected)
                    listBoxItem.Background = null;
                else
                {
                    listBoxItem.Background = DataGrid.Items.IndexOf(Item) % 2 == 0 ? 
                        DataGrid.RowBackgroundEvenBrush : DataGrid.RowBackgroundOddBrush;
                }
            }
        }

        private void UpdateItemDetails()
        {
            if (DataGrid.ItemDetailsTemplate != null)
            {
                if (!_isSelected)
                {
                    if (_detailsControl != null)
                    {
                        Children.Remove(_detailsControl);
                        _detailsControl = null;
                    }
                }
                else if (_isSelected)
                {
                    if (_detailsControl == null)
                    {
                        _detailsControl = new ContentControl();
                        _detailsControl.FontSize = DataGrid.FontSize; 
                        _detailsControl.Content = Item;
                        _detailsControl.ContentTemplate = DataGrid.ItemDetailsTemplate;
                        _detailsControl.VerticalContentAlignment = VerticalAlignment.Stretch;
                        _detailsControl.HorizontalContentAlignment = HorizontalAlignment.Stretch;

                        Children.Add(_detailsControl);
                        SetRow(_detailsControl, 1);
                        SetColumnSpan(_detailsControl, ColumnDefinitions.Count);
                    }
                    else
                        _detailsControl.ContentTemplate = DataGrid.ItemDetailsTemplate;
                }
            }

            foreach (var cell in Cells)
                cell.OnSelectedChanged(_isSelected);
        }
    }
}

