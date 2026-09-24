$(document).ready(function () {
  let filaContador = 0;

  function agregarFilaReceta(datos) {
    datos = datos || {};
    const index = filaContador++;
    const idProducto = datos.idProducto || "";
    const nombreProducto = datos.nombreProducto || "";
    const cantidad = datos.cantidadRequerida || "";
    const unidadMedida = datos.unidadMedida || "-"; // Leemos la unidad o ponemos un guion

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
            <input type="number" step="0.01" min="0.01" class="form-control"
                   name="Receta[${index}].CantidadRequerida" value="${cantidad}">
            <span class="input-group-text label-unidad bg-light">${unidadMedida}</span>
          </div>
        </td>
        <td class="text-end">
          <button type="button" class="btn btn-outline-danger btn-sm btn-quitar-fila">
            <i class="bi bi-x-circle"></i>
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
