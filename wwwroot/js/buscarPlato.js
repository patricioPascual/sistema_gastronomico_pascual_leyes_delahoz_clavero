function inicializarSelect2Plato($select, onCambio) {
  var ajaxUrl = $select.data("url");
  var placeholderText =
    $select.data("placeholder") || "Escriba para buscar un plato...";

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
              text:
                item.texto +
                " ($" +
                Number(item.precioCosto ?? 0).toFixed(2) +
                ")",
              precioCosto: item.precioCosto,
            };
          }),
        };
      },
      cache: true,
    },
  });

  $select.on("select2:select select2:clear select2:unselecting", function () {
    this.dispatchEvent(new Event("change"));

    if (typeof onCambio === "function") {
      var seleccionado = $select.select2("data")[0];
      onCambio(
        {
          id: seleccionado ? seleccionado.id : "",
          text: seleccionado ? seleccionado.text : "",
          precioCosto: seleccionado ? seleccionado.precioCosto : 0,
        },
        $select,
      );
    }
  });
}

$(document).ready(function () {
  $(".plato-ajax")
    .not("[data-manual]")
    .each(function () {
      inicializarSelect2Plato($(this));
    });
});
