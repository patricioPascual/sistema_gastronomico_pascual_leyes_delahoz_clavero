$(document).ready(function () {
  let filaContador = 0;

  function agregarFilaReceta(datos) {
    datos = datos || {};
    const index = filaContador++;
    const idProducto = datos.idProducto || "";
    const nombreProducto = datos.nombreProducto || "";
    const unidadMedida = datos.unidadMedida || "-";

    const cantidad = (datos.cantidadRequerida || "")
      .toString()
      .replace(".", ",");

    const opcionInicial = idProducto
      ? `<option value="${idProducto}" selected>${nombreProducto}</option>`
      : "";

    const fila = $(`
      <tr class="fila-receta">
        <td>
          <input type="hidden" name="Receta.Index" value="${index}">
          <select name="Receta[${index}].IdProducto" class="form-select select2-ajax"
                  data-url="/Producto/BuscarTexto"
                  data-placeholder="Escriba para buscar un insumo...">
            ${opcionInicial}
          </select>
        </td>
        <td>
          <div class="input-group">
            <!-- Usamos type="text" para que el navegador no te bloquee la coma, e inputmode para el celular -->
            <input type="text" inputmode="decimal" class="form-control"
                   name="Receta[${index}].CantidadRequerida" value="${cantidad}" required>
            <span class="input-group-text label-unidad bg-light">${unidadMedida}</span>
          </div>
        </td>
        <td class="text-end">
          <button type="button" class="btn btn-outline-danger btn-sm btn-quitar-fila">
            <i class="bi bi-trash"></i>
          </button>
        </td>
      </tr>
    `);

    $("#tablaReceta tbody").append(fila);

    const $select = fila.find(".select2-ajax");
    inicializarSelect2Ajax($select);

    $select.on("select2:select", function (e) {
      const data = e.params.data;
      if (data && data.unidad) {
        fila.find(".label-unidad").text(data.unidad);
      }
    });
  }

  const recetaInicialData = window.recetaInicial || [];

  if (recetaInicialData.length > 0) {
    recetaInicialData.forEach(agregarFilaReceta);
  } else {
    agregarFilaReceta();
  }

  $("#btnAgregarInsumo").on("click", function () {
    agregarFilaReceta();
  });

  $("#tablaReceta").on("click", ".btn-quitar-fila", function () {
    $(this).closest("tr").remove();
  });

  $("#tablaReceta").on(
    "input",
    "input[name*='CantidadRequerida']",
    function () {
      this.value = this.value.replace(/\./g, ",");
      this.value = this.value.replace(/[^0-9,]/g, "");
    },
  );

  $(window).on("keydown", function (event) {
    if (event.key === "Enter") {
      if (
        event.target.tagName !== "TEXTAREA" &&
        event.target.type !== "submit"
      ) {
        event.preventDefault();
        return false;
      }
    }
  });
});
