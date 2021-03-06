﻿namespace nuPickers.Shared.SqlDataSource
{
    using DataSource;
    using nuPickers.Shared.Editor;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Umbraco.Core.Persistence;

    public class SqlDataSource : IDataSource
    {
        public string SqlExpression { get; set; }

        public string ConnectionString { get; set; }

        [Obsolete("[v2.0.0]")]
        public string Typeahead { get; set; }

        [DefaultValue(false)]
        public bool HandledTypeahead { get; private set; }

        public IEnumerable<EditorDataItem> GetEditorDataItems(int currentId, int parentId, string typeahead)
        {
            return this.GetEditorDataItems(currentId, typeahead);
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
        /// <param name="contextId"></param>
        /// <returns></returns>
        private IEnumerable<EditorDataItem> GetEditorDataItems(int contextId, string typeahead)
        {
            List<EditorDataItem> editorDataItems = new List<EditorDataItem>();

            Database database = new Database(this.ConnectionString);

            if (database != null)
            {
                string sql = Regex.Replace(this.SqlExpression, "\n|\r", " ")
                             .Replace("@contextId", "@0")
                             .Replace("@typeahead", "@1");
                
                if (this.SqlExpression.Contains("@typeahead")) // WARNING: not a perfect check !
                {
                    this.HandledTypeahead = true;
                }

                editorDataItems = database.Fetch<EditorDataItem>(sql, contextId, typeahead);
            }

            return editorDataItems;
        }
    }
}