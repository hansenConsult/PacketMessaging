﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class ListViewParametersArray {
    
    private ListViewParametersArrayListViewParameters2[] listViewParameters2Field;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ListViewParameters2")]
    public ListViewParametersArrayListViewParameters2[] ListViewParameters2 {
        get {
            return this.listViewParameters2Field;
        }
        set {
            this.listViewParameters2Field = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ListViewParametersArrayListViewParameters2 {
    
    private ListViewParametersArrayListViewParameters2ListViewColumns listViewColumnsField;
    
    private string listViewField;
    
    /// <remarks/>
    public ListViewParametersArrayListViewParameters2ListViewColumns ListViewColumns {
        get {
            return this.listViewColumnsField;
        }
        set {
            this.listViewColumnsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ListView {
        get {
            return this.listViewField;
        }
        set {
            this.listViewField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ListViewParametersArrayListViewParameters2ListViewColumns {
    
    private ListViewParametersArrayListViewParameters2ListViewColumnsColumnDescriptionBase[] columnDescriptionBaseField;
    
    private bool listViewHeaderCreatedField;
    
    private string sortOrderField;
    
    private string sortPropertyNameField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ColumnDescriptionBase")]
    public ListViewParametersArrayListViewParameters2ListViewColumnsColumnDescriptionBase[] ColumnDescriptionBase {
        get {
            return this.columnDescriptionBaseField;
        }
        set {
            this.columnDescriptionBaseField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ListViewHeaderCreated {
        get {
            return this.listViewHeaderCreatedField;
        }
        set {
            this.listViewHeaderCreatedField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string SortOrder {
        get {
            return this.sortOrderField;
        }
        set {
            this.sortOrderField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string SortPropertyName {
        get {
            return this.sortPropertyNameField;
        }
        set {
            this.sortPropertyNameField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ListViewParametersArrayListViewParameters2ListViewColumnsColumnDescriptionBase {
    
    private string propertyNameField;
    
    private string headerField;
    
    private string headershortField;
    
    private string widthField;
    
    private decimal minWidthField;
    
    private string widthAsStringField;
    
    private string valueField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string PropertyName {
        get {
            return this.propertyNameField;
        }
        set {
            this.propertyNameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Header {
        get {
            return this.headerField;
        }
        set {
            this.headerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Headershort {
        get {
            return this.headershortField;
        }
        set {
            this.headershortField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Width {
        get {
            return this.widthField;
        }
        set {
            this.widthField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal MinWidth {
        get {
            return this.minWidthField;
        }
        set {
            this.minWidthField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string WidthAsString {
        get {
            return this.widthAsStringField;
        }
        set {
            this.widthAsStringField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value {
        get {
            return this.valueField;
        }
        set {
            this.valueField = value;
        }
    }
}
