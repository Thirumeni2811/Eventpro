$(document).ready(function () {
    $('.hamburger').click(function (e) {
        if (window.innerWidth <= 1024) {
            e.stopPropagation();
            $('.topics').slideToggle();
        }
    });

    $(document).click(function () {
        if (window.innerWidth <= 1024) {
            $('.topics').slideUp();
        }
    });
});
