using System.Collections.Specialized;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;

namespace SitecoreExtension.HistoricDataLog.Commands
{
    public class DeleteHistoricLogEntryCommand : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(context.Parameters["historicLogIds"], "id");

            var id = context.Parameters["historicLogIds"];

            if (string.IsNullOrEmpty(id))
            {
                SheerResponse.Alert("Select a historic log entry first.");
            }
            else
            {
                var args = new ClientPipelineArgs(new NameValueCollection()
                {
                    ["id"] = id
                });

                ContinuationManager.Current.Start(this, "Run", args);
            }
        }

        public void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.ArgumentNotNull(args.Parameters["id"], "id");

            var id = args.Parameters["id"];

            if (args.IsPostBack)
            {
                if (args.Result != "yes")
                {
                    return;
                }

                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Experience"].ConnectionString))
                {
                    var removeQuery = $@"DELETE FROM ItemHistory WHERE Id = '{id}'";

                    connection.Execute(removeQuery, new
                    {
                        id
                    });
                }

                SheerResponse.SetLocation(string.Empty);
            }
            else
            {
                var text = Translate.Text("Are you sure you want to delete this historic log?");
                SheerResponse.Confirm(text);
                args.WaitForPostBack();
            }
        }
    }
}