//-----------------------------------------------------------------------
// <copyright file="PropertyPathHelper.cs" company="MyToolkit">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/MyToolkit/MyToolkit/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------


using System;
//using DataGrid.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

//namespace DataGrid.Utilities
namespace DataGrid
{
    /// <summary>Provides helper methods for handling property paths. </summary>
    public static class PropertyPathHelper
    {
        private static readonly DependencyProperty DummyProperty =
            DependencyProperty.RegisterAttached(
            "Dummy", typeof(Object),
            typeof(DependencyObject),
            new PropertyMetadata(null));

        public static Object Evaluate(Object container, Binding binding)
        {
            DependencyObject dummyDO = new MyDependencyObject();
            BindingOperations.SetBinding(dummyDO, DummyProperty, binding);
            return dummyDO.GetValue(DummyProperty);
        }

        public static Object Evaluate(Object container, PropertyPath propertyPath)
        {
            return Evaluate(container, new Binding { Source = container, Path = propertyPath });
        }

        public static Object Evaluate(Object container, String propertyPath)
        {
            return Evaluate(container, new PropertyPath(propertyPath));
        }
    }
}
