define(["sitecore", "jquery"], function (sitecore, $) {
	var app = sitecore.Definitions.App.extend({
		initialized: function () {
			this.getConfiguredWatchlists();
			this.getConfiguredDeleteHistory();
			
			this.WatchlistSaveBtn.on("click", this.triggerSaveWatchlist, this);
			this.SaveDatabaseBtn.on("click", this.triggerSaveDeleteHistory, this);
			
			this.RemoveWatchlistBtn.on("click", this.triggerDeleteWatchlist, this);
		},
		getConfiguredWatchlists: function () {
			var app = this;
			
			app.WatchlistProgressIndicator.set("isbusy", true);
			app.WatchlistProgressIndicator.set("isvisible", true);

			$.ajax({
				type: "post",
				datatype: "json",
				url: "/api/configuration/watchlist/get",
				cache: false,
				success: function (data) {
					app.ConfiguredWatchlistList.set("items", data.Watchlists);		
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.WatchlistProgressIndicator.set("isbusy", false);
					app.WatchlistProgressIndicator.set("isvisible", false);
				}
			});
		},
		getConfiguredDeleteHistory: function () {
			var app = this;
			
			app.WatchlistProgressIndicator.set("isbusy", true);
			app.WatchlistProgressIndicator.set("isvisible", true);

			$.ajax({
				type: "post",
				datatype: "json",
				url: "/api/configuration/deletehistory/get",
				cache: false,
				success: function (data) {
					app.DatabaseList.set("items", data.NumberofDays);		
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.WatchlistProgressIndicator.set("isbusy", false);
					app.WatchlistProgressIndicator.set("isvisible", false);
				}
			});
		},
		triggerSaveWatchlist: function () {
			var app = this;
			
			app.WatchlistProgressIndicator.set("isbusy", true);
			app.WatchlistProgressIndicator.set("isvisible", true);
			
			var selectedItems = app.WatchlistItemTreeView.attributes.checkedItemIds;
			
			$.ajax({
				type: "post",
				datatype: "json",
				data: { selectedPaths: selectedItems },
				url: "/api/configuration/watchlist/save",
				cache: false,
				success: function (data) {
					app.getConfiguredWatchlists();
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.WatchlistProgressIndicator.set("isbusy", false);
					app.WatchlistProgressIndicator.set("isvisible", false);
				}
			});
		},
		triggerSaveDeleteHistory: function () {
			var app = this;
			
			app.WatchlistProgressIndicator.set("isbusy", true);
			app.WatchlistProgressIndicator.set("isvisible", true);
					
			var num = app.SourceDbTextBox.attributes.text;
			
			$.ajax({
				type: "post",
				datatype: "json",
				data: { number: num },
				url: "/api/configuration/deletehistory/save",
				cache: false,
				success: function (data) {
					app.getConfiguredDeleteHistory();
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.WatchlistProgressIndicator.set("isbusy", false);
					app.WatchlistProgressIndicator.set("isvisible", false);
				}
			});
		},
		
		triggerDeleteWatchlist: function () {
			var app = this;
			
			app.WatchlistProgressIndicator.set("isbusy", true);
			app.WatchlistProgressIndicator.set("isvisible", true);

			$.ajax({
				type: "post",
				datatype: "json",
				data: { selectedItems: app.ConfiguredWatchlistList.attributes.checkedItems },
				url: "/api/configuration/watchlist/delete",
				cache: false,
				success: function (data) {
					app.getConfiguredWatchlists();
				},
				error: function () {
					console.log("there was an error. try again please!");
				},
				complete: function (){
					app.WatchlistProgressIndicator.set("isbusy", false);
					app.WatchlistProgressIndicator.set("isvisible", false);
				}
			});
		}
	});
    return app;
});