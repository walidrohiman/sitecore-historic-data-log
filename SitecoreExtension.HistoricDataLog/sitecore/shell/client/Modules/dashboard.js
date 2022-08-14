define(["sitecore", "jquery"], function (sitecore, $) {
    var app = sitecore.Definitions.App.extend({
        initialized: function () {

            this.getItemList();
            
            this.DeleteButton.on("click", this.removeRecord, this);
            this.RefreshButton.on("click", this.getItemList, this);
        },
        getItemList: function () {
			var app = this;

			app.ProgressIndicator.set("isBusy", true);
			app.ProgressIndicator.set("IsVisible", true);
			
			$.ajax({
				type: "GET",
				dataType: "json",
				url: "/api/dashboard/gethistoriclogitems",
				cache: false,
				success: function (data) {
					app.ItemList.set("items", data.ItemInformations);
				},
				error: function () {
					console.log("There was an error. Try again please!");
				},
				complete: function (){
					app.ProgressIndicator.set("isBusy", false);
					app.ProgressIndicator.set("IsVisible", false);
				}
			});
		},
        removeRecord: function () {
            var app = this;
			
			var test = app.ItemList.attributes.checkedItems;
			console.log(JSON.stringify(test));
            $.ajax({
                type: "POST",
                dataType: "json",
                data: { selectedItems: app.ItemList.attributes.checkedItems },
                url: "/api/dashboard/removehistoriclogitems",
                cache: false,
                success: function (data) {
                    if (data === "success") {
                        alert("Historic Log Items deleted");
                    } 
					if(data === "noItem"){
						alert("No record selected!");
					}
					else {
                        alert(data);
                    }
                },
                error: function () {
                    console.log("There was an error. Try again please!");
                },
                complete: function () {
                    app.ProgressIndicator.set("isBusy", false);
                    app.ProgressIndicator.set("IsVisible", false);
					app.getItemList();
                }
            });
        }
    });
    return app;
});