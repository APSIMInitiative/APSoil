/// <reference path="jquery-1.2.3.min.js" />
$(document).ready(function () {
    $.ajax({
        type: "POST",
        url: "RSSReader.asmx/GetRSSReader",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            $('#RSSContent').removeClass('loading');
            $('#RSSContent').html(msg.d);
        }
    });
});