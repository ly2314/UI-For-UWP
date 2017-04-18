﻿using System;
using System.Collections.Generic;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Data.DataForm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;


namespace Telerik.UI.Xaml.Controls.Data
{
    public class EditorFactory
    {
        public bool RestrictEditableControls { get; internal set; }

        public EditorFactory()
        {
            this.RegisterDefaultEditors();
            this.RegisterDefaultViews();
        }

        internal Dictionary<Type, Type> defaultEditorTypeRegistry = new Dictionary<Type, Type>();
        internal Dictionary<Type, Type> defaultViewTypeRegistry = new Dictionary<Type, Type>();
        internal Dictionary<Type, Type> editorTypeRegistry = new Dictionary<Type, Type>();
        internal Dictionary<string, Type> editorPropertyRegistry = new Dictionary<string, Type>();
        internal Dictionary<Type, Type> viewTypeRegistry = new Dictionary<Type, Type>();
        internal Dictionary<string, Type> viewPropertyRegistry = new Dictionary<string, Type>();

        internal void RegisterDefaultEditors()
        {
            this.defaultEditorTypeRegistry.Add(typeof(bool), typeof(BooleanEditor));
            this.defaultEditorTypeRegistry.Add(typeof(string), typeof(StringEditor));
            this.defaultEditorTypeRegistry.Add(typeof(DateTime), typeof(DateEditor));
            this.defaultEditorTypeRegistry.Add(typeof(DateTime?), typeof(DateEditor));
            this.defaultEditorTypeRegistry.Add(typeof(double), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(double?), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(float), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(float?), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(int), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(int?), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(long), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(long?), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(decimal), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(decimal?), typeof(NumericEditor));
            this.defaultEditorTypeRegistry.Add(typeof(Enum), typeof(EnumEditor));
        }

        internal void RegisterDefaultViews()
        {
        }

        internal void RegisterTypeEditor(Type propertyType, Type editorType)
        {
            this.editorTypeRegistry[propertyType] = editorType;
        }

        internal void RegisterPropertyEditor(string propertyName, Type editorType)
        {
            this.editorPropertyRegistry[propertyName] = editorType;
        }

        internal void RegisterTypeView(Type propertyType, Type viewType)
        {
            this.viewTypeRegistry[propertyType] = viewType;
        }

        internal void RegisterPropertyView(string propertyName, Type viewType)
        {
            this.viewPropertyRegistry[propertyName] = viewType;
        }

        internal void UnRegisterTypeEditor(Type propertyType)
        {
            if (propertyType != null)
            {
                if (this.editorTypeRegistry.ContainsKey(propertyType))
                {
                    this.editorTypeRegistry.Remove(propertyType);
                }
            }
            else
            {
                this.editorTypeRegistry.Clear();
            }
        }

        internal void UnRegisterPropertyEditor(string propertyName)
        {
            if (propertyName != null)
            {
                if (this.editorPropertyRegistry.ContainsKey(propertyName))
                {
                    this.editorPropertyRegistry.Remove(propertyName);
                }
            }
            else
            {
                this.editorPropertyRegistry.Clear();
            }
        }

        internal void UnRegisterTypeView(Type propertyType)
        {
            if (propertyType != null)
            {
                if (this.viewTypeRegistry.ContainsKey(propertyType))
                {
                    this.viewTypeRegistry.Remove(propertyType);
                }
            }
            else
            {
                this.viewTypeRegistry.Clear();
            }
        }

        internal void UnRegisterPropertyView(string propertyName)
        {
            if (propertyName != null)
            {
                if (this.viewPropertyRegistry.ContainsKey(propertyName))
                {
                    this.viewPropertyRegistry.Remove(propertyName);
                }
            }
            else
            {
                this.viewPropertyRegistry.Clear();
            }
        }

        public EntityPropertyControl CreateEditor(EntityProperty property)
        {
            EntityPropertyControl editor = this.GenerateContainerForEntityProperty(property);
            var view = this.GenerateContainerForEditor(property);
            if (view != null)
            {
                editor.View = view;
                if ((view as ITypeEditor) != null)
                {
                    (view as ITypeEditor).BindEditor();
                }
            }
            else
            {
                return null;
            }

            var label = this.GenerateContainerForLabel(property);
            editor.Label = label;

            var errorView = this.GenerateContainerForErrors(property);
            editor.ErrorView = errorView;

            var positiveMessageView = this.GenerateContainerForPositiveMessage(property);
            editor.PositiveMessageView = positiveMessageView;

            if (property.Label != null)
            {
                AutomationProperties.SetName(editor.View, property.Label);
            }
            
            return editor;
        }

        protected virtual FrameworkElement GenerateContainerForEditor(EntityProperty property)
        {
            Type editorType;
            if (property.IsReadOnly || this.RestrictEditableControls)
            {
                editorType = this.GetViewRegistry(property);
                if (editorType == null)
                {
                    editorType = this.GetEditorRegistry(property);
                }
            }
            else
            {
                editorType = this.GetEditorRegistry(property);
            }

            if (editorType != null)
            {
                var container = this.CreateContainerInstance(editorType);

                return container;
            }

            return null;
        }

        protected virtual FrameworkElement GenerateContainerForLabel(EntityProperty property)
        {
            if (property.Label != null)
            {
                return new LabelControl();
            }

            return null;
        }

        protected virtual FrameworkElement GenerateContainerForErrors(EntityProperty property)
        {
            return new ErrorsControl();
        }

        protected virtual FrameworkElement GenerateContainerForPositiveMessage(EntityProperty property)
        {
            return new PositiveMessageControl();
        }

        protected virtual EntityPropertyControl GenerateContainerForEntityProperty(EntityProperty property)
        {
            return new EntityPropertyControl();
        }

        private Type GetEditorRegistry(EntityProperty property)
        {
            var propertyType = property.PropertyType;
            Type editorType = null;
            if (!this.editorPropertyRegistry.TryGetValue(property.PropertyName, out editorType))
            {
                if (!this.editorTypeRegistry.TryGetValue(propertyType, out editorType))
                {
                    if (property.PropertyValue is Enum)
                    {
                        this.editorTypeRegistry.TryGetValue(typeof(Enum), out editorType);
                    }

                    if (editorType == null && !this.defaultEditorTypeRegistry.TryGetValue(propertyType, out editorType))
                    {
                        if (property.PropertyValue is Enum)
                        {
                            this.defaultEditorTypeRegistry.TryGetValue(typeof(Enum), out editorType);
                        }
                    }
                }
            }
            
            return editorType;
        }

        private Type GetViewRegistry(EntityProperty property)
        {
            var propertyType = property.PropertyType;
            Type editorType = null;
            if (!this.viewPropertyRegistry.TryGetValue(property.PropertyName, out editorType))
            {
                if (!this.viewTypeRegistry.TryGetValue(propertyType, out editorType))
                {
                    if (property.PropertyValue is Enum)
                    {
                        this.viewTypeRegistry.TryGetValue(typeof(Enum), out editorType);
                    }

                    if (editorType == null && !this.defaultViewTypeRegistry.TryGetValue(propertyType, out editorType))
                    {
                        if (property.PropertyValue is Enum)
                        {
                            this.defaultViewTypeRegistry.TryGetValue(typeof(Enum), out editorType);
                        }
                    }
                }
            }
            return editorType;
        }

        private FrameworkElement CreateContainerInstance(Type type)
        {
            return Activator.CreateInstance(type) as FrameworkElement;
        }
    }
}
