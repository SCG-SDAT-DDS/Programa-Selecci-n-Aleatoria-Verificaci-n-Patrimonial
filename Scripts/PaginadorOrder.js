$(function () {
    var ShowLoading = function () {
        $(".wrapper-loader").show();
    }

    var HideLoading = function () {
        setTimeout(function () {
            setTimeout(function () {
                $(".wrapper-loader").hide();
            }, 200);
        }, 400)
    }
    $(document).ajaxComplete(function () {
        //$('[data-toggle="tooltip"]').tooltip();
        $(".td-error").tooltip();
    });
  
});

function changeStatus(span, Url, sid) { 
    ShowLoading();
    //var r = confirm("¿Esta seguro que desea modificar el valor?");
    //if (r) {
    $.ajax({
        type: 'POST',
        url: Url,
        data: {
            id: sid
        },
        success: function (data) {
            //console.log(data);
            if (data == "True") {
                $(span).text("Activo");
                $(span).removeClass("badge-danger").removeClass("label-danger").addClass("badge-primary");
            } else if (data == "False") {
                $(span).text("Inactivo");
                $(span).removeClass("badge-primary").removeClass("label-primary").addClass("badge-danger");
            } else {
                Swal.fire('Error', "Ocurrio un error al momennto de procesar la solicitud, por favor asegúrese que cuente con permisos.", 'error');
            }
            HideLoading();
        }
    });
    //}
}

function changeStatusDynamic(span, Url, iTablaFisicaId, sNombreTabla) {
    ShowLoading();
    //var r = confirm("¿Esta seguro que desea modificar el valor?");
    //if (r) {
    $.ajax({
        type: 'POST',
        url: Url,
        data: {
            TablaFisicaId: iTablaFisicaId,
            NombreTabla: sNombreTabla
        },
        success: function (data) {
            console.log(data);
            if (data == "True") {
                $(span).text("Activo");
                $(span).removeClass("badge-danger").removeClass("label-danger").addClass("badge-primary");
            } else if (data == "False") {
                $(span).text("Inactivo");
                $(span).removeClass("badge-primary").removeClass("label-primary").addClass("badge-danger");
            }
            HideLoading();

        }
    })
    //}
}


$(document).ajaxError(function () {
    console.log('Error');
});

function BeginClient() {
    ShowLoading();
}

function CompleteClient() {
    HideLoading();
}

$.ajaxSetup({
    beforeSend: function () {
        ShowLoading();
    },
    complete: function () {
        HideLoading();
    }
});

function OrderBy(vElement) {
    //$("#ibox-index").children('.ibox-content').addClass('sk-loading');
    var vOrder = $(vElement).attr("order");
    $(vElement).find("i").toggleClass("");
    $("#sOrder").val(vOrder);
    $("#form0").submit();
}

$(".filtrar").change(function () {
    var vElement = $(this);
    /*if ($(vElement).val().length > 0) {
        $(vElement).siblings(".clean-filter").css({ display: "table-cell" });
    }
    else {
        $(vElement).siblings(".clean-filter").css({ display: "none" });
    }*/
    //$("#ibox-index").children('.ibox-content').addClass('sk-loading');
    $(vElement).blur();
    setTimeout(function () {
        $("#form0").submit();
    }, 200);
});



