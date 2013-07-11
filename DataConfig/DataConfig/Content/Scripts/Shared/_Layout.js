/*
** Onload
*/
$(function () {
    $(window).resize(function () {
        if ($(window).width() < 800) {
            $('#banner').hide();
        } else {
            $('#banner').show();
        }
    });
});
