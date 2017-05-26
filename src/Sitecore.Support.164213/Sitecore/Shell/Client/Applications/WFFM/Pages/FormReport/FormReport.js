define(["sitecore"], function (sc) {

    var self;

    var app = sc.Definitions.App.extend({
        initialized: function () {
            self = this;

            this.FormFieldsBarChart.on("segmentSelected", this.selectSegment);
        },

        exportToExcel: function () {
            this.exportData(1);
        },

        exportToXml: function () {
            this.exportData(0);
        },

        selectSegment: function (obj) {
            self.trigger("close:DetailedFieldsReportSmartPanel");
            var dataSourceItems = self.FormResponsesDataSource.attributes.data;

            var listDetails = dataSourceItems[obj.id];
            if (listDetails == null) {
                return;
            }
            self.FormResponsesDataSource.set("selectedSegmentData", listDetails);

            self.trigger("open:DetailedFieldsReportSmartPanel");
        },

        exportData: function (format) {
            var params = sc.Helpers.url.getQueryParameters(window.location.href);
            var id = this.get("itemId") || params.itemId || this.get("formId") || params.formId || this.get("id") || params.id;

            $.ajax({
                type: 'POST',
                //Sitecore.Support.164213
                url: "/api/sitecore/SupportExportFormData/Export?id=" + id + "&format=" + format,
                //End of changes
                data: JSON.stringify({ "id": id }),
                contentType: 'application/json',
                dataType: 'json',
                success: function (returnValue) {
                    window.location = "/api/sitecore/ExportFormData/Download?file=" + returnValue.File + "&format=" + format;
                }
            });
        }
    });
    return app;
});