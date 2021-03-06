﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Abstractions.Data;
using Raven.Json.Linq;

namespace CommonJobs.Raven.Migrations
{
    public abstract class Migration : CommonJobs.Raven.Migrations.IMigration
    {
        public IDocumentStore DocumentStore { get; set; }

        public abstract void Up();
        public abstract void Down();

        protected bool IndexExists(string index, int pageSize = 64)
        {
            int start = 0;
            while (true)
            {
                var names = DocumentStore.DatabaseCommands.GetIndexNames(start, pageSize);
                if (names.Length == 0)
                    return false;
                else if (names.Contains(index))
                    return true;
                start += pageSize;
            }
        }

        protected void DoInResults(
            Action<RavenJObject> action, 
            string query = "*:*", 
            int pageSize = 64, 
            string index = "Dynamic", 
            string[] includes = null)
        {
            int start = 0;
            while (true)
            {
                var qry = new IndexQuery() 
                { 
                    Query = query, 
                    PageSize = pageSize,
                    Start = start //TODO: consider to use another thing in place of start
                };

                var results = DocumentStore.DatabaseCommands.Query(index, qry, includes);

                if (results.Results.Count == 0)
                    break;

                foreach (var result in results.Results)
                    action(result);

                start += pageSize;
            }
        }
    }
}
