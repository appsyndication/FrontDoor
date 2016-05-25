$(function () {
   $('.search-toggle').on('click', function () {
    var target = $(this).data("target");
    var focusTarget = $(this).data("focus-target");
    $(target).toggleClass('visible-xs');
    $(focusTarget).val('').focus();
  });

   $('.dropdown-menu').find('form').click(function(e) {
      e.stopPropagation();
    });

  $('tr[data-href]').on("click", function() {
    document.location = $(this).data('href');
  });

  $('.highlight-target').on('click', function () {
    var target = $(this).data("highlight-target");
    $(target).select();
  });

  $('.highlight-text').focus(function () {
    $(this).select().one('mouseup', function (e) {
      $(this).off('keyup');
      e.preventDefault();
    }).one('keyup', function () {
      $(this).select().off('mouseup');
    });
  });
});
