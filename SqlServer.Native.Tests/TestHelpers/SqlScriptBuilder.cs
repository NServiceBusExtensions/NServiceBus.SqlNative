using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

public class SqlScriptBuilder
{
    bool storedProcedures;
    bool tables;
    bool views;
    Func<string, bool> include;

    public SqlScriptBuilder(bool storedProcedures = false, bool tables = false, bool views = false, params string[] namesToInclude)
    {
        this.storedProcedures = storedProcedures;
        this.tables = tables;
        this.views = views;
        if (namesToInclude == null || !namesToInclude.Any())
        {
            include = s => true;
        }
        else
        {
            include = s => namesToInclude.Contains(s);
        }
    }

    public string BuildScript(SqlConnection sqlConnection)
    {
        var builder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
        var theServer = new Server(new ServerConnection(sqlConnection));
        var database = theServer.Databases[builder.InitialCatalog];
        return string.Join("\n\n", GetScripts(database));
    }

    IEnumerable<string> GetScripts(Database database)
    {
        foreach (var scriptable in GetScriptingObjects(database))
        {
            if (((dynamic)scriptable).IsSystemObject)
            {
                continue;
            }

            yield return Script(scriptable);
        }
    }

    string Script(IScriptable scriptable)
    {
        var options = new ScriptingOptions
        {
            ChangeTracking = true,
        };
        return string.Join("\n\n", scriptable.Script(options)
            .Cast<string>()
            .Where(ShouldInclude));
    }

    IEnumerable<IScriptable> GetScriptingObjects(Database database)
    {
        if (tables)
        {
            foreach (Table table in database.Tables)
            {
                if (include(table.Name))
                {
                    yield return table;
                }
            }
        }

        if (views)
        {
            foreach (View view in database.Views)
            {
                if (include(view.Name))
                {
                    yield return view;
                }
            }
        }

        if (storedProcedures)
        {
            foreach (StoredProcedure procedure in database.StoredProcedures)
            {
                if (include(procedure.Name))
                {
                    yield return procedure;
                }
            }
        }
    }

    static bool ShouldInclude(string script)
    {
        if (script == "SET ANSI_NULLS ON")
        {
            return false;
        }

        if (script == "SET QUOTED_IDENTIFIER ON")
        {
            return false;
        }

        return true;
    }
}