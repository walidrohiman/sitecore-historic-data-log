using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;

namespace SitecoreExtension.HistoricDataLog.Commands
{
    public class AddWatchlistCommand : Command, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            ContinuationManager.Current.Start(this, "Run", new ClientPipelineArgs());
        }

        public void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            if (args.IsPostBack)
            {
                return;
            }
           
            var urlString = new UrlString(UIUtil.GetUri("control:AddWatchlist"));
            SheerResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", string.Empty, true);
            args.WaitForPostBack();
        }
    }
}