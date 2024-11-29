$.ajaxSetup({
    beforeSend: function () {
        ShowLoading();
    },
    complete: function () {
        HideLoading();
    }
});

$(function () {

    //Mostrar limpiar filtro cuando este lleno
    $(".clean-filter").each(function (index, item) {
        if ($(item).prev().val().length > 0) {
            $(this).css({ display: "table-cell" });
        }
    })

    //$('.selectpicker').selectpicker({ liveSearchNormalize: true });

    var myElem = document.getElementById('pagedList');
    if (myElem != null) {
        var str = document.getElementById("pagedList").innerHTML;
        var res = str.replace("Showing items", "Elementos del");
        res = res.replace("through", "al");
        res = res.replace("of", "de");
        document.getElementById("pagedList").innerHTML = res;
    }


    //$("#pagedList .pagination-container .pagination").addClass("pagination-sm");


    
    CreateAutocompletesBootstrap();//CreateAutocompletes();


    var getPage = function () {
        var $a = $(this)

        var options = {
            url: $a.attr("href"),
            data: $("form").serialize(),
            type: "get"
        };

        $.ajax(options).done(function (data) {
            var target = $a.parents("div.pagedList").attr("data-otf-target");
            $(target).replaceWith(data);
        });
        return false;
    };


    //$("body").on("click", ".pagedList a", getPage);
    $(".pagedList a").click(getPage);

    $('.datepicker').datepicker({
        format: "dd/mm/yyyy",
        todayBtn: "linked",
        language: "es",
        autoclose: true,
        todayHighlight: true
    });

    $("#btnCleanFilter").click(function (event)
    {
        event.preventDefault();
        $("input").val("");
        //$("select").val("");
        $("select").prop("selectedIndex", 0);
        $("#sOrder").val("");
        $('select.select2').select2("val", 0);
        ShowLoading();
        $(".clean-filter").css({ display: "none" });
        $("#iFiltros").val(1);
        $("#form0").submit();
    })

    $(".change-checked").click(function()
    {
        var vElementId = $(this).attr("element-id");
        if($(this).hasClass("fa-square-o"))
        {
            $(this).removeClass("fa-square-o").addClass("fa-check-square-o");
            $("#" + vElementId).val(true);
        }
        else
        {
            $(this).removeClass("fa-check-square-o").addClass("fa-square-o");
            $("#" + vElementId).val(false);
        }
    })


    $(".filtrar").each(function (x, y) {
        var vParentElement = $(this).parent();
        if ($(this).val() == "") {
            vParentElement.addClass("noFiltering");
        }
    });

    $(".filtrar")
        .change(function () {
            var vElement = $(this);
            var vPArentElement
            if ($(vElement).val().length > 0) {
                $(vElement).siblings(".clean-filter").css({ display: "table-cell" }).parent().removeClass('noFiltering');       
            }
            else {
                $(vElement).siblings(".clean-filter").css({ display: "none" }).parent().addClass('noFiltering');
             
            }
            //ShowLoading();
            $(vElement).blur();
            setTimeout(function () {
                $("#form0").submit();
            }, 200);
        })
        .keypress(function () {
            if (event.charCode == 13) {
                //ShowLoading();
                ShowLoading();
                $("#form0").submit();
            }
        })
        .keyup(function () {
            if ($(this).val().length > 0) {
                $(this).siblings(".clean-filter").css({ display: "table-cell" }).parent().removeClass('noFiltering');
            }
            else {
                $(this).siblings(".clean-filter").css({ display: "none" }).parent().addClass('noFiltering');
            }
        });

    $(".clean-filter").click(function () {
        var $vElement = $(this).parent().find(".filtrar");
        $vElement.val("").trigger("change");
        if ($vElement.hasClass("select2")) {
            $vElement.select2("val", 0);
        }
    });

    $('.phone-mask').mask('000-000-0000');

    $("#pagedList a[href]").click(function()
    {
        ShowLoading();
        ShowLoading();
    })

    $(".select2").select2({
        width: "100%",
        language: {
            noResults: function () {
                return "No se encontraron resultados.";
            }
        }
    });

    

});

function CreateAutocompletesBootstrap()
{
    $('[data-autocomple]').each(function ()
    {
        //var vUrl = $(this).attr("data-autocomple") + "?term=" + $(this).val();
        //$(this).typeahead({
        //    ajax: {
        //        url: vUrl
        //    }
        //});

       
        var myAccentMap = { "á": "a", "é": "e", "í": "i", "ó": "o", "ú": "u" };
        var vUrl = $(this).attr("data-autocomple");// + "?term=" + $(this).val();
        var vElement = $(this);
        $.get(vUrl, function (data) {
            $(vElement).typeahead({
                source: data,
                autoSelect: false,
                limit: 10
                //updater: function(data)
                //{
                //    return data;
                //}
            });
        }, 'json');
    })
}

function CreateAutocompletes()
{
    var createAutocomplete = function () {
        var $input = $(this);
        var options = {
            source: $input.attr("data-autocomple")
        };
        $input.autocomplete(options);
    };

    $("input[data-autocomple]").each(createAutocomplete)
}

function OrderBy(vElement)
{
    ShowLoading();
    var vOrder = $(vElement).attr("order");
    $(vElement).find("i").toggleClass("");
    $("#sOrder").val(vOrder);
    $("#form0").submit();
}


$(document).ajaxSuccess(function()
{
    //$("#pagedList .pagination-container .pagination").addClass("pagination-sm");
    //CreateAutocompletesBootstrap();//CreateAutocompletes();

    //$(".filtrar").change(function () {
    //    ShowLoading();
    //    $("#form0").submit();
    //})

    //$("#btnCleanFilter").click(function (event) {
    //    event.preventDefault();
    //    $("input").val("");
    //    $("select").val("");
    //    $("#sOrder").val("");
    //    ShowLoading();
    //    $("#form0").submit();
    //})

    $("#pagedList a[href]").click(function () {
        ShowLoading();
        ShowLoading();
    })
})


function MyToast(vTitulo, vMensaje, vTipo, vTiempo)
{
    vTiempo = vTiempo == undefined ? "5000" : vTiempo;
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "progressBar": true,
        "preventDuplicates": true,
        "positionClass": "toast-top-center",//toast-top-right
        "onclick": null,
        "showDuration": "400",
        "hideDuration": "1000",
        "timeOut": vTiempo,
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "slideDown",
        "hideMethod": "slideUp"
    }

    toastr[vTipo](vMensaje, vTitulo);
}

var ShowLoading = function (text = "Cargando...") {

    //console.log(text);

    $(".loader").find("small").html(text);
    $(".loader").removeClass('d-none');
    $(".loader-backdrop").removeClass('d-none');
    $(".loader").addClass('animated fadeIn');
    $(".loader").addClass('animated bounceInUp');

};

var HideLoading = function () {
    setTimeout(function () {
        $(".loader").addClass('animated bounceOutDown');
        $(".loader-backdrop").addClass('animated faceOut');
        setTimeout(function () {
            $(".loader").addClass('d-none');
            $(".loader-backdrop").addClass('d-none');
        }, 200);
    }, 400)
};


//#region bitacora
var vGirarIcono = false;
var vGiro = 0;
var vTiempo = 1;
var vInterval = null;



$(document).on('click', '#iActualizarBitacora', function () {
    vGirarIcono = true;
    vInterval = setInterval(GirarIcono, vTiempo);
    $("#FiltrarBitacora").prop("selectedIndex", 0);
    ShowLoading();
    $("#frmBitacora").submit();
});


$(document).on('change', '#FiltrarBitacora', function () {
    ShowLoading();
    $("#frmBitacora").submit();
});

$(document).on('click', '.open-children', function () {
    var vIcon = $(this).find("i");
    var vBitacoraId = $(this).attr("bitacora-id");
    if (vIcon.hasClass("fa-chevron-right")) {
        vIcon.addClass("fa-chevron-up").removeClass("fa-chevron-right");
        $(".child-" + vBitacoraId).show();
    } else {
        vIcon.addClass("fa-chevron-right").removeClass("fa-chevron-up");
        $(".child-" + vBitacoraId).hide();
    }
});

function GirarIcono() {
    vGiro++;
    $("#iActualizarBitacora").css({ "-ms-transform": "rotate(" + vGiro + "deg)", "-webkit-transform": "rotate(" + vGiro + "deg)", "transform": "rotate(" + vGiro + "deg)" });
}

var interval_id = 9999;
for (var i = 1; i < interval_id; i++)
    window.clearInterval(i);
//#endregion