//-----------------------------------------------------------------------
// <copyright file="DataGridColumnCollection.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------



using System.Collections.ObjectModel;

namespace DataGrid
{
    /// <summary>A typed observable collection of <see cref="DataGridColumnBase"/> items. </summary>
    public class DataGridColumnCollection : ObservableCollection<DataGridColumnBase>
    {
    }
}

