let enumTipoCampo = {
    Texto : 1,
    AreaTexto : 2,
    Numerico : 3,
    Alfanumerico : 4,
    Dinero : 5,
    Porcentaje : 6,
    Decimal : 7,
    Fecha : 8,
    Hora : 9,
    Hipervinculo : 10,
    email : 11,
    Telefono : 12,
    ArchivoAdjunto : 13,
    Catalogo : 14,
    CasillaVerificacion : 15
}

class Row {
    constructor(tipo, tipoText, nombre, conDecimales, etiqueta, longitud, tipoFecha, gpoExtension, size, catalogo, ayuda, requerido) {
        this.tipo = tipo;
        this.tipoText = tipoText;
        this.nombre = nombre;
        this.conDecimales = conDecimales;
        this.etiqueta = etiqueta;
        this.longitud = longitud;
        this.tipoFecha = tipoFecha;
        this.gpoExtension = gpoExtension;
        this.size = size;
        this.catalogo = catalogo;
        this.ayuda = ayuda;
        this.requerido = requerido;
    }
};


$.validator.addMethod("ValidLongitud", function (value, element) {
    var tipoCampo = $("#AddTipoCampo").val();
    switch (tipoCampo) {
        //Longitud
        case "1": case "2": case "3": case "4": case "5": case "6": case "7": case "10": case "11": case "12": case "13":
            if (isEmpty($("#AddLongitud").val())) {
                return false;
            }
            break;
    } 
    return true;
}, "Tiene que agregar una longitud para continuar.");

$.validator.addMethod("ValidarFecha", function (value, element) {
    var tipoCampo = $("#AddTipoCampo").val();
    switch (tipoCampo) {
        //Longitud
        case "8":
            if (isEmpty($("#AddTipoFecha").val())) {
                return false;
            }
            break;
    }
    return true;
}, "Tiene que seleccionar un tipo de fecha para continuar.");

$.validator.addMethod("ValidatExtension", function (value, element) {
    var tipoCampo = $("#AddTipoCampo").val();
    switch (tipoCampo) {
        //Longitud
        case "13":
            if (isEmpty($("#AddGrupoExtensionId").val())) {
                return false;
            }
            break;
    }
    return true;
}, "Tiene que seleccionar un tipo de extensión para continuar.");

$.validator.addMethod("ValidarSize", function (value, element) {
    var tipoCampo = $("#AddTipoCampo").val();
    switch (tipoCampo) {
        //Longitud
        case "13":
            if (isEmpty($("#AddSize").val())) {
                return false;
            }
            break;
    }
    return true;
}, "Tiene que capturar un tamaño máximo para el archivo para continuar.");

$.validator.addMethod("ValidarCatalogos", function (value, element) {
    var tipoCampo = $("#AddTipoCampo").val();
    switch (tipoCampo) {
        //Longitud
        case "14":
            if (isEmpty($("#AddCatalogoId").val())) {
                return false;
            }
            break;
    }
    return true;
}, "Tiene que seleccionar un catálogo para continuar para continuar.");

function isEmpty(value) {
    return (value == null || value.length === 0);
}

$('#AddTipoFecha').change(function () {
    $('label[for=ValidarFecha]').remove();
});

$('#AddGrupoExtensionId').change(function () {
    $('label[for=ValidatExtension]').remove();
});

$('#AddLongitud').on('input', function (e) {
    if ($(this).val() != "") {
        $('label[for=ValidLongitud]').hide();
    } else {
        $('label[for=ValidLongitud]').show();
    }
});

$('#AddSize').on('input', function (e) {
    if ($(this).val() != "") {
        $('label[for=ValidarSize]').hide();
    } else {
        $('label[for=ValidarSize]').show();
    }
});


$(document).ready(function () {
    //RULES VALIDATES
    // Validar Formulario y Submit donde insertar,modificas, e inactivas
    $("#frmAgregarCampo").validate({
        ignore: "",
        rules: {
            AddTipoCampo: {
                required: true
            },
            AddNombre: {
                required: true
            },
            AddEtiqueta: {
                required: true
            },
            ValidLongitud: {
                ValidLongitud: true,
            },
            ValidarFecha: {
                ValidarFecha: true
            },
            ValidatExtension: {
                ValidatExtension: true
            },
            AddLongitud: {
                number: true
            },
            AddSize: {
                number: true
            },
            ValidarSize: {
                ValidarSize: true,

            },
            ValidarCatalogos: {
                ValidarCatalogos: true
            }
        },
        messages: {
            AddTipoCampo: {
                required: "Este campo es obligatorio."
            },
            AddNombre: {
                required: "Este campo es obligatorio."
            },
            AddEtiqueta: {
                required: "Este campo es obligatorio."
            },
            AddLongitud: {
                number: "Este campo solo acepta números."
            },
            AddSize: {
                number: "Este campo solo acepta números."
            }
        }
    });
    $("#noceform").validate({
        ignore: "",
        rules: {
            AddTipoCampo2: {
                required: true
            }
        },
        messages: {
            AddTipoCampo2: {
                required: "Este campo es obligatorio."
            }
        }
    });
    
    let tabla = document.getElementById("tblCampos");
    let divLongitud = document.getElementById("divLongitud");
    let divTipoFecha = document.getElementById("divTipoFecha");
    let divGpoExtensiones = document.getElementById("divGpoExtensiones");
    let divSize = document.getElementById("divSize");
    let divCatalogos = document.getElementById("divCatalogos");
    let divPorcentaje = document.getElementById("divPorcentaje");
    let divPrincipal = document.getElementById("divPrincipal");

    function initCampos() {
        divLongitud.style.display = "block";
        divTipoFecha.style.display = "none";
        divGpoExtensiones.style.display = "none";
        divCatalogos.style.display = "none";
        divTipoFecha.style.display = "none";
        divSize.style.display = "none";
        divPorcentaje.style.display = "none";
    }

    initCampos();

    $('#AddTipoCampo').change(function () {
        initCampos();
        var tipo = document.getElementById("AddTipoCampo").value;
        $("#frmAgregarCampo").validate().resetForm();
        setCampos(tipo)
    });

    $(document).on('click', '.elimina', function () {
        var _this = $(this);
        RemoveRow(_this);
    });

    function agregarCampo() {
        //TODO Validaciones
        let tipo = document.getElementById("AddTipoCampo");
        let tipoText = tipo.options[tipo.selectedIndex].text;
        let nombre = document.getElementById("AddNombre").value;
        let conDecimales = $("#AddConDecimales").iCheck('update')[0].checked;
        let etiqueta = document.getElementById("AddEtiqueta").value;
        let longitud = document.getElementById("AddLongitud").value
        let tipoFecha = document.getElementById("AddTipoFecha");
        let tipoFechaVal = tipoFecha.options[tipoFecha.selectedIndex].value;
        let gpoExtensiones = document.getElementById("AddGrupoExtensionId");
        let gpoExtensionesVal = gpoExtensiones.options[gpoExtensiones.selectedIndex].value;
        let size = document.getElementById("AddSize").value;
        let catalogo = document.getElementById("AddCatalogoId")
        let catalogoId = catalogo.options[catalogo.selectedIndex].value;
        let requerido = $("#AddRequerido").iCheck('update')[0].checked;
        let ayuda = document.getElementById("AddAyuda").value;

        var objRow = new Row(tipo.value, tipoText, nombre, conDecimales, etiqueta, longitud, tipoFechaVal, gpoExtensionesVal, size, catalogoId, ayuda, requerido);
        AddRow(objRow);
        $('#frmAgregarCampo')[0].reset();
        $("#AddRequerido").iCheck('uncheck');
    }

    function AddRow(row) {
        const _TIPO = +row.tipo;

        //Creamos Row
        let tbodyRef = document.getElementById('bodyTable');
        let newRow = tbodyRef.insertRow(tbodyRef.rows.length);

        //Creamos Cell Tipo
        let cellTipo = newRow.insertCell(newRow.cells.length);
        cellTipo.appendChild(document.createTextNode(row.tipoText));
        cellTipo.appendChild(getHiddenValue(_TIPO, 'TipoCampo', row.tipo));

        //Creamos Cell Nombre
        let cellNombre = newRow.insertCell(newRow.cells.length);
        cellNombre.appendChild(document.createTextNode(row.nombre));
        cellNombre.appendChild(getHiddenValue(_TIPO, 'Nombre', row.nombre));

        //Con Decimales (hidden)
        cellNombre.appendChild(getHiddenValue(_TIPO, 'ConDecimales', row.conDecimales));

        //Creamos Cell Etiqueta
        let cellEtiqueta = newRow.insertCell(newRow.cells.length);
        cellEtiqueta.appendChild(document.createTextNode(row.etiqueta));
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'Etiqueta', row.etiqueta));

        //Longitud (hidden)
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'Longitud', row.longitud));

        //TipoFecha (hidden)
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'TipoFecha', row.tipoFecha));

        //GrupoExtensiones
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'GrupoExtensionId', row.gpoExtension));

        //Size
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'Size', row.size));

        //CatalogoId
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'CatalogoId', row.catalogo));

        //Ayuda
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'Ayuda', row.ayuda));

        //Requerido
        cellEtiqueta.appendChild(getHiddenValue(_TIPO, 'Requerido', row.requerido));
        
        let btnElimina = createInput('button', 'btn btn-white btn-xs elimina', null, 'Remover Campo')

        let cellAcciones = newRow.insertCell(newRow.cells.length);
        cellAcciones.appendChild(btnElimina);

    }

    function RemoveRow(obj) {
        obj.closest('tr').remove();
    }

    function getHiddenValue(tipoCampoEnum, id, value) {

        switch (id) {
            case 'TipoCampo':
            case 'Nombre':
            case 'Etiqueta':
            case 'Ayuda':
            case 'Requerido':
                return createInput('hidden', null, id, value);

            case 'ConDecimales':
                let valConDec = tipoCampoEnum == enumTipoCampo.Porcentaje ? value : false;
                return createInput('hidden', null, id, valConDec);

            case 'Longitud':
                var valLongitud = "0";
                if (tipoCampoEnum != enumTipoCampo.Porcentaje && tipoCampoEnum != enumTipoCampo.Fecha && tipoCampoEnum != enumTipoCampo.Hora
                    && tipoCampoEnum != enumTipoCampo.CasillaVerificacion && tipoCampoEnum != enumTipoCampo.Catalogo) {
                    valLongitud = value;
                } else {
                    valLongitud = "0";
                }
                return createInput('hidden', null, id, valLongitud);

            case 'TipoFecha':
                var valTipoFecha = tipoCampoEnum == enumTipoCampo.Fecha ? value : "0"
                return createInput('hidden', null, id, valTipoFecha);

            case 'GrupoExtensionId':
                var valGpoExt = tipoCampoEnum == enumTipoCampo.ArchivoAdjunto ? value :"0";
                return createInput('hidden', null, id, valGpoExt);

            case 'Size':
                var valSize = tipoCampoEnum == enumTipoCampo.ArchivoAdjunto ? value : "0";
                return createInput('hidden', null, id, valSize);

            case 'CatalogoId':
                var valCatalogo = tipoCampoEnum == enumTipoCampo.Catalogo ? value : "0";
                return createInput('hidden', null, id, valCatalogo);

            default:
                return createInput('hidden', null, 'Err', '0');
        }
    }

    function createInput(type, clase, id, value) {

        let input = document.createElement('input');

        if (type != null) {
            let attType = document.createAttribute('type');
            attType.value = type;
            input.setAttributeNode(attType);
        }

        if (clase != null) {
            let attClass = document.createAttribute('class');
            attClass.value = clase;
            input.setAttributeNode(attClass);
        }

        if (id != null) {
            let attId = document.createAttribute('id');
            let attName = document.createAttribute('name');
            attId.value = id;
            attName.value = id;
            input.setAttributeNode(attId);
            input.setAttributeNode(attName);
        }
        
        let attValue = document.createAttribute('value');
        attValue.value = value;
        input.setAttributeNode(attValue);

        return input;
    }

    function setCampos(tipo) {
        switch (+tipo) {
            case enumTipoCampo.Texto:
            case enumTipoCampo.AreaTexto:
            case enumTipoCampo.Numerico:
            case enumTipoCampo.Alfanumerico:
            case enumTipoCampo.Dinero:
            case enumTipoCampo.Decimal:
            case enumTipoCampo.Hipervinculo:
            case enumTipoCampo.email:
            case enumTipoCampo.Telefono:
                initCampos();
                break;

            case enumTipoCampo.Porcentaje:
                porcentajeConfig();
                break;
            case enumTipoCampo.Fecha:
                fechaConfig();
                break;
            case enumTipoCampo.Hora:
                clearLongitud();
                break;
            case enumTipoCampo.ArchivoAdjunto:
                adjuntoConfig();
                break;
            case enumTipoCampo.Catalogo:
                catalogoConfig();
                break;
            case enumTipoCampo.CasillaVerificacion:
                clearLongitud();
                break;
            default:
                initCampos();
                break;
        }
    }

    function clearLongitud() {
        initCampos();
        if (divLongitud != null)
            divLongitud.style.display = "none";
    }
    function clearPrincipal() {
        if (divPrincipal != null)
            divPrincipal.style.display = "none";
    }

    function fechaConfig() {
        clearLongitud();

        if (divTipoFecha != null)
            divTipoFecha.style.display = "block";
    }

    function adjuntoConfig() {
        initCampos();
        clearLongitud();
        clearPrincipal();

        if (divSize != null)
            divSize.style.display = "block";
        
        if (divGpoExtensiones != null)
            divGpoExtensiones.style.display = "block";
    }

    function catalogoConfig() {
        clearLongitud();

        if (divCatalogos != null)
            divCatalogos.style.display = "block";
    }

    function porcentajeConfig() {
        //clearLongitud();
        //nvarchar(max)
        if (divPorcentaje != null)
            divPorcentaje.style.display = "block";
    }
});
