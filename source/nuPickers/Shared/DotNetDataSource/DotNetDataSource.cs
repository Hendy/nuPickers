﻿namespace nuPickers.Shared.DotNetDataSource
{
    using DataSource;
    using nuPickers.Shared.Editor;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    public class DotNetDataSource : IDataSource
    {
        public string AssemblyName { get; set; }

        public string ClassName { get; set; }

        public IEnumerable<DotNetDataSourceProperty> Properties { get; set; }

        [Obsolete("[v2.0.0]")]
        public string Typeahead { get; set; }

        [DefaultValue(false)]
        public bool HandledTypeahead { get; private set; }

        public IEnumerable<EditorDataItem> GetEditorDataItems(int currentId, int parentId, string typeahead)
        {
            return this.GetEditorDataItems(currentId == 0 ? parentId : currentId); // fix from PR #110
        }

        public IEnumerable<EditorDataItem> GetEditorDataItems(int currentId, int parentId, string[] keys)
        {
            return Enumerable.Empty<EditorDataItem>();
        }

        [Obsolete("[v2.0.0]")]
        public IEnumerable<EditorDataItem> GetEditorDataItems(int contextId)
        {
            return this.GetEditorDataItems(contextId, this.Typeahead);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextId">'contextId' implies that it could be the current node id, or it could be the parent node id</param>
        /// <returns></returns>
        private IEnumerable<EditorDataItem> GetEditorDataItems(int contextId, string typeahead)
        {
            IEnumerable<EditorDataItem> editorDataItems = Enumerable.Empty<EditorDataItem>();

            object dotNetDataSource = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(Helper.GetAssembly(this.AssemblyName).FullName, this.ClassName);

            if (dotNetDataSource != null)
            {
                if (dotNetDataSource is IDotNetDataSourceTypeahead)
                {
                    ((IDotNetDataSourceTypeahead)dotNetDataSource).Typeahead = typeahead;
                    this.HandledTypeahead = true;
                }

                foreach (PropertyInfo propertyInfo in dotNetDataSource.GetType().GetProperties().Where(x => this.Properties.Select(y => y.Name).Contains(x.Name)))
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        string propertyValue = this.Properties.Where(x => x.Name == propertyInfo.Name).Single().Value;

                        if (propertyValue != null)
                        {
                            // process any tokens
                            propertyValue = propertyValue.Replace("$(ContextId)", contextId.ToString());

                            propertyInfo.SetValue(dotNetDataSource, propertyValue);
                        }
                    }
                    else
                    {
                        // TODO: log unexpected property type
                    }
                }

                editorDataItems = ((IDotNetDataSource)dotNetDataSource)
                                            .GetEditorDataItems(contextId)
                                            .Select(x => new EditorDataItem() { Key = x.Key, Label = x.Value });
            }

            return editorDataItems;
        }
    }
}