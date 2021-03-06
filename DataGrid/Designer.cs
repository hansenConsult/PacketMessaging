﻿//-----------------------------------------------------------------------
// <copyright file="Designer.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.ComponentModel;

using Windows.UI.Xaml;

//UI
namespace DataGrid
{
    internal class MyDependencyObject : DependencyObject { }

    public static class Designer
    {
        public static bool IsInDesignMode
        {
            get { return Windows.ApplicationModel.DesignMode.DesignModeEnabled; }
        }
    }
}
