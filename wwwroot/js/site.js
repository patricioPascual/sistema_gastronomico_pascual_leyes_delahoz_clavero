// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//funcion para Inicizar select2
function inicializarSelect2Ajax($elemento) {
    var urlEndpoint = $elemento.data('url') || '/Producto/BuscarTexto';

    $elemento.select2({
        theme:'bootstrap-5',
        placeholder: 'Buscar producto...',
        allowClear: true,
        width: '100%',
        ajax: {
            url: urlEndpoint,
            dataType: 'json',
            delay: 300,
            data: function (params) {
                return {
                    q: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map(function (item) {
                        return {
                            id: item.id,
                            text: item.texto || item.nombre || item.text, // Mapea la propiedad de C# a Select2
                            precioCosto: item.precioCosto,
                            unidad: item.unidad
                        };
                    })
                };
            },
            cache: true
        }
    });
}