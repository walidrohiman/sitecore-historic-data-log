function OnResize() {
 
  if (typeof (Items) != 'undefined') {
    Items.render();
  }

  // Some Component Art styles breaks layout.
  $$(".Grid td").each(function (e) {
    //id ends with "_Items_dom"
    if (e.id.lastIndexOf("_Items_dom") + "_Items_dom".length == e.id.length) {
      e.style.height = "";
      e.style.width = "";
    }
  });
  /* re-render again after some "magic amount of time" - without this second re-render grid doesn't pick correct width sometimes */
  setTimeout("Items.render()", 150);

  if (jQuery("#versionsGrid tbody")[0].initialized) {
    jQuery("#versionsGrid").trigger('tableresize');
  }

  fixVersionsContainerForIE();
}

function scOnGridLoad(scHandler) {
  scHandler.add_itemSelect(itemsOnItemSelect);
  scHandler.add_itemUnSelect(itemsOnItemUnSelect);
}


function fixVersionsContainerForIE() {
  if (scForm.browser.IE) {
    var container = $('tablesorterBodyContainer');
    container.style.height = parseInt(container.style.height) - 15 + "px";
  }
}

window.onresize = OnResize;
Event.observe(window, "load", OnResize);
Event.observe(window, "load", OnLoad);

function OnLoad() {
  setTimeout(function () { window != window.parent && (window.parent.frameElement.style.width = window.parent.frameElement.clientWidth + 20 + 'px'); }, 200);
}

function showVersionsBox() {
  if (typeof (Items) == 'undefined') {
    setTimeout(showVersionsBox, 100);
    return;
  }
  cleanupVersions();
  $("VersionsGridContainer").removeClassName('hidden');
  Items.unSelectAll();
  // We have own logic of storing selection that is called for itemSelect and itemUnSelect.
  // Unfortunately unSelectAll method does not trigger itemUnSelect, so need to do next line manually.
  Items.scHandler.updateSelection();
  Items.AllowMultipleSelect = 0;
  OnResize();
}

function hideVersionsBox() {
  if (typeof (Items) == 'undefined') {
    setTimeout(showVersionsBox, 100);
    return;
  }

  $("VersionsGridContainer").addClassName('hidden');
  Items.AllowMultipleSelect = 1;
  OnResize();
}

function renderVersions(versions) {
  try {
    versions = versions.evalJSON();
  }
  catch (err) {
    return;
  }
  
  var tableBody = jQuery("#versionsGrid tbody")[0];
	versions.each(function (version) {
	  jQuery(tableBody).append("<tr class=\"Row\"><td class='languageColumn'>" + version.fieldName + 
	  "</td><td class='versionColumn'>" + version.oldValue + 
	  "</td></td><td class='dateColumn'>" + version.NewValue + "</td></tr>");   
  });

  var rowsNum = jQuery("#versionsGrid tbody tr") ? jQuery("#versionsGrid tbody tr").length : 0;

  if (tableBody.initialized && rowsNum > 0) {
    jQuery("#versionsGrid").trigger("updateTable")
    .trigger("sorton", [jQuery("#versionsGrid").get(0).config.sortList])
    .trigger("appendCache")
    .trigger("applyWidgets")
    .trigger("tableresize");
  }
  else {
      jQuery.tablesorter.defaults.widgets = ['select', 'scrollable'];

      jQuery.tablesorter.addParser({
        id: "customDate",
        is: function (s) {
          return s.match(/^[0-9]{1,2}\/[0-9]{1,2}\/[0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2} [AM|PM]$/);
        },
        format: function (s) {
          return new Date(s).getTime();
        },
        type: "numeric"
      });

      jQuery("#versionsGrid").tablesorter({
        headers: {
            3: { sorter:'customDate' }
          }
        });

        jQuery("#versionsGrid").bind('select.tablesorter.select', function (e, ts) {
          var selectedIds = "";
          jQuery.each(ts.selected, function (index, value) {
            selectedIds = selectedIds + ";" + value.getAttribute("versionId");
          });

          $("selectedVersions").value = selectedIds.substring(1);
        });

      tableBody.initialized = true;
    }
    fixVersionsContainerForIE();
}

function registerTranslations(translationsJSON){
  try {
    var translations = translationsJSON.evalJSON();
  }
  catch (err) {
    return;
  }

  if (!Object.isArray(translations)) {
    translations = [translations];
  }

  translations.each(function (phrase) {
      scForm.registerTranslation(phrase.key, phrase.value);
  });
}

function itemsOnItemUnSelect(sender) {
  cleanupVersions();
}

function itemsOnItemSelect(sender) {
  cleanupVersions();
  if ($("VersionsGridContainer").hasClassName("hidden")) {
    return true;
  }

  var keys = Items.getSelectedKeys();
  
  var stringKeys = "";
  keys.each(function (key) {
    stringKeys = stringKeys + ";" + key;
  });

if (stringKeys.length == 0) {
    return;
 }
scForm.postRequest("", "", "", "GetFieldsInfo(\"" + stringKeys.substring(1) + "\")", renderVersions, true);
  //scForm.postRequest("", "", "", "GetVersions(\"" + stringKeys.substring(1) + "\")", renderVersions, true);
  return true;
}

function getOuterHeight(element) {
    var layout = element.getLayout();
    var height = Number(layout.get("height"));
    var padding = Number(layout.get("padding-top")) + Number(layout.get("padding-bottom"));
    var margin = Number(layout.get("margin-top")) + Number(layout.get("margin-bottom"));
    var border = Number(layout.get("border-top")) + Number(layout.get("border-bottom"));

    return height + padding + margin + border;
}

function cleanupVersions() {
  $("selectedVersions").value = "";

  if (typeof (jQuery("#versionsGrid")[0].config) != "undefined") {
    var selectionConfig = jQuery("#versionsGrid")[0].config.select;
    if (typeof (selectionConfig) != "undefined") {
      selectionConfig.prevId = -1;
      selectionConfig.selection = [];
    }
  }

  if (typeof($$('#versionsGrid tbody tr')) != "undefined") {
    $$('#versionsGrid tbody tr').each(function (e) { e.remove(); });
  }
}

setInterval(function () {
  var searchBox = document.querySelector("[id$=searchBox]");
  //console.log("search: "+ searchBox.value);
  if (searchBox && searchBox.value.indexOf('\"') != -1) {
    searchBox.value = searchBox.value.replace(/"/g, "");
  };
}, 50);