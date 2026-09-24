function inicializarSelect2Ajax($select) {
  var ajaxUrl = $select.data("url");
  var placeholderText = $select.data("placeholder");

  $select.select2({
    theme: "bootstrap-5",
    placeholder: placeholderText,
    minimumInputLength: 2,
    ajax: {
      url: ajaxUrl,
      dataType: "json",
      delay: 300,
      data: function (params) {
        return {
          q: params.term,
        };
      },
      processResults: function (data) {
        return {
          results: $.map(data, function (item) {
            return {
              id: item.id,
              text: item.texto,
              unidad: item.unidad,
            };
          }),
        };
      },
      cache: true,
    },
  });

  $select.on("select2:select select2:clear select2:unselecting", function () {
    this.dispatchEvent(new Event("change"));
  });
}

$(document).ready(function () {
  $(".select2-ajax").each(function () {
    inicializarSelect2Ajax($(this));
  });
});
