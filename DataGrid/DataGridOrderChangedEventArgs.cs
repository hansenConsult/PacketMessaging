//-----------------------------------------------------------------------
// <copyright file="DataGridOrderChangedEventArgs.cs" company="MyToolkit">
//     Copyright (c) Bram Stoeller. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Bram Stoeller, bram@stoeller.nl</author>
//-----------------------------------------------------------------------



using DataGrid;
using System;

namespace DataGrid
{
    public class DataGridOrderChangedEventArgs : EventArgs
    {
        internal DataGridOrderChangedEventArgs(DataGridColumnBase column)
        {
            Column = column;
        }

        /// <summary>Gets ordering column. </summary>
        public DataGridColumnBase Column { private set; get; }

    }
}

